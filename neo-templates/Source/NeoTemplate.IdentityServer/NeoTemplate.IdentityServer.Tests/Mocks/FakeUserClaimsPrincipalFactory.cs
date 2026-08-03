namespace NeoTemplate.IdentityServer.Tests.Mocks
{
  using System.Security.Claims;
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Identity;

  /// <summary>
  /// A minimal fake <see cref="IUserClaimsPrincipalFactory{TUser}"/>. Claims-principal
  /// creation is not exercised by the tests. Used to satisfy the sign in manager base
  /// constructor.
  /// </summary>
  /// <typeparam name="TUser">The user type.</typeparam>
  public class FakeUserClaimsPrincipalFactory<TUser> : IUserClaimsPrincipalFactory<TUser>
    where TUser : class
  {
    /// <inheritdoc />
    public Task<ClaimsPrincipal> CreateAsync(TUser user) => Task.FromResult(new ClaimsPrincipal(new ClaimsIdentity()));
  }
}
