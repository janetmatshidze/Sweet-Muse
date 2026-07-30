namespace NeoTemplate.Api
{
  using Microsoft.AspNetCore.Hosting;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.DependencyInjection;
  using Neo.Extensions.DependencyInjection;
  using Neo.Model.Services;
  using NeoTemplate.App.Services;
  using NeoTemplate.Models;
  using NeoTemplate.Models.Initializers;
  using NeoTemplate.Models.Migrations.Initializers;

  /// <summary>
  /// Startup extensions
  /// </summary>
  public static class StartupExtensions
  {
    // The connection string for this Api.
    private const string AggregateRootConnectionStringKey = "AggregateRoot";

    /// <summary>
    /// Adds NeoTemplateServiceName user data services
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="environment">The web host environment</param>
    /// <param name="configuration">The configuration</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddNeoTemplateServiceNameUserDataServices(this IServiceCollection services, IWebHostEnvironment environment, IConfiguration configuration)
    {
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
      services.AddNeoDbContext<AggregateRootDbContext>(
        options => options.UseSqlServer(configuration.GetConnectionString(AggregateRootConnectionStringKey)));

      // The sequence of these initializers is important!
      services.AddAsyncInitializer<AggregateRootDbAsyncInitializer>();
      //services.AddSystemUser<AggregateRootDbContext, User>();
      services.AddAsyncInitializer<AggregateRootSeedDataAsyncInitializer>();

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
      services.AddScoped<IUpdateableModelService<AggregateRoot, AggregateRootDbContext, int>, AggregateRootService>();
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
  }
}
