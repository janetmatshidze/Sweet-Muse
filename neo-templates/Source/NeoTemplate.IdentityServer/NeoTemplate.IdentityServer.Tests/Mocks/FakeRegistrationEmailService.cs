namespace NeoTemplate.IdentityServer.Tests.Mocks
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using NeoTemplate.IdentityServer.App.Services;
  using NeoTemplate.IdentityServer.Models;
  using NeoTemplate.IdentityServer.Models.UserManagement;

  /// <summary>
  /// A fake <see cref="IRegistrationEmailService"/> that records the notifications it is
  /// asked to send so that tests can assert against them.
  /// </summary>
  public class FakeRegistrationEmailService : IRegistrationEmailService
  {
    /// <summary>
    /// Gets the users that a verification email was sent to.
    /// </summary>
    public List<NeoTemplateApplicationUser> SentToUsers { get; } = new List<NeoTemplateApplicationUser>();

    /// <summary>
    /// Gets the user invites that were sent.
    /// </summary>
    public List<UserInvite> SentUserInvites { get; } = new List<UserInvite>();

    /// <inheritdoc />
    public Task SendVerificationEmailAsync(NeoTemplateApplicationUser user)
    {
      this.SentToUsers.Add(user);
      return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task SendUserInviteEmailAsync(UserInvite userInvite)
    {
      this.SentUserInvites.Add(userInvite);
      return Task.CompletedTask;
    }
  }
}
