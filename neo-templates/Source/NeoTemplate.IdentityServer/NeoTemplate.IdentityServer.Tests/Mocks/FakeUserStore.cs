namespace NeoTemplate.IdentityServer.Tests.Mocks
{
  using System;
  using System.Threading;
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Identity;

  /// <summary>
  /// A minimal fake <see cref="IUserStore{TUser}"/> used to satisfy the
  /// <see cref="UserManager{TUser}"/> base constructor. The user manager methods that the
  /// tests exercise are overridden, so the members below are never invoked; they return
  /// neutral values (mirroring the previous loose Moq behaviour).
  /// </summary>
  /// <typeparam name="TUser">The user type.</typeparam>
  public class FakeUserStore<TUser> : IUserStore<TUser>
    where TUser : class
  {
    /// <inheritdoc />
    public Task<string> GetUserIdAsync(TUser user, CancellationToken cancellationToken) => Task.FromResult(string.Empty);

    /// <inheritdoc />
    public Task<string?> GetUserNameAsync(TUser user, CancellationToken cancellationToken) => Task.FromResult<string?>(null);

    /// <inheritdoc />
    public Task SetUserNameAsync(TUser user, string? userName, CancellationToken cancellationToken) => Task.CompletedTask;

    /// <inheritdoc />
    public Task<string?> GetNormalizedUserNameAsync(TUser user, CancellationToken cancellationToken) => Task.FromResult<string?>(null);

    /// <inheritdoc />
    public Task SetNormalizedUserNameAsync(TUser user, string? normalizedName, CancellationToken cancellationToken) => Task.CompletedTask;

    /// <inheritdoc />
    public Task<IdentityResult> CreateAsync(TUser user, CancellationToken cancellationToken) => Task.FromResult(IdentityResult.Success);

    /// <inheritdoc />
    public Task<IdentityResult> UpdateAsync(TUser user, CancellationToken cancellationToken) => Task.FromResult(IdentityResult.Success);

    /// <inheritdoc />
    public Task<IdentityResult> DeleteAsync(TUser user, CancellationToken cancellationToken) => Task.FromResult(IdentityResult.Success);

    /// <inheritdoc />
    public Task<TUser?> FindByIdAsync(string userId, CancellationToken cancellationToken) => Task.FromResult<TUser?>(null);

    /// <inheritdoc />
    public Task<TUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken) => Task.FromResult<TUser?>(null);

    /// <inheritdoc />
    public void Dispose()
    {
      GC.SuppressFinalize(this);
    }
  }
}
