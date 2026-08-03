namespace NeoTemplate.Models.Initializers
{
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
    private readonly ReportingDbContext context;
    private readonly ISystemUserService<User> systemUserService;
    private readonly IHostEnvironment? environment;

    /// <summary>
    /// Initializes a new instance of the <see cref="SeedDataAsyncInitializer"/> class.
    /// </summary>
    /// <param name="context">The db context to initialize.</param>
    /// <param name="systemUserService">The system user service.</param>
    /// <param name="environment">The host environment.</param>
    public SeedDataAsyncInitializer(
      ReportingDbContext context,
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
    /// Will generate the appropriate seed data for the given environment.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task awaiting the seed data generation.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Future use")]
    public Task GenerateSeedDataAsync(CancellationToken cancellationToken)
    {
      return Task.CompletedTask;
    }
  }
}
