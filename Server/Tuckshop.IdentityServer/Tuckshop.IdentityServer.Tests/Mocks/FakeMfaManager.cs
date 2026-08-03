namespace Tuckshop.IdentityServer.Tests.Mocks
{
  using Microsoft.AspNetCore.Identity;
  using Tuckshop.IdentityServer.App.Services;
  using Tuckshop.IdentityServer.Contracts.Registration;
  using Tuckshop.IdentityServer.Contracts.UserManagement.Queries;
  using Tuckshop.IdentityServer.Models;

  /// <summary>
  /// A fake <see cref="IMfaManager"/> whose responses can be configured by the test.
  /// </summary>
  public class FakeMfaManager : IMfaManager
  {
    /// <summary>
    /// Gets or sets a value indicating whether <see cref="UserRequiresTwoFactor"/> returns true.
    /// </summary>
    public bool UserRequiresTwoFactorResult { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether <see cref="SignInRequiresMFA"/> returns true.
    /// </summary>
    public bool SignInRequiresMFAResult { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether <see cref="NewUserRequiresTwoFactor"/> returns true.
    /// </summary>
    public bool NewUserRequiresTwoFactorResult { get; set; }

    /// <inheritdoc />
    public bool SignInRequiresMFA(SignInResult result, TuckshopApplicationUser user) => this.SignInRequiresMFAResult;

    /// <inheritdoc />
    public bool NewUserRequiresTwoFactor(IRegistrationUser user, int identityProviderId) => this.NewUserRequiresTwoFactorResult;

    /// <inheritdoc />
    public bool UserRequiresTwoFactor(UserLookup user) => this.UserRequiresTwoFactorResult;
  }
}
