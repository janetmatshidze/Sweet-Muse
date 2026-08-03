namespace NeoTemplate.AuthorisationServer.Models
{
  using System.Threading;
  using System.Threading.Tasks;
  using Extensions.Hosting.AsyncInitialization;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Hosting;
  using Neo.Extensions;

  /// <summary>
  /// Will migrate the database and add test data if the environment is Development.
  /// </summary>
  public class AuthorisationDbAsyncInitializer : IAsyncInitializer
  {
    private readonly AuthorisationDbContext context;
    private readonly IHostEnvironment environment;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorisationDbAsyncInitializer"/> class.
    /// </summary>
    /// <param name="context">The db context.</param>
    /// <param name="environment">The web host environment.</param>
    public AuthorisationDbAsyncInitializer(AuthorisationDbContext context, IHostEnvironment environment)
    {
      this.context = context;
      this.environment = environment;
    }

    /// <inheritdoc/>
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
      await this.context.Database.MigrateAsync(cancellationToken);

      if (this.environment.IsDevelopment())
      {
        this.context.AssertMigrationsCreated();
      }
    }
  }
}