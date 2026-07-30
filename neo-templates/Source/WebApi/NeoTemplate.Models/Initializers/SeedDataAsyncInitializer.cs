namespace NeoTemplate.Models.Initializers
{
  using System;
  using System.Collections.Generic;
  using System.Threading;
  using System.Threading.Tasks;
  using Extensions.Hosting.AsyncInitialization;
  using Microsoft.Extensions.Hosting;
  using Neo.Model.Identity.SystemUser;
  using NeoTemplate.Models.Identity;

  /// <summary>
  /// Seed data generation service
  /// </summary>
  public class SeedDataAsyncInitializer : IAsyncInitializer
  {
    private readonly ModelDbContext context;
    private readonly ISystemUserService<User> systemUserService;
    private readonly IHostEnvironment? environment;

    /// <summary>
    /// Initializes a new instance of the <see cref="SeedDataAsyncInitializer"/> class.
    /// </summary>
    /// <param name="context">The db context to initialize.</param>
    /// <param name="systemUserService">The system user service.</param>
    /// <param name="environment">The host environment.</param>
    public SeedDataAsyncInitializer(
      ModelDbContext context,
      ISystemUserService<User> systemUserService,
      IHostEnvironment? environment)
    {
      this.context = context;
      this.systemUserService = systemUserService;
      this.environment = environment;
    }

    /// <inheritdoc/>
    public Task InitializeAsync(CancellationToken cancellationToken)
    {
      return this.systemUserService.RunWithSystemUserAsync(() => this.GenerateSeedDataAsync(cancellationToken));
    }

    /// <summary> 
    /// Will generate the appropriate seed data for the given environment
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task awaiting the seed data generation</returns>
    public Task GenerateSeedDataAsync(CancellationToken cancellationToken)
    {
      return this.AddExampleEntitiesAsync(cancellationToken);
    }

    private Task<int> AddExampleEntitiesAsync(CancellationToken cancellationToken)
    {
      var exampleEntities = new List<AggregateRoot>()
      {
        new AggregateRoot()
        {
          AggregateRootName = "Example Entity 1",
          ExampleDate = DateTime.Now.Date.AddDays(-2),
        },
        new AggregateRoot()
        {
          AggregateRootName = "Example Entity 2",
          ExampleDate = DateTime.Now.Date.AddDays(-1),
        },
        new AggregateRoot()
        {
          AggregateRootName = "Example Entity 3",
          ExampleDate = DateTime.Now.Date,
        },
      };

      if (this.environment == null)
      {
        int id = 1;
        foreach (var ee in exampleEntities)
        {
          ee.AggregateRootId = id++;
        }
      }

      this.context.AggregateRoots.AddRange(exampleEntities);

      return this.context.SaveChangesAsync(cancellationToken);
    }
  }
}
