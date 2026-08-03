namespace NeoTemplate.Api
{
  using Microsoft.AspNetCore.Builder;
  using Microsoft.AspNetCore.Hosting;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.DependencyInjection;
  using Neo.App;
  using Neo.Extensions.DependencyInjection;

  /// <summary>
  /// The startup of the application
  /// </summary>
  public class Startup : NeoApiStartupBase
  {
    /// <summary>
    /// The config key for the primary db connection string
    /// </summary>
    public const string MainConnectionStringKey = "Main";

    /// <summary>
    /// Initializes a new instance of the <see cref="Startup"/> class.
    /// </summary>
    /// <param name="configuration">Config</param>
    /// <param name="env">The hosting environment</param>
    public Startup(IConfiguration configuration, IWebHostEnvironment env)
      : base(configuration, env)
    {
    }

    /// <summary>
    /// This method gets called by the runtime. Use this method to add services to the container.
    /// </summary>
    /// <param name="serviceCollection">The services container</param>
    public void ConfigureServices(IServiceCollection serviceCollection)
    {
      serviceCollection
       .AddNeoTemplateServiceNameWebApiBase(this.Environment, this.Configuration, builder =>
       {
         builder
           .Add(NeoSetupActions.SwaggerOptions, services => services.AddNeoSwaggerOptions(this.Configuration))
           .Add(NeoSetupActions.Authentication, services => services.AddNeoAuthentication(this.Environment, this.Configuration))
           .Add(StartupActions.UserDataServices, services => services.AddNeoTemplateServiceNameUserDataServices(this.Environment, this.Configuration))
           .Add(StartupActions.DataServices, services => services.AddNeoTemplateServiceNameDataServices(this.Environment, this.Configuration))
           .Add(StartupActions.ModelServices, services => services.AddNeoTemplateServiceNameModelServices(this.Environment, this.Configuration))
           .Add(StartupActions.Controllers, services => services.AddNeoTemplateServiceNameControllers(this.Environment, this.Configuration))
           .Add(StartupActions.EntityChangePublishers, services => services.AddNeoTemplateServiceNameEntityChangePublishers(this.Environment, this.Configuration))
           .Add(StartupActions.Caches, services => services.AddNeoTemplateServiceNameCaches(this.Environment, this.Configuration))
           .Add(NeoSetupActions.Swagger, services => services.AddNeoTemplateServiceNameSwagger(this.Environment, this.Configuration));

         this.ConfigureServicesOverrides(builder);
       });
    }

    /// <summary>
    /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    /// </summary>
    /// <param name="app">The app builder</param>
    public void Configure(IApplicationBuilder app)
    {
      app
        .UseNeoWebApi(this.Environment, this.Configuration, builder =>
        {
          builder.Replace(NeoSetupActions.Authentication, app =>
          {
            app.UseNeoAuthentication(this.Environment, this.Configuration);
            app.UseAuthorization();
          });

          this.ConfigureOverrides(builder);
        })
        .UseNeoTemplateServiceNameSwagger(this.Environment, this.Configuration);
    }
  }
}