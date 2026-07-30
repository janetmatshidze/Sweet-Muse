---
name: background-worker
description: >
  Guides implementation of a long-running task as a Neo background worker.
  Use when you need to offload work that would otherwise time out an HTTP request —
  such as external API calls, multi-step provisioning, or bulk processing —
  and optionally push a real-time result back to the requesting browser tab via SignalR.
---

# Neo Background Worker — Long-Running Task Pattern

Use this pattern when a task is too slow to run synchronously in an HTTP handler.
The API endpoint starts the work and returns immediately; a background worker picks up the
order and executes it. When finished, the worker can push the result back to the user's
browser using Neo's notification `SendProcessMessageAsync`.

---

## How It Works

1. **API endpoint** saves any "pending" state to the database, then enqueues a work order.
2. **Background worker** is woken up by Neo's `QueueHostedService`, resolves dependencies
   from DI (scoped), executes the task, and saves the result.
3. **Optional push notification** — the worker calls `notificationService.SendProcessMessageAsync`
   to deliver the result back to the requesting user's browser via SignalR.
4. **Frontend ViewModel** subscribes to the process key via `ServerMessageSubscriber` and
   handles the incoming message (update UI, show toast, etc.).

### Isolated vs Shared Queue

By default all workers share a single `QueueHostedService`. Implementing
`IIsolatedBackgroundWorker` (in addition to `IBackgroundWorker<,>`) gives the worker its
own dedicated `QueueHostedService<TOrder, TWorker>` hosted service and its own isolated
queue. Use `IIsolatedBackgroundWorker` when you need either of the following:

- **Startup recovery** — the isolated hosted service calls `StartAsync` on the worker once,
  when the application starts. Use it to directly process any tasks that were in-progress
  when the app last shut down or crashed. `StartAsync` runs on the background thread (not
  blocking application startup), so the HTTP server is available immediately. Any new work
  orders enqueued during recovery are held in the isolated queue and processed once recovery
  finishes.
- **Queue isolation** — long-running orders for one worker type should not block orders
  queued for a different worker type.

---

## Backend

> **Multi-tenancy** — sections marked `// [multi-tenancy]` throughout the code below are
> only required if the project uses Neo multi-tenancy (`ITenantService` / `ITenantEntity`).
> Omit them (and the `Neo.Model.MultiTenancy` using) if the project is single-tenant.

### Step 1 — Create the Background Work File

Create a single `static` class that contains the work **Order**, the **Worker**, and an
extension method on `IBackgroundTaskQueue` to enqueue it cleanly. Convention: name the file
`{Feature}BackgroundWork.cs`.

The example below also implements `IIsolatedBackgroundWorker` to get a dedicated queue and
startup-recovery behaviour. If you do not need either, implement only `IBackgroundWorker<,>`.

```csharp
namespace MyApp.Core.App.Services.MyFeature
{
  using System;
  using System.Linq;
  using System.Threading;
  using System.Threading.Tasks;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Logging;
  using MyApp.Core.Models;
  using Neo.BackgroundWork;
  using Neo.Extensions;
  using Neo.Identity;
  using Neo.Model.Exceptions;
  using Neo.Model.MultiTenancy; // [multi-tenancy]
  using Neo.NotificationServer.Models;
  using Neo.NotificationServer.Services;

  /// <summary>
  /// Background work for processing a long-running task.
  /// </summary>
  public static class LongRunningTaskBackgroundWork
  {
    /// <summary>
    /// Extensions on <see cref="IBackgroundTaskQueue"/>.
    /// </summary>
    /// <param name="queue">The queue instance.</param>
    extension(IBackgroundTaskQueue queue)
    {
      /// <summary>
      /// Queues a long-running task work order.
      /// </summary>
      /// <param name="tenantId">The tenant to run the work under.</param>  // [multi-tenancy]
      /// <param name="entityId">The ID of the entity to process.</param>
      /// <param name="requestedByUser">The user who triggered the request.</param>
      public void QueueLongRunningTask(int tenantId, int entityId, User requestedByUser) // [multi-tenancy]: remove tenantId if single-tenant
      {
        queue.Queue(Order.ForEntity(tenantId, entityId, requestedByUser));
      }
    }

    /// <summary>
    /// Work order carrying the data needed to perform the task.
    /// Use the static factory methods to construct the appropriate order type.
    /// </summary>
    public class Order : IBackgroundWorkOrder<Order, Worker>
    {
      public int TenantId { get; private set; } // [multi-tenancy]
      public int EntityId { get; private set; }

      /// <summary>
      /// Capture the full user object (not just the ID) so the worker can
      /// restore the user context and send the push notification to the correct recipient.
      /// </summary>
      public User? RequestedByUser { get; private set; }

      /// <summary>
      /// When true, the worker scans the database for all incomplete tasks and
      /// re-processes them. Used exclusively by <see cref="Worker.StartAsync"/> for
      /// crash recovery on application startup.
      /// </summary>
      public bool AllIncomplete { get; private set; }

      private Order() { }

      /// <summary>Creates a normal work order for a single entity.</summary>
      public static Order ForEntity(int tenantId, int entityId, User requestedByUser) // [multi-tenancy]: remove tenantId if single-tenant
      {
        return new Order
        {
          TenantId = tenantId, // [multi-tenancy]
          EntityId = entityId,
          RequestedByUser = requestedByUser,
        };
      }

      /// <summary>
      /// Creates a startup-recovery order. The worker will query the database for
      /// all entities whose processing was started but never completed.
      /// </summary>
      public static Order AllIncompleteItems()
      {
        return new Order { AllIncomplete = true };
      }
    }

    /// <summary>
    /// Worker that processes long-running task orders.
    /// Implements <see cref="IIsolatedBackgroundWorker"/> to get a dedicated queue and
    /// automatic startup recovery via <see cref="StartAsync"/>.
    /// </summary>
    public class Worker(
      AppDbContext dbContext,
      MyEntityQueryService queryService,
      INotificationService notificationService,
      ITenantService tenantService,               // [multi-tenancy]
      IOverridableUserResolver<User> userResolver,
      ILogger<Worker> logger) : IBackgroundWorker<Order, Worker>, IIsolatedBackgroundWorker
    {
      /// <summary>
      /// Called once by the framework when the application starts, on the background thread.
      /// Directly processes any tasks that were in-progress when the app last shut down or crashed.
      /// New work orders enqueued during this recovery are held in the isolated queue and
      /// will be processed once this method returns.
      /// </summary>
      public Task StartAsync(CancellationToken cancellationToken)
      {
        return this.DoWork(Order.AllIncompleteItems(), cancellationToken);
      }

      /// <inheritdoc/>
      public async Task DoWork(Order order, CancellationToken cancellationToken)
      {
        if (order.AllIncomplete)
        {
          // Query across all tenants — tenant filter must be bypassed here because
          // this path runs before any tenant context is established.
          var incompleteItems = await (
            from entity in dbContext.MyEntities.IgnoreTenantFilter() // [multi-tenancy]: use dbContext.MyEntities directly if single-tenant
            join requestedByUser in dbContext.Users
              on entity.RequestedByUserId equals requestedByUser.UserId
            where entity.ProcessingStartedOn != null && entity.ProcessingCompletedOn == null
            select new
            {
              entity.EntityId,
              TenantId = EF.Property<int>(entity, "TenantId"), // [multi-tenancy]
              RequestedByUser = requestedByUser,
            }).ToListAsync(cancellationToken);

          foreach (var item in incompleteItems)
          {
            await this.ProcessItemAsync(
              Order.ForEntity(item.TenantId, item.EntityId, item.RequestedByUser), cancellationToken); // [multi-tenancy]: remove item.TenantId if single-tenant
          }
        }
        else
        {
          await this.ProcessItemAsync(order, cancellationToken);
        }
      }

      private async Task ProcessItemAsync(Order order, CancellationToken cancellationToken)
      {
        // [multi-tenancy]: wrap in RunWithOverrideTenantIdAsync only if multi-tenanted.
        // Single-tenant projects call userResolver.RunWithOverrideUserAsync directly.
        await tenantService.RunWithOverrideTenantIdAsync(order.TenantId, async () =>  // [multi-tenancy]
        {                                                                               // [multi-tenancy]
          await userResolver.RunWithOverrideUserAsync(order.RequestedByUser, async () =>
          {
            MyEntity? entity = null;
            try
            {
              entity = await dbContext.MyEntities
                .FirstOrDefaultAsync(e => e.EntityId == order.EntityId, cancellationToken)
                ?? throw new InvalidDomainOperationException($"Entity {order.EntityId} not found.");

              // ── Perform the long-running work here ──────────────────────────
              // e.g. call an external API, run a multi-step provisioning process,
              // execute a bulk operation, etc.
              // entity.ResultField = await externalService.DoSomethingAsync(entity);
              entity.ProcessingError = null;
            }
            catch (Exception ex)
            {
              // Record the error on the entity if it was fetched successfully.
              // If it was null (not found), only log — there is nothing to stamp.
              if (entity != null)
              {
                entity.ProcessingError = ex.Message.Length > 500
                  ? ex.Message[..500]
                  : ex.Message;
              }
              logger.LogError(ex, "Error processing entity {EntityId}", order.EntityId);
            }

            // Stamp completion and notify only when the entity was found.
            // ProcessingError being set vs null distinguishes failure from success.
            if (entity != null)
            {
              entity.ProcessingCompletedOn = DateTime.UtcNow;
              await dbContext.SaveChangesAsync(cancellationToken);

              // ── Push the result back to the requesting user's browser ─────────
              // The process key ("LongRunningTask" below) must match the key used
              // in the frontend ServerMessageSubscriber.subscribe call.
              var lookup = await queryService.GetMyEntityAsync(order.EntityId);
              if (lookup != null && order.RequestedByUser?.IdentityGuid is Guid recipientId)
              {
                await notificationService.SendProcessMessageAsync(
                  Recipient.ProcessMessageRecipient(recipientId, "LongRunningTask", lookup));
              }
            }
          });        // [multi-tenancy]
        });          // [multi-tenancy]
      }
    }
  }
}
```

> **Key points**
> - The `Order` class uses **static factory methods** and a private constructor so the two
>   distinct order modes (`ForEntity` / `AllIncompleteItems`) are self-documenting and
>   impossible to construct in an invalid state.
> - The `Order` class captures the *full* `User` object (not just the ID). The background
>   thread has no HTTP context, so `RunWithOverrideUserAsync` is required to restore user
>   context for auditing and push notifications. In multi-tenanted projects,
>   `RunWithOverrideTenantIdAsync` is also required to restore EF query filters.
> - `IIsolatedBackgroundWorker` gives the worker its own dedicated hosted service and queue.
>   `StartAsync` is called once on the background thread at application startup — it calls
>   `DoWork` directly, so recovery is processed in-place before the normal dequeue loop
>   begins. Application startup is not blocked because the hosted service fires the background
>   thread and returns `Task.CompletedTask` immediately. New work orders enqueued during
>   recovery wait safely in the isolated queue until `StartAsync` returns.
> - In multi-tenanted projects, the `AllIncomplete` recovery path uses `IgnoreTenantFilter()`
>   because no tenant context exists at startup. Each recovered item re-enters the normal
>   path via `RunWithOverrideTenantIdAsync`.
> - Errors are caught and stored on the entity so the frontend can surface them. The entity
>   is declared before the `try` block so a failure during the fetch itself is also caught
>   and logged (though no completion can be stamped if the entity was never found).
> - `ProcessingCompletedOn` is stamped and `SaveChangesAsync` is called outside the
>   `try/catch`, only when the entity was successfully fetched — ensuring completion is
>   always persisted regardless of whether the work succeeded or failed.
> - `SendProcessMessageAsync` is a fire-and-forget push over SignalR. It is safe to call even
>   when the user's browser tab is closed — the message is silently dropped.

---

### Step 2 — Start the Work From the Command Service

In your command service, mark the entity as "processing started" synchronously (so the UI
can react immediately), then enqueue the work order:

```csharp
/// <summary>
/// Begins processing the long-running task for the given entity.
/// </summary>
/// <param name="entityId">The entity to process.</param>
public async Task StartLongRunningTaskAsync(int entityId)
{
  var currentUser = await userResolver.GetUserAsync()
    ?? throw new InvalidDomainOperationException("Current user not found.");

  // Attach a stub, reset completion state, and stamp the "in-progress" field
  // synchronously so the HTTP response already reflects the pending state.
  // Resetting ProcessingCompletedOn ensures a re-run is picked up correctly
  // by startup recovery if the app crashes before the worker finishes.
  var entity = dbContext.MyEntities.Attach(new MyEntity { EntityId = entityId }).Entity;
  entity.ProcessingStartedOn = DateTime.UtcNow;
  entity.ProcessingCompletedOn = null;
  entity.ProcessingError = null;
  await dbContext.SaveChangesAsync();

  // Queue the actual work — returns immediately.
  backgroundTaskQueue.QueueLongRunningTask(
    tenantService.GetCurrentTenantId(), // [multi-tenancy]
    entityId,
    currentUser);
}
```

Constructor injects `IBackgroundTaskQueue` and (if multi-tenanted) `ITenantService` alongside
your existing dependencies:

```csharp
public class MyEntityCommandService(
  AppDbContext dbContext,
  MyEntityQueryService queryService,
  IUserResolver<User> userResolver,
  IBackgroundTaskQueue backgroundTaskQueue,
  ITenantService tenantService)  // [multi-tenancy]
{ ... }
```

---

### Step 3 — Register the Background Workers

Call `AddBackgroundWorkers` once in your startup/`Program.cs`, passing the assembly that
contains your worker class:

```csharp
services.AddBackgroundWorkers(typeof(LongRunningTaskBackgroundWork).Assembly);
```

This single call registers:
- `IBackgroundTaskQueue` (singleton)
- `QueueHostedService` (hosted service that drains the queue)
- Every `IBackgroundWorker<,>` implementation found in the given assembly (scoped)

> If you call `AddBackgroundWorkers` for more than one assembly in the same application, use
> multiple calls — one per assembly.

---

## Frontend

### Step 4 — Subscribe to the Process Message in the ViewModel

Inject `ServerMessageSubscriber` via the constructor (it is registered by the
`@singularsystems/neo-notifications` module):

```typescript
import { Model } from '@singularsystems/neo-core';
import { Views } from '@singularsystems/neo-react';
import { AppService, Types } from '../../DomainTypes';
import MyEntityLookup from '../../Contracts/MyFeature/Lookups/MyEntityLookup';

export default class MyFeatureListVM extends Views.ViewModelBase {

    constructor(
        taskRunner = AppService.get(Types.Neo.TaskRunner),
        private commandApiClient = AppService.get(Types.Domain.ApiClients.MyFeatureCommandApiClient),
        private notifications = AppService.get(Types.Neo.UI.GlobalNotifications),
        serverMessageSubscriber = AppService.get(Types.Neo.Messaging.ServerMessageSubscriber)) {

        super(taskRunner);
        this.makeObservable();

        // "LongRunningTask" must match the process key used in SendProcessMessageAsync.
        this.autoDispose(
            serverMessageSubscriber.subscribe("LongRunningTask", this.onTaskCompleted.bind(this))
        );
    }

    public async startTask(entity: MyEntityLookup) {
        await this.taskRunner.run(async () => {
            await this.commandApiClient.startLongRunningTask(entity.entityId);

            // Optimistically disable the action button while processing.
            entity.processingStartedOn = new Date();

            this.notifications.addSuccess(
                "Processing started",
                "Your request is being processed. You will be notified when it completes.");
        });
    }

    private onTaskCompleted(message: Model.PlainObject<MyEntityLookup>) {
        // Update the in-memory record so the view reflects the latest state
        // without requiring a page refresh.
        const entity = this.pageManager.data.find(e => e.entityId === message.entityId);
        if (entity) {
            entity.mapFrom(message);
        }

        if (message.processingError) {
            this.notifications.addDanger(
                "Processing failed",
                `Failed to process entity ${message.entityId}. ${message.processingError}`);
        } else {
            this.notifications.addSuccess(
                "Processing complete",
                "The task completed successfully.");
        }
    }
}
```

> **Key points**
> - Always wrap the subscription in `this.autoDispose(...)` so it is unsubscribed when the
>   view is torn down and memory is not leaked.
> - The `processKey` string (`"LongRunningTask"`) is the only coupling between backend and
>   frontend — keep it as a named constant if it is used in more than one place.
> - `entity.mapFrom(message)` applies all updated fields from the server payload to the
>   existing MobX-observable object, causing the bound view to re-render automatically.

---

## Checklist

- [ ] `Order` uses static factory methods and a private constructor
- [ ] `Order` captures the full `User` object (and `TenantId` if multi-tenanted)
- [ ] Worker implements `IBackgroundWorker<Order, Worker>` (and optionally `IIsolatedBackgroundWorker` for startup recovery)
- [ ] If `IIsolatedBackgroundWorker`: `StartAsync` calls `DoWork` directly with an `AllIncomplete` order; `DoWork` handles the `AllIncomplete` branch (with `IgnoreTenantFilter()` if multi-tenanted)
- [ ] `ProcessItemAsync` wraps logic in `RunWithOverrideUserAsync` (and `RunWithOverrideTenantIdAsync` if multi-tenanted)
- [ ] Errors are caught and stored on the entity (not re-thrown), so the push message always fires
- [ ] `AddBackgroundWorkers(typeof(MyBackgroundWork).Assembly)` is called during startup
- [ ] Frontend `subscribe` call is wrapped in `autoDispose`
- [ ] Process key string matches exactly on both sides
