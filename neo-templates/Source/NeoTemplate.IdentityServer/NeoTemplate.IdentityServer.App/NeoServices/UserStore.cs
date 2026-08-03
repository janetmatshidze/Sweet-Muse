namespace Neo.IdentityServer.App.Services
{
  using System;
  using System.Collections.Generic;
  using System.Threading;
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Identity;
  using Microsoft.EntityFrameworkCore;
  using NeoTemplate.IdentityServer;
  using NeoTemplate.IdentityServer.Models;

  /// <summary>
  /// Represents a user store for audit operations.
  /// </summary>
  public sealed class UserStore : Identity.IUserStore<NeoTemplateApplicationUser>
  {
    private readonly IdentityDbContext identityContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserStore"/> class.
    /// </summary>
    /// <param name="identityContext">The identity context.</param>
    public UserStore(IdentityDbContext identityContext)
    {
      this.identityContext = identityContext;
    }

    /// <inheritdoc/>
    public Task<IdentityResult> CreateAsync(NeoTemplateApplicationUser user, CancellationToken cancellationToken)
    {
      this.identityContext.Add(user);
      return Task.FromResult(IdentityResult.Success);
    }

    /// <inheritdoc/>
    public Task<IdentityResult> DeleteAsync(NeoTemplateApplicationUser user, CancellationToken cancellationToken)
    {
      throw new InvalidOperationException("Users should be deleted through AspNetIdentity");
    }

    /// <inheritdoc/>
    public void Dispose()
    {
      this.identityContext.Dispose();
    }

    /// <inheritdoc/>
    public Task<NeoTemplateApplicationUser?> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
      return this.identityContext.Users.FindAsync(new[] { userId }, cancellationToken: cancellationToken).AsTask();
    }

    /// <inheritdoc/>
    public Task<NeoTemplateApplicationUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
      return this.identityContext.Users.FirstOrDefaultAsync(user => user.NormalizedUserName == normalizedUserName, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<string?> GetNormalizedUserNameAsync(NeoTemplateApplicationUser user, CancellationToken cancellationToken)
    {
      return Task.FromResult(user.NormalizedUserName);
    }

    /// <inheritdoc/>
    public Task<string> GetUserIdAsync(NeoTemplateApplicationUser user, CancellationToken cancellationToken)
    {
      return Task.FromResult(user.Id);
    }

    /// <inheritdoc/>
    public Task<string?> GetUserNameAsync(NeoTemplateApplicationUser user, CancellationToken cancellationToken)
    {
      return Task.FromResult(user.UserName);
    }

    /// <inheritdoc/>
    public Task SetNormalizedUserNameAsync(NeoTemplateApplicationUser? user, string? normalizedName, CancellationToken cancellationToken)
    {
      user?.NormalizedUserName = normalizedName;

      return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task SetUserNameAsync(NeoTemplateApplicationUser? user, string? userName, CancellationToken cancellationToken)
    {
      user?.UserName = userName;

      return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<IdentityResult> UpdateAsync(NeoTemplateApplicationUser? user, CancellationToken cancellationToken)
    {
      if (user != null)
      {
        this.identityContext.Update(user);
      }
      return Task.FromResult(IdentityResult.Success);
    }

    /// <inheritdoc/>
    public Task ClearCachedUsersAsync(List<NeoTemplateApplicationUser> users, CancellationToken cancellationToken)
    {
      return Task.CompletedTask;
    }
  }
}
