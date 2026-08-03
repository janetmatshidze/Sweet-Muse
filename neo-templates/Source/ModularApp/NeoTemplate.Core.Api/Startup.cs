namespace NeoTemplate.Core.Api
{
  using Microsoft.AspNetCore.Builder;
  using Microsoft.AspNetCore.Hosting;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.DependencyInjection;
  using Neo.App;
  using Neo.Extensions.DependencyInjection;
  using Neo.Identity.Api;
  using NeoTemplate.Extensions;

  /// <summary>
  /// The startup of the application.
  /// </summary>
  /// <remarks>
  /// Initializes a new instance of the <see cref="Startup"/> class.
  /// </remarks>
  /// <param name="configuration">Config.</param>
  /// <param name="env">The hosting environment.</param>
  public class Startup(IConfiguration configuration, IWebHostEnvironment env) : NeoApiStartupBase(configuration, env)
  {
    /// <summary>
    /// The config key for the primary db connection string.
    /// </summary>
    public const string MainConnectionStringKey = "Main";

    /// <summary>
    /// This method gets called by the runtime. Use this method to add services to the container.
    /// </summary>
    /// <param name="serviceCollection">The services container.</param>
    public void ConfigureServices(IServiceCollection serviceCollection)
    {
      serviceCollection
       .AddNeoTemplateWebApiBase(this.Environment, this.Configuration, (builder, startupOptions) =>
       {
         builder
          .Add(NeoSetupActions.HealthChecks, services => services.AddNeoHealthChecks(this.Environment, this.Configuration))
          .Replace(NeoSetupActions.Mvc, services => services.AddNeoTemplateMvc(this.Environment, this.Configuration, startupOptions))
          .Add(NeoSetupActions.SwaggerOptions, services => services.AddNeoSwaggerOptions(this.Configuration))
          .Add(StartupActions.SecretVaults, services => services.AddNeoTemplateSecretVaults(this.Environment, this.Configuration))
          .Add(StartupActions.AlwaysEncrypted, services => services.AddNeoTemplateAlwaysEncrypted(this.Environment, this.Configuration))
          .Add(NeoSetupActions.Authentication, services => services.AddNeoTemplateAuthentication(this.Environment, this.Configuration))
          .Add(NeoSetupActions.AuthenticationProviders, services => services.AddNeoOpenIddictAuthenticationProviders(this.Environment, this.Configuration))
          .Add(StartupActions.UserDataServices, services => services.AddNeoTemplateUserDataServices(this.Environment, this.Configuration))
          .Add(StartupActions.DataServices, services => services.AddNeoTemplateDataServices(this.Environment, this.Configuration))
          .Add(StartupActions.ModelServices, services => services.AddNeoTemplateModelServices(this.Environment, this.Configuration))
          .Add(StartupActions.ProcessingServices, services => services.AddNeoTemplateProcessingServices(this.Environment, this.Configuration))
          .Add(StartupActions.IntegrationServices, services => services.AddNeoTemplateIntegrationServices(this.Environment, this.Configuration))
          .Add(StartupActions.Jobs, services => services.AddNeoTemplateJobs(this.Environment, this.Configuration))
          .Add(StartupActions.IntegrityChecking, services => services.AddNeoTemplateIntegrityChecking(this.Environment, this.Configuration))
          .Add(StartupActions.FileStorageServices, services => services.AddNeoTemplateFileStorageServices(this.Environment, this.Configuration))
          .Add(StartupActions.SignalR, services => services.AddNeoTemplateSignalR(this.Environment, this.Configuration))
          .Add(StartupActions.EntityChangePublishers, services => services.AddNeoTemplateEntityChangePublishers(this.Environment, this.Configuration))
          .Add(StartupActions.Caches, services => services.AddNeoTemplateCaches(this.Environment, this.Configuration))
          .Add(StartupActions.MultiTenancy, services => services.AddNeoTemplateMultiTenancy(this.Environment, this.Configuration))
          .Add(StartupActions.Modules, services => services.AddNeoTemplateModules(this.Environment, this.Configuration))
          .Add(NeoSetupActions.Swagger, services => services.AddNeoTemplateSwagger(this.Environment, this.Configuration))
          .Add(StartupActions.Logging, services => services.AddNeoTemplateLogging(this.Environment, this.Configuration));

         serviceCollection.AddAuthorization(options => options.AddIsServicePolicy(serviceCollection));

         this.ConfigureServicesOverrides(builder);
       });
    }

    /// <summary>
    /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    /// </summary>
    /// <param name="app">The app builder.</param>
    /// <param name="apiAuthenticationOptions">Api authentication options.</param>
    public void Configure(
      IApplicationBuilder app,
      IApiAuthenticationOptions apiAuthenticationOptions)
    {
      app
        .UseNeoWebApi(this.Environment, this.Configuration, builder =>
        {
          builder.Replace(NeoSetupActions.Authentication, app =>
          {
            app.UseNeoAuthentication(this.Environment, this.Configuration);
            app.UseAuthorization();
          });

          builder.Replace(NeoSetupActions.Endpoints, app =>
          {
            app.UseEndpoints(endpoints =>
            {
              endpoints.MapNeoHealthCheckEndpoints();
              endpoints.MapNeoTemplateAuthorisationHubs(this.Configuration, apiAuthenticationOptions);
              endpoints.MapNeoTemplateNotificationsHubs(apiAuthenticationOptions);
              endpoints.MapControllers().RequireAuthorization();
            });
          });

          builder.Replace(NeoSetupActions.Cors, app =>
          {
            app.UseCors(corsPolicyBuilder =>
            {
              // AllowCredentials is required for signalR. Neo doesn't add this by default.
              corsPolicyBuilder.SetIsOriginAllowed(_ => true).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
            });
          });

          this.ConfigureOverrides(builder);
        })
        .UseNeoTemplateSwagger(this.Environment, this.Configuration);
    }
  }
}