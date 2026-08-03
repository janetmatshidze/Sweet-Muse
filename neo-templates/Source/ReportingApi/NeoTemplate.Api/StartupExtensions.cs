namespace NeoTemplate.Api
{
  using System;
  using Microsoft.AspNetCore.Builder;
  using Microsoft.AspNetCore.Hosting;
  using Microsoft.AspNetCore.Http;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Hosting;
  using Neo.App;
  using Neo.Builders;
  using Neo.Extensions;
  using Neo.Extensions.DependencyInjection;
  using Neo.Identity;
  using Neo.Model.AuditTrail;
  using Neo.Model.Identity;
  using Neo.Model.Processing;
  using Neo.Model.Swagger;
  using Neo.Options;
  using Neo.Reporting.Pdf;
  using NeoTemplate.Models.Identity;
  using NeoTemplate.Models.Initializers;
  using NeoTemplate.Models.Migrations.Initializers;

  /// <summary>
  /// Startup extensions.
  /// </summary>
  public static class StartupExtensions
  {
    /// <summary>
    /// Adds base services for a Neo Web API project.
    /// </summary>
    /// <param name="serviceCollection">The service collection.</param>
    /// <param name="environment">The web host environment.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="actionBuilder">The action builder (Optional).</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddReportingWebApiBase(this IServiceCollection serviceCollection, IWebHostEnvironment environment, IConfiguration configuration, Action<NeoServiceCollectionBuilder>? actionBuilder = null)
    {
      var startupOptions = new NeoModelStartupOptions();

      serviceCollection
        .AddNeoWebApiBase(environment, configuration, startupOptions, builder =>
        {
          actionBuilder?.Invoke(builder);
        });

      return serviceCollection;
    }

    /// <summary>
    /// Adds Reporting user data services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="environment">The web host environment.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddReportingUserDataServices(this IServiceCollection services, IWebHostEnvironment environment, IConfiguration configuration)
    {
      services.AddScoped<IDbContextProcessor, AuditTrailProcessor<User>>();

      services.AddNeoClientUserResolver<User, UserClaimMapper>(
        serviceProvider => new UserClaimMapper(serviceProvider.GetService<IHttpContextAccessor>()?.HttpContext?.User));

      services.AddDbContext<IUsersDbContext<User>, ReportingDbContext>(
       options => options.UseSqlServer(configuration.GetConnectionString(Startup.MainConnectionStringKey)!, builder => builder.MigrationsAssembly(typeof(ReportingDbAsyncInitializer).Assembly.GetName().Name)));

      return services;
    }

    /// <summary>
    /// Adds Reporting Data services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="environment">The web host environment.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddReportingDataServices(this IServiceCollection services, IWebHostEnvironment environment, IConfiguration configuration)
    {
      services.AddNeoDbContext<ReportingDbContext>(
        options => options.UseSqlServer(
          configuration.GetConnectionString(Startup.MainConnectionStringKey)!,
          builder => builder.MigrationsAssembly(typeof(ReportingDbAsyncInitializer).Assembly.GetName().Name)));

      services.AddReportingWithNotificationsWithDbCache<ReportingDbContext, User, IronPdfChromeRenderer>(environment, configuration);

      services.AddAsyncInitialization();

      // The sequence of these initializers is important!
      services.AddAsyncInitializer<ReportingDbAsyncInitializer>();
      services.AddSystemUser<ReportingDbContext, User>();
      services.AddAsyncInitializer<SeedDataAsyncInitializer>();

      return services;
    }

    /// <summary>
    /// Adds Swagger services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="environment">The web host environment.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddReportingSwagger(this IServiceCollection services, IWebHostEnvironment environment, IConfiguration configuration)
    {
      if (!environment.IsProduction())
      {
        var appOptions = configuration.GetOptions<NeoAppOptions>();
        var authenticationOptions = configuration.GetOptions<NeoAuthenticationOptions>();
        var neoSwaggerOptions = configuration.GetOptions<NeoSwaggerOptions>();

        // If there are no scopes, use the defaults
        if (neoSwaggerOptions.Scopes.Count == 0)
        {
          neoSwaggerOptions.Scopes.Add(configuration["ApiResource:ResourceName"]!, $"{appOptions.Title} - full access");
        }

        services.AddNeoSwaggerGen(neoSwaggerOptions, appOptions, authenticationOptions);
      }

      return services;
    }

    /// <summary>
    /// Adds standard Neo Web API services.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <param name="environment">The web host environment.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The app builder.</returns>
    public static IApplicationBuilder UseReportingApiBase(this IApplicationBuilder app, IWebHostEnvironment environment, IConfiguration configuration)
    {
      NeoApplicationBuilder builder = new NeoApplicationBuilder(app);

      builder
        .Add(NeoSetupActions.PathBase, app =>
        {
          if (configuration.OptionsConfigExists<RoutingOptions>() &&
              !string.IsNullOrEmpty(configuration.GetOptions<RoutingOptions>().PathBase))
          {
            app.UseNeoPathBase(environment, configuration);
          }
        })
        .Add(NeoSetupActions.ForwardedHeaders, app =>
        {
          if (configuration.OptionsConfigExists<NeoForwardedHeadersOptions>())
          {
            app.UseNeoForwardedHeaders(environment, configuration);
          }
        })
        .Add(NeoSetupActions.ExceptionHandling, app => app.UseNeoExceptionHandling(environment, configuration))
        .Add(NeoSetupActions.Hsts, app => app.UseNeoHsts(environment, configuration))
        .Add(NeoSetupActions.DefaultFiles, app => app.UseNeoDefaultFiles(environment, configuration))
        .Add(NeoSetupActions.HttpsRedirection, app => app.UseNeoHttpsRedirection(environment, configuration))
        .Add(NeoSetupActions.StaticFiles, app => app.UseNeoStaticFiles(environment, configuration))
        .Add(NeoSetupActions.Routing, app => app.UseNeoRouting(environment, configuration))
        .Add(NeoSetupActions.Cors, app => app.UseNeoCors(environment, configuration))
        .Add(NeoSetupActions.PreAuthenticationMiddleware, app => app.UseNeoPreAuthenticationMiddleware(environment, configuration))
        .Add(NeoSetupActions.Authentication, app => app.UseNeoAuthentication(environment, configuration))
        .Add(NeoSetupActions.PostAuthenticationMiddleware, app => app.UseNeoPostAuthenticationMiddleware(environment, configuration))
        .Add(NeoSetupActions.Endpoints, app => app.UseNeoControllersEndpoints(environment, configuration));

      return app;
    }

    /// <summary>
    /// Configures standard Swagger setup.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <param name="environment">The web host environment.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The app builder.</returns>
    public static IApplicationBuilder UseReportingSwagger(this IApplicationBuilder app, IWebHostEnvironment environment, IConfiguration configuration)
    {
      if (!environment.IsProduction())
      {
        var appOptions = configuration.GetOptions<NeoAppOptions>();
        var authenticationOptions = configuration.GetOptions<NeoAuthenticationOptions>();
        var neoSwaggerOptions = configuration.GetOptions<NeoSwaggerOptions>();

        app.UseSwagger();
        app.UseNeoSwagger(appOptions, neoSwaggerOptions);
      }

      return app;
    }
  }
}
