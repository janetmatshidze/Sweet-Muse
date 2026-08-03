namespace Tuckshop.IdentityServer.Tests.Mocks
{
  using System;

  /// <summary>
  /// A minimal fake <see cref="IServiceProvider"/> that resolves nothing. Used to satisfy the
  /// <see cref="Microsoft.AspNetCore.Identity.UserManager{TUser}"/> base constructor.
  /// </summary>
  public class FakeServiceProvider : IServiceProvider
  {
    /// <inheritdoc />
    public object? GetService(Type serviceType) => null;
  }
}
