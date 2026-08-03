namespace NeoTemplate.Api
{
  using System;
  using System.Collections.Generic;
  using System.Configuration;
  using Microsoft.AspNetCore.Builder;
  using Microsoft.AspNetCore.Hosting;
  using Microsoft.AspNetCore.Http;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Hosting;
  using Neo.Builders;
  using Neo.Extensions;
  using Neo.Extensions.DependencyInjection;
  using Neo.Identity;
  using Neo.Model.AuditTrail;
  using Neo.Model.Identity;
  using Neo.Model.Processing;
  using Neo.Model.Services;
  using Neo.Model.Swagger;
  using Neo.Options;
  using NeoTemplate.App.Services;
  using NeoTemplate.Models;
  using NeoTemplate.Models.Identity;
  using NeoTemplate.Models.Initializers;
  using NeoTemplate.Models.Migrations.Initializers;

  /// <summary>
  /// Startup extensions
  /// </summary>
  public static class StartupExtensions
  {
    /// <summary>
    /// Adds base services for a Neo Web API project.
    /// </summary>
    /// <param name="serviceCollection">The service collection.</param>
    /// <param name="environment">The web host environment.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="actionBuilder">The action builder (Optional)</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddNeoTemplateServiceNameWebApiBase(this IServiceCollection serviceCollection, IWebHostEnvironment environment, IConfiguration configuration, Action<NeoServiceCollectionBuilder>? actionBuilder = null)
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
    /// Adds NeoTemplateServiceName user data services
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="environment">The web host environment</param>
    /// <param name="configuration">The configuration</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddNeoTemplateServiceNameUserDataServices(this IServiceCollection services, IWebHostEnvironment environment, IConfiguration configuration)
    {
      services.AddScoped<IDbContextProcessor, AuditTrailProcessor<User>>();

      services.AddNeoClientUserResolver<User, UserClaimMapper>(
        serviceProvider => new UserClaimMapper(serviceProvider.GetService<IHttpContextAccessor>()?.HttpContext?.User));

      services.AddDbContext<IUsersDbContext<User>, ModelDbContext>(
       options => options.UseSqlServer(configuration.GetConnectionString(Startup.MainConnectionStringKey)));

      return services;
    }

    /// <summary>
    /// Adds NeoTemplateServiceName Data services
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="environment">The web host environment</param>
    /// <param name="configuration">The configuration</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddNeoTemplateServiceNameDataServices(this IServiceCollection services, IWebHostEnvironment environment, IConfiguration configuration)
    {
      var x = new List<string>();

      services.AddNeoModelSqlErrorPolicies();

      services.AddHttpContextAccessor();

      services.AddNeoDbContext<ModelDbContext>(
        options => options.UseSqlServer(configuration.GetConnectionString(Startup.MainConnectionStringKey)));

      services.AddSingleton<Neo.Model.Metadata.IMetadataService, Neo.Model.Metadata.MetadataService>();

      services.AddCommandDbContext<ModelDbContext>();

      services.AddAsyncInitialization();

      // The sequence of these initializers is important!
      services.AddAsyncInitializer<ModelDbAsyncInitializer>();
      services.AddSystemUser<ModelDbContext, User>();
      services.AddAsyncInitializer<SeedDataAsyncInitializer>();

      return services;
    }

    /// <summary>
    /// Adds NeoTemplateServiceName Model services
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="environment">The web host environment</param>
    /// <param name="configuration">The configuration</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddNeoTemplateServiceNameModelServices(this IServiceCollection services, IWebHostEnvironment environment, IConfiguration configuration)
    {
      services.AddScoped<IUpdateableModelService<AggregateRoot, ModelDbContext, int>, AggregateRootService>();
      return services;
    }

    /// <summary>
    /// Adds NeoTemplateServiceName Controllers
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="environment">The web host environment</param>
    /// <param name="configuration">The configuration</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddNeoTemplateServiceNameControllers(this IServiceCollection services, IWebHostEnvironment environment, IConfiguration configuration)
    {
      // add any generic controllers here
      return services;
    }

    /// <summary>
    /// Adds NeoTemplateServiceName File Storage Services
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="environment">The web host environment</param>
    /// <param name="configuration">The configuration</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddNeoTemplateServiceNameFileStorageServices(this IServiceCollection services, IWebHostEnvironment environment, IConfiguration configuration)
    {
      /* If you require file services replace AddXFileStorage below with one of the neo storage options. Azure file store, SqlServer file store, or FileSystem store.
         For more information read: https://bitbucket.org/iiintel/neo.core/src/master/Source/Neo.Model/FileStorage/ReadMe.md

      services.AddXFileStorage<FileDescriptor, FileContext, ModelDbContext>(options =>
      {
        options.IncludeHttpServices = true;
      });

      services.Configure<Neo.Model.FileStorage.Options.HttpFileManagerOptions>(opt =>
      {
        opt.DisallowedExtensions.Add(".sql");
      });
      */
      return services;
    }

    /// <summary>
    /// Will add the NeoTemplateServiceName Entity Change Publishers
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="environment">The web host environment</param>
    /// <param name="configuration">The configuration</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddNeoTemplateServiceNameEntityChangePublishers(this IServiceCollection services, IWebHostEnvironment environment, IConfiguration configuration)
    {
      /*
      services.AddSingleton<IEntityPublisher, NeoTemplatePublisher>();
      services.AddNeoDbContextChangePublisher();
      */
      return services;
    }

    /// <summary>
    /// Will add the NeoTemplateServiceName Caches
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="environment">The web host environment</param>
    /// <param name="configuration">The configuration</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddNeoTemplateServiceNameCaches(this IServiceCollection services, IWebHostEnvironment environment, IConfiguration configuration)
    {
      services.AddDistributedMemoryCache();
      return services;
    }

    /// <summary>
    /// Adds Swagger services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="environment">The web host environment.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddNeoTemplateServiceNameSwagger(this IServiceCollection services, IWebHostEnvironment environment, IConfiguration configuration)
    {
      if (!environment.IsProduction())
      {
        var appOptions = configuration.GetOptions<NeoAppOptions>();
        var authenticationOptions = configuration.GetOptions<NeoAuthenticationOptions>();
        var neoSwaggerOptions = configuration.GetOptions<NeoSwaggerOptions>();

        // If there are no scopes, use the defaults
        // ToDo: Eliminate direct usage of configuration
        if (neoSwaggerOptions.Scopes.Count == 0)
        {
          neoSwaggerOptions.Scopes.Add(configuration["ApiResource:ResourceName"] ?? throw new ConfigurationErrorsException("ApiResource:ResourceName missing"), $"{appOptions.Title} - full access");
        }

        services.AddNeoSwaggerGen(neoSwaggerOptions, appOptions, authenticationOptions);
      }

      return services;
    }

    /// <summary>
    /// Configures standard Swagger setup.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <param name="environment">The web host environment.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The app builder.</returns>
    public static IApplicationBuilder UseNeoTemplateServiceNameSwagger(this IApplicationBuilder app, IWebHostEnvironment environment, IConfiguration configuration)
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
