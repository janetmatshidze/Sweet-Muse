namespace NeoTemplate.Api
{
  using Microsoft.AspNetCore.Hosting;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.DependencyInjection;

  /// <summary>
  /// AggregateRoot Setup Class.
  /// 
  /// The SetupModule method needs to be called from the base Api during the Startup process. It is best used in StartupExtensions.
  /// Simply call 
  ///     AggregateRootSetup.SetupModule(services, environment, configuration);
  /// in the "Add.....Modules()" method in your main api. (The same place it registers the notifications and reporting services).
  /// </summary>
  public static class AggregateRootSetup
  {
    /// <summary>
    /// Setup module.
    /// </summary>
    /// <param name="services">Services.</param>
    /// <param name="env">Environment.</param>
    /// <param name="configuration">Configuration.</param>
    public static void SetupModule(IServiceCollection services, IWebHostEnvironment env, IConfiguration configuration)
    {
      // setup client module
      services.AddNeoTemplateServiceNameDataServices(env, configuration);
      services.AddNeoTemplateServiceNameModelServices(env, configuration);
      services.AddNeoTemplateServiceNameEntityChangePublishers(env, configuration);
    }
  }
}
