namespace Tuckshop.IdentityServer.Tests.Mocks
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using Tuckshop.IdentityServer.App.Services;
  using Tuckshop.IdentityServer.Models;
  using Tuckshop.IdentityServer.Models.UserManagement;

  /// <summary>
  /// A fake <see cref="IRegistrationEmailService"/> that records the notifications it is
  /// asked to send so that tests can assert against them.
  /// </summary>
  public class FakeRegistrationEmailService : IRegistrationEmailService
  {
    /// <summary>
    /// Gets the users that a verification email was sent to.
    /// </summary>
    public List<TuckshopApplicationUser> SentToUsers { get; } = new List<TuckshopApplicationUser>();

    /// <summary>
    /// Gets the user invites that were sent.
    /// </summary>
    public List<UserInvite> SentUserInvites { get; } = new List<UserInvite>();

    /// <inheritdoc />
    public Task SendVerificationEmailAsync(TuckshopApplicationUser user)
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
