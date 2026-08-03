namespace NeoTemplate.Extensions
{
  using System;
  using Microsoft.AspNetCore.Hosting;
  using Microsoft.AspNetCore.Routing;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.EntityFrameworkCore.Diagnostics;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.DependencyInjection;
  using Neo.AuthorisationServer.Api.StartupOptions;
  using Neo.AuthorisationServer.Models;
  using Neo.Extensions.DependencyInjection;
  using Neo.Identity.Api;
  using NeoTemplate.AuthorisationServer.Migrations.DesignTime;
  using NeoTemplate.AuthorisationServer.Models;

  /// <summary>
  /// Adds services for hosting authorisation server.
  /// </summary>
  public static class AuthorisationServerSetup
  {
    /// <summary>
    /// The prefix for the authorisation controllers and signalR hubs.
    /// </summary>
    public const string EndpointPrefix = "Authorisation";

    /// <summary>
    /// Add the notification services to the main apps service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="env">The web host environment.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="roleTypes">The role types.</param>
    public static void AddNeoTemplateAuthorisationServices(this IServiceCollection services, IWebHostEnvironment env, IConfiguration configuration, params Type[] roleTypes)
    {
      services.AddNeoDbContext<AuthorisationDbContext>(
        options =>
        {
          options.UseSqlServer(
            configuration.GetConnectionString(AuthorisationDbContext.ConnectionStringKey),
            options => options.MigrationsAssembly(typeof(DesignTimeDbContextFactory).Assembly.GetName().Name));

          options.ConfigureWarnings(builder =>
          {
            builder.Ignore(CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning);
            // ignore model change warnings because with audit entities relationships are required in the model that we do not want
            // in the database. This is so that the FK values in the audit tables are automatically handled by EF, but we can still
            // delete the entity without deleting the audit records.
            builder.Ignore(RelationalEventId.PendingModelChangesWarning);
          });
        });

      services.AddAuthorisationAsyncInitialisers<AuthorisationDbContext, NeoTemplateAuthorisationUser, AuthorisationDbAsyncInitializer>(env, configuration);

      services.AddHostedAuthorisationServer<AuthorisationDbContext, NeoTemplateAuthorisationUser, UserClaimMapper>(env, configuration, new AuthorisationStartupOptions<NeoTemplateAuthorisationUser>()
        .WithUserOptions(GetAuthorisationUserOptions())
        .ConfigureModelOptions(options => options.CacheUserMemberships = true)
        .WithEnrollmentHandler<UserEnrolmentHandler>()
        .WithRoleTypes(roleTypes));
    }

    private static AuthorisationUserOptions<NeoTemplateAuthorisationUser> GetAuthorisationUserOptions()
    {
      return new AuthorisationUserOptions<NeoTemplateAuthorisationUser>()
      {
        // This will filter the users in the user access page to only invited users.
        LookupPredicate = user => user.IsInvitedUser
      };
    }

    /// <summary>
    /// Adds the authorisation services controllers.
    /// </summary>
    /// <param name="mvcBuilder">The MVC builder.</param>
    /// <param name="env">The web host environment.</param>
    /// <param name="configuration">The configuration.</param>
    public static void AddNeoTemplateAuthorisationMvc(this IMvcBuilder mvcBuilder, IWebHostEnvironment env, IConfiguration configuration)
    {
      mvcBuilder.AddAuthorisationControllers<NeoTemplateAuthorisationUser>(env, configuration, controllerPrefix: EndpointPrefix);
    }

    /// <summary>
    /// Adds the notification services hubs.
    /// </summary>
    /// <param name="endpointRouteBuilder">The endpoint route builder.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="apiAuthenticationOptions">The API authentication options.</param>
    public static void MapNeoTemplateAuthorisationHubs(this IEndpointRouteBuilder endpointRouteBuilder, IConfiguration configuration, IApiAuthenticationOptions apiAuthenticationOptions)
    {
      endpointRouteBuilder.MapAuthorisationHubs(configuration, apiAuthenticationOptions, hubPrefix: EndpointPrefix);
    }
  }
}