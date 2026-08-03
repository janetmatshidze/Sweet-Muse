namespace NeoTemplate.IdentityServer.Tests.Mocks
{
  using Microsoft.AspNetCore.Identity;

  /// <summary>
  /// A minimal fake <see cref="IPasswordHasher{TUser}"/>. Password hashing is not exercised
  /// by the tests, so these members return neutral values.
  /// </summary>
  /// <typeparam name="TUser">The user type.</typeparam>
  public class FakePasswordHasher<TUser> : IPasswordHasher<TUser>
    where TUser : class
  {
    /// <inheritdoc />
    public string HashPassword(TUser user, string password) => string.Empty;

    /// <inheritdoc />
    public PasswordVerificationResult VerifyHashedPassword(TUser user, string hashedPassword, string providedPassword) => PasswordVerificationResult.Failed;
  }
}
