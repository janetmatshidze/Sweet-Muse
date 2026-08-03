namespace NeoTemplate.Extensions
{
  using Microsoft.AspNetCore.Hosting;
  using Microsoft.AspNetCore.Routing;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.EntityFrameworkCore.Diagnostics;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.DependencyInjection;
  using Neo.Extensions.DependencyInjection;
  using Neo.Identity;
  using Neo.Identity.Api;
  using Neo.NotificationServer.Options;
  using NeoTemplate.Notifications;
  using NeoTemplate.Notifications.App.Functions;
  using NeoTemplate.Notifications.App.Security;
  using NeoTemplate.Notifications.Migrations.Initializers;

  /// <summary>
  /// Notification server setup.
  /// </summary>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Future use")]
  public static class NotificationServerSetup
  {
    /// <summary>
    /// Add the notification services to the main apps service collection.
    /// </summary>
    /// <typeparam name="TUser">User type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="env">The web host environment.</param>
    /// <param name="configuration">The configuration.</param>
    public static void AddNeoTemplateNotificationServices<TUser>(this IServiceCollection services, IWebHostEnvironment env, IConfiguration configuration)
      where TUser : class, IClientUser
    {
      services.AddNeoDbContext<NotificationsDbContext>(
        options =>
        {
          options.UseSqlServer(
            configuration.GetConnectionString(NotificationsDbContext.ConnectionStringKey),
            builder => builder.MigrationsAssembly(typeof(NotificationsDbAsyncInitializer).Assembly.GetName().Name));

          options.ConfigureWarnings(builder =>
          {
            builder.Ignore(CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning);
          });
        });

      services.AddAsyncInitializer<NotificationsDbAsyncInitializer>();

      services.AddNeoTemplateNotificationsFileServices(env, configuration);
      services.AddHostedNotificationServer<NotificationsDbContext>(env, configuration, ConfigureTemplateOptions, ConfigureNotificationOptions);

      services.AddBulkNotifications<NotificationsDbContext, TUser>(env, configuration, typeof(NotificationsDbContext).Assembly);
    }

    /// <summary>
    /// Adds the notification services controllers.
    /// </summary>
    /// <param name="mvcBuilder">The MVC builder.</param>
    /// <param name="env">The web host environment.</param>
    /// <param name="configuration">The configuration.</param>
    public static void AddNeoTemplateNotificationsMvc(this IMvcBuilder mvcBuilder, IWebHostEnvironment env, IConfiguration configuration)
    {
      mvcBuilder.AddNotificationsMvc(new NotificationsApiConvention());
      mvcBuilder.AddBulkNotifications(new BulkNotificationsApiConvention());
    }

    /// <summary>
    /// Adds the notification services hubs.
    /// </summary>
    /// <param name="endpointRouteBuilder">The endpoint route builder.</param>
    /// <param name="apiAuthenticationOptions">The API authentication options.</param>
    public static void MapNeoTemplateNotificationsHubs(this IEndpointRouteBuilder endpointRouteBuilder, IApiAuthenticationOptions apiAuthenticationOptions)
    {
      endpointRouteBuilder.MapNotificationsHubs(apiAuthenticationOptions);
    }

    private static void AddNeoTemplateNotificationsFileServices(this IServiceCollection services, IWebHostEnvironment env, IConfiguration configuration)
    {
      /* 
       * TODO: Add file services for the notifications FileDescriptor and FileContext.
       *       Also uncomment 'WithAttachmentSupport' in the ConfigureNotificationOptions method below.
      */

      // services.AddXFileStorage<Neo.NotificationServer.Models.FileDescriptor, Neo.NotificationServer.Models.FileContext, NotificationsDbContext>(includeHttpServices = false);
    }

    private static void ConfigureTemplateOptions(TemplateStartupOptions templateStartupOptions)
    {
      // Register template layouts and styles here.

      templateStartupOptions.WithScribanTemplatingEngine(options => options.WithCustomFunctions(typeof(ScribanFunctions)));
    }

    private static void ConfigureNotificationOptions(NotificationStartupOptions notificationStartupOptions)
    {
      // Service bus message processor is not required when notification server is hosted by the main app.
      notificationStartupOptions.SuppressMessageProcessor();

      // notificationStartupOptions.WithAttachmentSupport(includeHttpServices: false);
    }
  }
}