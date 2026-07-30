namespace Tuckshop.IdentityServer.Tests.Mocks
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using Neo.AuthorisationServer.Client;

  /// <summary>
  /// A fake <see cref="IAuthorisationService"/> that treats the service as initialised and
  /// every role check as satisfied, which is what the user management tests require.
  /// </summary>
  public class FakeAuthorisationService : IAuthorisationService
  {
    /// <inheritdoc />
    public bool HasInitialized { get; private set; } = true;

    /// <inheritdoc />
    public Task InitializeAsync()
    {
      this.HasInitialized = true;
      return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task AssertUserHasRoleAsync(Enum role) => Task.CompletedTask;

    /// <inheritdoc />
    public Task<bool> UserHasRoleAsync(Enum role) => Task.FromResult(true);

    /// <inheritdoc />
    public Task<Dictionary<Enum, bool>> UserHasRolesAsync(IEnumerable<Enum> role) =>
      Task.FromResult((role ?? Enumerable.Empty<Enum>()).ToDictionary(item => item, _ => true));
  }
}
