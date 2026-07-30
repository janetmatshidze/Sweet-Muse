namespace NeoTemplate.Models.Migrations.Initializers
{
  using System.Threading;
  using System.Threading.Tasks;
  using Extensions.Hosting.AsyncInitialization;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.Hosting;
  using Neo.Model.Processing;
  using NeoTemplate.Models.Migrations.DesignTime;

  /// <summary>
  /// Will migrate the database and add test data if the environment is Development.
  /// </summary>
  public class AggregateRootDbAsyncInitializer : IAsyncInitializer
  {
    private readonly IConfiguration configuration;
    // This is suppressed so I can leave the constructor parameters in tact.
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Future use")]
    private readonly IHostEnvironment environment;

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateRootDbAsyncInitializer"/> class.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    /// <param name="environment">The web host environment.</param>
    public AggregateRootDbAsyncInitializer(IConfiguration configuration, IHostEnvironment environment)
    {
      this.configuration = configuration;
      this.environment = environment;
    }

    /// <inheritdoc/>
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
      using var context = this.CreateDbContext();
      await context.Database.MigrateAsync(cancellationToken);
    }

    /// <summary>
    /// Will return an DbContext using the connection string from appsettings.json
    /// </summary>
    /// <returns>A DbContext</returns>
    private AggregateRootDbContext CreateDbContext()
    {
      var builder = new DbContextOptionsBuilder<AggregateRootDbContext>();
      var connectionString = this.configuration.GetConnectionString("AggregateRoot");
      builder.UseSqlServer(connectionString, sqlServerDbContextOptionsBuilder => sqlServerDbContextOptionsBuilder.MigrationsAssembly(typeof(DesignTimeDbContextFactory).Assembly.GetName().Name));
      var processingOptions = new DbContextProcessingOptions<AggregateRootDbContext>();
      return new AggregateRootDbContext(builder.Options, processingOptions);
    }
  }
}
