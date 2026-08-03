namespace NeoTemplate.AuthorisationServer.Api
{
  using Microsoft.AspNetCore.Hosting;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.DependencyInjection;
  using Neo.App;
  using Neo.AuthorisationServer.Models;
  using Neo.AuthorisationServer.Services;
  using Neo.Builders;
  using Neo.Extensions.DependencyInjection;
  using NeoTemplate.AuthorisationServer.Migrations.DesignTime;
  using NeoTemplate.AuthorisationServer.Models;

  /// <summary>
  /// The startup of the application
  /// </summary>
  public class Startup : Neo.AuthorisationServer.Api.Startup
  {
    /// <summary>
    /// The config key for the primary db connection string
    /// </summary>
    public const string MainConnectionStringKey = "Main";

    /// <summary>
    /// Initializes a new instance of the <see cref="Startup"/> class.
    /// </summary>
    /// <param name="configuration">Config</param>
    /// <param name="environment">The web host environment</param>
    public Startup(IConfiguration configuration, IWebHostEnvironment environment)
      : base(configuration, environment)
    {
    }

    /// <inheritdoc/>
    protected override void BuildServices(NeoServiceCollectionBuilder builder)
    {
      this.BuildServices<AuthorisationDbContext, NeoTemplateAuthorisationUser, UserClaimMapper>(builder);

      builder
        .Replace(
          Neo.AuthorisationServer.Api.StartupActions.DataServices,
          services => services.AddAuthorisationDataServices<AuthorisationDbContext, NeoTemplateAuthorisationUser>(this.Environment, this.Configuration, (options, connectionString) =>
          {
            options.UseSqlServer(connectionString, builder => builder.MigrationsAssembly(typeof(DesignTimeDbContextFactory).Assembly.GetName().Name));
          }))

        .Replace(
          Neo.AuthorisationServer.Api.StartupActions.ModelServices,
          services => services.AddAuthorisationModelServices<AuthorisationDbContext, NeoTemplateAuthorisationUser>(this.Environment, this.Configuration, new AuthorisationUserOptions<NeoTemplateAuthorisationUser>()
          {
            // This will filter the users in the user access page to only invited users.
            LookupPredicate = user => user.IsInvitedUser
          }))

        .Replace(NeoSetupActions.MultiTenancy, services => services.AddAuthorisationMultiTenancy<AuthorisationDbContext, NeoTemplateAuthorisationUser>(this.Environment, this.Configuration, singleTenantId: 0))

        .Replace(
          Neo.AuthorisationServer.Api.StartupActions.AsyncInitialisers,
          services => services.AddAuthorisationAsyncInitialisers<AuthorisationDbContext, NeoTemplateAuthorisationUser, AuthorisationDbAsyncInitializer>(this.Environment, this.Configuration))

        .Replace(Neo.AuthorisationServer.Api.StartupActions.UserEnrolmentOptions, services => services.AddScoped<IUserEnrolmentOptions, UserEnrolmentHandler>())
        .Replace(StartupActions.Logging, services => services.AddAuthorisationLogging(this.Environment, this.Configuration))
        .Add(StartupActions.Integrations, services => services.AddIntegrations(this.Environment, this.Configuration))
        .Replace(NeoSetupActions.Mvc, services => services.AddAuthorisationMvc<NeoTemplateAuthorisationUser>(this.Environment, this.Configuration, options => Neo.Extensions.DependencyInjection.StartupExtensions.ConfigureAuthorizationOptions(options, services))!
                                                          .AddApplicationPart(typeof(Startup).Assembly));
    }
  }
}