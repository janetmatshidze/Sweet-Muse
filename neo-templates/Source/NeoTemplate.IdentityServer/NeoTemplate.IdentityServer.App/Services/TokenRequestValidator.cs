namespace NeoTemplate.IdentityServer.App.Services
{
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Identity;
  using Neo.IdentityServer.App.OpenIddict.Validators;
  using NeoTemplate.IdentityServer.Models;

  /// <summary>
  /// Request validator to validate the token and add additional claims.
  /// </summary>
  public class TokenRequestValidator : ICustomTokenRequestValidator
  {
    private readonly UserManager<NeoTemplateApplicationUser> userManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenRequestValidator"/> class.
    /// </summary>
    /// <param name="userManager">The user manager.</param>
    public TokenRequestValidator(UserManager<NeoTemplateApplicationUser> userManager)
    {
      this.userManager = userManager;
    }

    /// <inheritdoc/>
    public Task ValidateAsync(CustomTokenRequestValidationContext context)
    {
      // not a client integration, make sure the user is not blocked.
      var username = (context.User as NeoTemplateApplicationUser)?.UserName;

      if (!string.IsNullOrEmpty(username))
      {
        NeoTemplateApplicationUser? appUser = context.User as NeoTemplateApplicationUser;

        if (appUser == null)
        {
          context.IsError = true;
          context.Error = "No user found.";
        }
        else if (!appUser.IsActive)
        {
          context.IsError = true;
          context.Error = "Blocked.";
        }
        else if (appUser.ForceLogout)
        {
          context.IsError = true;
          context.Error = "You have been logged out.";
        }
      }

      return Task.CompletedTask;
    }
  }
}
