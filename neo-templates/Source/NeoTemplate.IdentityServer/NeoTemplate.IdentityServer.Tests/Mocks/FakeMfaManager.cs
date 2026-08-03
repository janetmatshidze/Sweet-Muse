namespace NeoTemplate.IdentityServer.Tests.Mocks
{
  using Microsoft.AspNetCore.Identity;
  using NeoTemplate.IdentityServer.App.Services;
  using NeoTemplate.IdentityServer.Contracts.Registration;
  using NeoTemplate.IdentityServer.Contracts.UserManagement.Queries;
  using NeoTemplate.IdentityServer.Models;

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
    public bool SignInRequiresMFA(SignInResult result, NeoTemplateApplicationUser user) => this.SignInRequiresMFAResult;

    /// <inheritdoc />
    public bool NewUserRequiresTwoFactor(IRegistrationUser user, int identityProviderId) => this.NewUserRequiresTwoFactorResult;

    /// <inheritdoc />
    public bool UserRequiresTwoFactor(UserLookup user) => this.UserRequiresTwoFactorResult;
  }
}
