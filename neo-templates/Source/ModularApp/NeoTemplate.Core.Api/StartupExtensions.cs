namespace NeoTemplate.Core.Api
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Reflection;
  using Microsoft.AspNetCore.Builder;
  using Microsoft.AspNetCore.Hosting;
  using Microsoft.AspNetCore.Http;
  using Microsoft.AspNetCore.SignalR;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.DependencyInjection.Extensions;
  using Microsoft.Extensions.Hosting;
  using Neo.AuthorisationServer.Client;
  using Neo.Builders;
  using Neo.Extensions;
  using Neo.Extensions.DependencyInjection;
  using Neo.Identity;
  using Neo.Model.AuditTrail;
  using Neo.Model.Excel.EPPlus;
  using Neo.Model.Identity;
  using Neo.Model.Mappers;
  using Neo.Model.MultiTenancy;
  using Neo.Model.Processing;
  using Neo.Model.Serilog.Enrichers;
  using Neo.Model.Swagger;
  using Neo.Options;
  using Neo.SecretVault;
  using Neo.SignalR;
  using NeoTemplate.Core.App.Services;
  using NeoTemplate.Core.Models;
  using NeoTemplate.Core.Models.Files;
  using NeoTemplate.Core.Models.Identity;
  using NeoTemplate.Core.Models.Initializers;
  using NeoTemplate.Core.Models.Migrations.DesignTime;
  using NeoTemplate.Core.Models.Migrations.Initializers;
  using NeoTemplate.Extensions;
  using Serilog;
  using Serilog.Filters;

  /// <summary>
  /// Startup extensions.
  /// </summary>
  public static class StartupExtensions
  {
    /// <summary>
    /// The secret vaults provider. This has to be created explicitly because it is needed during startup (I.e. before the DI container is available).
    /// It is declared here so that all the startup methods in this class can access it if required.
    /// </summary>
    private static readonly ISecretVaultServiceProvider SecretVaultsProvider = new SecretVaultServiceProvider();

    /// <summary>
    /// Adds NeoTemplate model services.
    /// Model services are services that interact with the DbContext.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="environment">The web host environment.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddNeoTemplateModelServices(this IServiceCollection services, IWebHostEnvironment environment, IConfiguration configuration)
    {
      services.TryAddSingleton<INeoMapper, NeoMapper>();

      services.AddScoped<CatalogueModelService>();

      return services;
    }

    /// <summary>
    /// Adds Identity Secret Vault services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="environment">The web host environment.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddNeoTemplateSecretVaults(this IServiceCollection services, IWebHostEnvironment environment, IConfiguration configuration)
    {
      services.AddSingleton(SecretVaultsProvider);
      services.AddNeoKeyVaults(configuration, SecretVaultsProvider);
      return services;
    }

    /// <summary>
    /// Adds Always Encrypted services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="environment">The web host environment.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddNeoTemplateAlwaysEncrypted(this IServiceCollection services, IWebHostEnvironment environment, IConfiguration configuration)
    {
      return services.AddNeoSqlAlwaysEncryptedWithKeyVaultAsKeyStoreProvider(configuration, "Main");
    }

    /// <summary>
    /// Adds Identity Secret Vault services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="environment">The web host environment.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddNeoTemplateAuthentication(this IServiceCollection services, IWebHostEnvironment environment, IConfiguration configuration)
    {
      return services.AddNeoOpenIddictApiAuthentication(environment, configuration, SecretVaultsProvider, "Main");
    }

    /// <summary>
    /// Adds base services for a Neo Web API project.
    /// </summary>
    /// <param name="serviceCollection">The service collection.</param>
    /// <param name="environment">The web host environment.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="actionBuilder">The action builder (Optional).</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddNeoTemplateWebApiBase(
      this IServiceCollection serviceCollection,
      IWebHostEnvironment environment,
      IConfiguration configuration,
      Action<NeoServiceCollectionBuilder, NeoModelStartupOptions> actionBuilder)
    {
      var startupOptions = new NeoModelStartupOptions() { FluentValidatorAssemblies = [typeof(ModelDbContext).Assembly] };

      serviceCollection
        .AddNeoWebApiBase(environment, configuration, startupOptions, builder =>
        {
          actionBuilder?.Invoke(builder, startupOptions);
        });

      return serviceCollection;
    }

    /// <summary>
    /// Adds NeoTemplate processing services.
    /// Processing services are higher level services that use Model services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="environment">The web host environment.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddNeoTemplateProcessingServices(this IServiceCollection services, IWebHostEnvironment environment, IConfiguration configuration)
    {
      // Add this line once a background worker class is defined.
      // services.AddBackgroundWorkers(typeof(MyBackgroundWorkClass).Assembly);

      return services;
    }

    /// <summary>
    /// Adds NeoTemplate integrations services.
    /// Integration services are services like api clients, file imports and exports etc.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="environment">The web host environment.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddNeoTemplateIntegrationServices(this IServiceCollection services, IWebHostEnvironment environment, IConfiguration configuration)
    {
      services.AddCanvasGridServerProcessing(environment, configuration);
      services.AddExcelExporting<EPPlusExcelLayoutExporter>();

      services.AddIdentityClientServices(configuration, environment);

      return services;
    }

    /// <summary>
    /// Adds NeoTemplate jobs.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="environment">The web host environment.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddNeoTemplateJobs(this IServiceCollection services, IWebHostEnvironment environment, IConfiguration configuration)
    {
      services.AddNeoQuartzServices();

      // Register, and link job classes to config values here.
      // services.AddScheduledJob<MyJob>(configuration.GetValue<string>("JobSchedules:MyJob"));

      return services;
    }

    /// <summary>
    /// Adds integrity checkers.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="environment">The web host environment.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddNeoTemplateIntegrityChecking(this IServiceCollection services, IHostEnvironment environment, IConfiguration configuration)
    {
      // Add this line once an integrity checker class is defined.
      // services.AddIntegrityChecking(typeof(MyChecker).Assembly, configuration.GetSection("IntegrityChecking").Get<IntegrityCheckingOptions>());

      return services;
    }

    /// <summary>
    /// Adds NeoTemplate Data services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="environment">The web host environment.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddNeoTemplateDataServices(this IServiceCollection services, IWebHostEnvironment environment, IConfiguration configuration)
    {
      services.AddNeoModelSqlErrorPolicies();

      services.AddNeoDbContext<ModelDbContext>(
        options => options.UseSqlServer(
          configuration.GetConnectionString(Startup.MainConnectionStringKey),
          builder => builder.MigrationsAssembly(typeof(DesignTimeDbContextFactory).Assembly.GetName().Name)));

      services.AddSingleton<Neo.Model.Metadata.IMetadataService, Neo.Model.Metadata.MetadataService>();

      services.AddCommandDbContext<ModelDbContext>();
      services.AddNeoModelUtilityServices();

      services.AddAsyncInitialization();

      // The sequence of these initializers is important!
      services.AddAsyncInitializer<ModelDbAsyncInitializer>();
      services.AddSystemUser<ModelDbContext, User>();
      services.AddAsyncInitializer<SeedDataAsyncInitializer>();

      return services;
    }

    /// <summary>
    /// Adds NeoTemplate File Storage Services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="environment">The web host environment.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddNeoTemplateFileStorageServices(this IServiceCollection services, IWebHostEnvironment environment, IConfiguration configuration)
    {
      services.Configure<Neo.Model.FileStorage.Options.FileManagerOptions<FileDescriptor, FileContext>>(opt =>
      {
        opt.DisallowedExtensions.Add(".sql");
      });

      services.AddFileHeaderValidator();

      /*
        TODO: 
        Replace AddXFileStorage below with one of the neo storage options. Azure file store, SqlServer file store, or FileSystem store.
        For more information read: https://bitbucket.org/iiintel/neo.core/src/master/Source/Neo.Model/FileStorage/ReadMe.md

        services.AddXFileStorage<FileDescriptor, FileContext, ModelDbContext>(options =>
        {
          options.IncludeHttpServices = true;
        });
       */

      return services;
    }

    /// <summary>
    /// Adds NeoTemplate user data services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="environment">The web host environment.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddNeoTemplateUserDataServices(this IServiceCollection services, IWebHostEnvironment environment, IConfiguration configuration)
    {
      services.AddHttpContextAccessor();

      services.AddScoped<IDbContextProcessor, AuditTrailProcessor<User>>();

      services.AddNeoClientUserResolver<User, UserClaimMapper>(
        serviceProvider => new UserClaimMapper(serviceProvider.GetService<IHttpContextAccessor>()?.HttpContext?.User),
        options => options.WithSelectNameExpression(u => $"{u.FirstName} {u.LastName}"));

      services.AddScoped<IUsersDbContext<User>>(services => services.GetRequiredService<ModelDbContext>());

      services.AddOneTimeTokenServiceWithDbCache<ModelDbContext>();

      return services;
    }

    /// <summary>
    /// Adds the application modules.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="environment">The web host environment.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddNeoTemplateModules(this IServiceCollection services, IWebHostEnvironment environment, IConfiguration configuration)
    {
      // get all the types that implement IRoles from the assembly.
      var roleTypes = GetLoadableTypes(typeof(Security.Roles).Assembly).Where(t => typeof(IRoles).IsAssignableFrom(t) && t.IsClass).ToList();
      roleTypes.Add(typeof(Reporting.Security.Roles));
      roleTypes.Add(typeof(Neo.NotificationServer.Models.Security.Roles));

      services.AddNeoTemplateAuthorisationServices(
        environment,
        configuration,
        roleTypes.ToArray());

      services.AddNeoTemplateNotificationServices<User>(environment, configuration);
      services.AddNeoTemplateReportingServices<User>(environment, configuration);

      return services;
    }

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
      try
      {
        return assembly.GetTypes();
      }
      catch (ReflectionTypeLoadException ex)
      {
        return ex.Types.Where(t => t is not null)!;
      }
    }

    /// <summary>
    /// Adds the app controllers for this app, as well as the controllers for other modules hosted by this app.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="environment">The web host environment.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="startupOptions">The startup options.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddNeoTemplateMvc(this IServiceCollection services, IWebHostEnvironment environment, IConfiguration configuration, NeoModelStartupOptions startupOptions)
    {
      // Add controllers.
      var mvcBuilder = services.AddNeoControllers(environment, configuration, startupOptions);

      // Add modules.
      mvcBuilder.AddNeoTemplateAuthorisationMvc(environment, configuration);
      mvcBuilder.AddNeoTemplateNotificationsMvc(environment, configuration);
      services.AddNeoTemplateReportingMvc(environment, configuration, mvcBuilder);

      return services;
    }

    /// <summary>
    /// Will add the NeoTemplate Caches.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="environment">The web host environment.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddNeoTemplateSignalR(this IServiceCollection services, IWebHostEnvironment environment, IConfiguration configuration)
    {
      services.AddSignalR().AddJsonProtocol(options =>
      {
        options.PayloadSerializerOptions.DictionaryKeyPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
      });
      services.AddSingleton<IUserIdProvider, SignalRUserIdProvider>();
      return services;
    }

    /// <summary>
    /// Will add the NeoTemplate Entity Change Publishers.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="environment">The web host environment.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddNeoTemplateEntityChangePublishers(this IServiceCollection services, IWebHostEnvironment environment, IConfiguration configuration)
    {
      /*
      services.AddSingleton<IEntityPublisher, NeoTemplatePublisher>();
      services.AddNeoDbContextChangePublisher();
      */
      return services;
    }

    /// <summary>
    /// Will add the NeoTemplate Caches.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="environment">The web host environment.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddNeoTemplateCaches(this IServiceCollection services, IWebHostEnvironment environment, IConfiguration configuration)
    {
      services.AddDistributedMemoryCache();
      services.AddLazyCache();
      return services;
    }

    /// <summary>
    /// Adds multi tenancy.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="environment">The web host environment.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddNeoTemplateMultiTenancy(this IServiceCollection services, IWebHostEnvironment environment, IConfiguration configuration)
    {
      services.AddSingleton<ITenantService>(new CustomTenantService());

      // Add this if your project requires tenant processing.
      // services.AddNeoMultiTenancy(...)

      return services;
    }

    /// <summary>
    /// Adds Swagger services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="environment">The web host environment.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddNeoTemplateSwagger(this IServiceCollection services, IWebHostEnvironment environment, IConfiguration configuration)
    {
      var swaggerOptions = configuration.GetOptions<NeoSwaggerOptions>();
      if (swaggerOptions.Enabled)
      {
        var appOptions = configuration.GetOptions<NeoAppOptions>();
        var authenticationOptions = configuration.GetOptions<NeoAuthenticationOptions>();

        if (swaggerOptions.Scopes.Count == 0)
        {
          swaggerOptions.Scopes.Add(configuration["ApiResource:ResourceName"] ??
            throw new System.Configuration.ConfigurationErrorsException("ApiResource:ResourceName is missing"), $"{appOptions.Title} - full access");
        }

        services.AddNeoSwaggerGen(swaggerOptions, appOptions, authenticationOptions);
      }

      return services;
    }

    /// <summary>
    /// Configures serilog from the app config.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="environment">The web host environment.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddNeoTemplateLogging(
      this IServiceCollection services,
      IWebHostEnvironment environment,
      IConfiguration configuration)
    {
      LoggerConfiguration loggerConfig = new LoggerConfiguration()
          // serilog-aspnetcore picks up and logs exceptions, so we filter out logs from the standard middleware to prevent duplicate logs.
          .Filter.ByExcluding(Matching.FromSource<Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddleware>())
          // Filters out 'info' logs from Microsoft.AspNetCore when the request path is a health check URL
          .Filter.ByExcluding(Matching.WithProperty<string>(
            propertyName: "RequestPath",
            requestPath => requestPath.EndsWith("/health/live", StringComparison.InvariantCultureIgnoreCase) || requestPath.EndsWith("/health/ready", StringComparison.InvariantCultureIgnoreCase)))
          .Enrich.FromLogContext()
          .ReadFrom.Configuration(configuration);

      var trimmingOptions = configuration.GetSection("Logging:ExceptionTrimming").Get<ExceptionTrimmingOptions>();
      if (trimmingOptions != null)
      {
        loggerConfig.Enrich.With(new ExceptionTrimmingEnricher(trimmingOptions));
      }

      Log.Logger = loggerConfig.CreateLogger();

      services.AddLogCleanupForSqlServer<ModelDbContext>(configuration);

      return services;
    }

    /// <summary>
    /// Configures standard Swagger setup.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <param name="environment">The web host environment.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection.</returns>
    public static IApplicationBuilder UseNeoTemplateSwagger(this IApplicationBuilder app, IWebHostEnvironment environment, IConfiguration configuration)
    {
      var swaggerOptions = configuration.GetOptions<NeoSwaggerOptions>();
      if (swaggerOptions.Enabled)
      {
        var appOptions = configuration.GetOptions<NeoAppOptions>();
        var neoSwaggerOptions = configuration.GetOptions<NeoSwaggerOptions>();

        app.UseNeoSwagger(appOptions, neoSwaggerOptions);
      }

      return app;
    }
  }
}