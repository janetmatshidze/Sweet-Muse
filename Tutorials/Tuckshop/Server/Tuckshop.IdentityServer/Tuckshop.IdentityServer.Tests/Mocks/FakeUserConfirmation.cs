namespace Tuckshop.IdentityServer.Tests.Mocks
{
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Identity;

  /// <summary>
  /// A minimal fake <see cref="IUserConfirmation{TUser}"/>. User confirmation is not
  /// exercised by the tests. Used to satisfy the sign in manager base constructor.
  /// </summary>
  /// <typeparam name="TUser">The user type.</typeparam>
  public class FakeUserConfirmation<TUser> : IUserConfirmation<TUser>
    where TUser : class
  {
    /// <inheritdoc />
    public Task<bool> IsConfirmedAsync(UserManager<TUser> manager, TUser user) => Task.FromResult(true);
  }
}
