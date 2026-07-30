namespace Tuckshop.IdentityServer.Tests.Mocks
{
  using Microsoft.AspNetCore.Identity;

  /// <summary>
  /// A minimal fake <see cref="ILookupNormalizer"/> that returns the supplied value unchanged.
  /// Used to satisfy the <see cref="UserManager{TUser}"/> base constructor.
  /// </summary>
  public class FakeLookupNormalizer : ILookupNormalizer
  {
    /// <inheritdoc />
    public string? NormalizeName(string? name) => name;

    /// <inheritdoc />
    public string? NormalizeEmail(string? email) => email;
  }
}
