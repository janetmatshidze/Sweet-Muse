namespace NeoTemplate.AuthorisationServer.Models
{
  using System;
  using System.Linq;
  using System.Threading.Tasks;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Configuration;
  using Neo.AuthorisationServer.Models;
  using Neo.AuthorisationServer.Services;
  using Neo.Extensions;
  using NeoTemplate.IdentityServer.Services;

  /// <summary>
  /// Service that handles when a new user is created in authorisation server.
  /// </summary>
  public class UserEnrolmentHandler : IUserEnrolmentOptions
  {
    private readonly IConfiguration configuration;
    private readonly IIdentityServerService identityService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserEnrolmentHandler"/> class.
    /// </summary>
    /// <param name="configuration">App config.</param>
    /// <param name="identityService">Identity server service.</param>
    public UserEnrolmentHandler(
      IConfiguration configuration,
      IIdentityServerService identityService)
    {
      this.configuration = configuration;
      this.identityService = identityService;
    }

    /// <inheritdoc/>
    public async Task EnrollingUserAsync(UserEnrolmentContext context)
    {
      var userEnrolmentOptions = this.configuration.GetOptions<UserEnrolmentHandlerOptions>();

      if (userEnrolmentOptions.Administrators!.Contains(context.User.UserName, StringComparer.OrdinalIgnoreCase))
      {
        // enrol them in the administrators group
        if (context.AdminsUserGroupId != null)
        {
          // add this user to the admins group
          context.DbContext.Memberships.Add(new Membership() { UserId = context.UserId, UserGroupId = context.AdminsUserGroupId.Value });
        }
      }
      else if (context.User is NeoTemplateAuthorisationUser user && user.IsInvitedUser)
      {
        var invitedUser = await this.identityService.FindInvitedUserAsync(context.User);

        if (invitedUser?.AddToGroupId != null)
        {
          context.DbContext.Memberships.Add(new Membership() { UserId = context.UserId, UserGroupId = invitedUser.AddToGroupId.Value });
        }
      }
    }
  }
}