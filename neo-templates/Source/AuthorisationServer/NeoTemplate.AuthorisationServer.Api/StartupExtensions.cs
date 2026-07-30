namespace NeoTemplate.AuthorisationServer.Api
{
  using System;
  using Microsoft.AspNetCore.Hosting;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.DependencyInjection;
  using Neo.Extensions.DependencyInjection;
  using NeoTemplate.AuthorisationServer.Models;
  using Serilog;
  using Serilog.Filters;

  /// <summary>
  /// The Authorisation Server Startup Extensions class
  /// </summary>
  public static class StartupExtensions
  {
    /// <summary>
    /// Configures serilog from the app config.
    /// </summary>    
    /// <param name="services">services.</param>
    /// <param name="environment">environment</param>
    /// <param name="configuration">configuration.</param>
    /// <returns>IServiceCollection</returns>
    public static IServiceCollection AddAuthorisationLogging(
      this IServiceCollection services,
      IWebHostEnvironment environment,
      IConfiguration configuration)
    {
      LoggerConfiguration loggerConfig = new LoggerConfiguration()
          // serilog-aspnetcore picks up and logs exceptions, so we filter out logs from the standard middleware to prevent duplicate logs.
          .Filter.ByExcluding(Matching.FromSource<Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddleware>())
          .Filter.ByExcluding(Matching.FromSource<Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddleware>())
          // Filters out 'info' logs from Microsoft.AspNetCore when the request path is a health check URL
          .Filter.ByExcluding(Matching.WithProperty<string>(
            propertyName: "RequestPath",
            requestPath => requestPath.EndsWith("/health/live", StringComparison.InvariantCultureIgnoreCase) || requestPath.EndsWith("/health/ready", StringComparison.InvariantCultureIgnoreCase)))
          .Enrich.FromLogContext()
          .ReadFrom.Configuration(configuration);

      Log.Logger = loggerConfig.CreateLogger();

      services.AddLogCleanupForSqlServer<AuthorisationDbContext>(configuration);

      return services;
    }

    /// <summary>
    /// Adds integrations to other services.
    /// </summary>
    /// <param name="services">services.</param>
    /// <param name="environment">environment</param>
    /// <param name="configuration">configuration.</param>
    /// <returns>IServiceCollection</returns>
    public static IServiceCollection AddIntegrations(
      this IServiceCollection services,
      IWebHostEnvironment environment,
      IConfiguration configuration)
    {
      services.AddIdentityClientServices(configuration, environment);

      return services;
    }
  }
}
