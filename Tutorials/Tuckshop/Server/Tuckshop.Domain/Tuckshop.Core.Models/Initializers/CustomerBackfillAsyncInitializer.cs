namespace Tuckshop.Core.Models.Initializers
{
  using Extensions.Hosting.AsyncInitialization;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Logging;
  using System;
  using System.Linq;
  using System.Threading;
  using System.Threading.Tasks;

  /// <summary>
  /// One-time backfill that creates Customer records from legacy free-text
  /// Order.CustomerName values, and links the matching Orders to them.
  /// </summary>
  /// <remarks>
  /// Idempotent by construction: it only looks at Orders where CustomerId is
  /// still null, so after the first successful run there is nothing left to
  /// process and this becomes a no-op. Safe to leave registered, or remove
  /// once you've confirmed the Customers table looks right.
  /// </remarks>
  /// <param name="context">The model database context.</param>
  /// <param name="logger">Logger for reporting what was backfilled.</param>
  public class CustomerBackfillAsyncInitializer(
    ModelDbContext context,
    ILogger<CustomerBackfillAsyncInitializer> logger) : IAsyncInitializer
  {
    private readonly ModelDbContext context = context;
    private readonly ILogger<CustomerBackfillAsyncInitializer> logger = logger;

    /// <inheritdoc/>
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
      var ordersToProcess = await this.context.Orders
        .Where(o => o.CustomerId == null && !o.IsCashSale)
        .ToListAsync(cancellationToken)
        .ConfigureAwait(false);

      if (ordersToProcess.Count == 0)
      {
        return; // Nothing to backfill - either already run, or no legacy data.
      }

      this.logger.LogInformation(
        "Customer backfill: found {Count} order(s) without a linked customer.",
        ordersToProcess.Count);

      // Group by normalized name (trim + case-insensitive) so near-duplicate
      // entries (e.g. "alice " vs "Alice") merge into one Customer.
      var groups = ordersToProcess
        .GroupBy(o => o.CustomerName.Trim(), StringComparer.OrdinalIgnoreCase)
        .ToList();

      // We can't tell from a bare name whether repeats are the same person
      // ordering many times (the common case) or different people who share
      // a name (rare, but possible) - just log it so it can be reviewed.
      foreach (var group in groups.Where(g => g.Count() > 1))
      {
        this.logger.LogInformation(
          "Customer backfill: '{Name}' — {Count} order(s) grouped as one customer.",
          group.Key,
          group.Count());
      }

      var placeholderIndex = 1;

      foreach (var group in groups)
      {
        var rawName = group.Key;
        var nameParts = rawName.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var firstName = nameParts.Length > 0 ? nameParts[0] : rawName;
        var lastName = nameParts.Length > 1 ? nameParts[1] : "TBC";

        var customer = new Customer
        {
          FirstName = firstName,
          LastName = lastName,

          // Placeholders - Customer requires these fields non-empty. Filter
          // on "pending+" afterward to find every record still needing real
          // contact details.
          Email = $"pending+{placeholderIndex}@tuckshop.local",
          PhoneNumber = "0000000000",
        };

        this.context.Customers.Add(customer);
        await this.context.SaveChangesAsync(cancellationToken).ConfigureAwait(false); // save now so CustomerId is generated

        // Order.CustomerId has a private setter and no domain method to
        // assign it, so set it directly on the tracked entity.
        foreach (var order in group)
        {
          this.context.Entry(order).Property(o => o.CustomerId).CurrentValue = customer.CustomerId;
        }

        placeholderIndex++;
      }

      await this.context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

      this.logger.LogInformation(
        "Customer backfill complete. Review Customers with an Email starting with 'pending+' and fill in FirstName/LastName/Email/PhoneNumber.");
    }
  }
}
