namespace NeoTemplate.Models.Initializers
{
  using System.Threading;
  using System.Threading.Tasks;
  using Extensions.Hosting.AsyncInitialization;
  using Microsoft.Extensions.Hosting;
  using Neo.Model.Identity.SystemUser;
  using NeoTemplate.Models.DummyUser;

  /// <summary>
  /// Seed data generation service
  /// </summary>
  public class AggregateRootSeedDataAsyncInitializer : IAsyncInitializer
  {
    private readonly AggregateRootDbContext context;
    private readonly ISystemUserService<User> systemUserService;
    private readonly IHostEnvironment? environment;

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateRootSeedDataAsyncInitializer"/> class.
    /// </summary>
    /// <param name="context">The db context to initialize.</param>
    /// <param name="systemUserService">The system user service.</param>
    /// <param name="environment">The host environment.</param>
    public AggregateRootSeedDataAsyncInitializer(
      AggregateRootDbContext context,
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
      return this.context.SaveChangesAsync(cancellationToken);
    }
  }
}
