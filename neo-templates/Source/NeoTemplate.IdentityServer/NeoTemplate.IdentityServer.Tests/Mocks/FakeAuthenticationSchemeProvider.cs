namespace NeoTemplate.IdentityServer.Tests.Mocks
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Authentication;

  /// <summary>
  /// A fake <see cref="IAuthenticationSchemeProvider"/> whose behaviour can be supplied by
  /// the test via optional delegates. The members that the tests do not configure return
  /// empty / null results (mirroring the previous loose Moq behaviour).
  /// </summary>
  public class FakeAuthenticationSchemeProvider : IAuthenticationSchemeProvider
  {
    private readonly Action<AuthenticationScheme>? addSchemeHandler;
    private readonly Action<string>? removeSchemeHandler;
    private readonly Func<string, AuthenticationScheme?>? getSchemeHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="FakeAuthenticationSchemeProvider"/> class.
    /// </summary>
    /// <param name="addSchemeHandler">Invoked when a scheme is added.</param>
    /// <param name="removeSchemeHandler">Invoked when a scheme is removed.</param>
    /// <param name="getSchemeHandler">Used to resolve a scheme by name.</param>
    public FakeAuthenticationSchemeProvider(
      Action<AuthenticationScheme>? addSchemeHandler = null,
      Action<string>? removeSchemeHandler = null,
      Func<string, AuthenticationScheme?>? getSchemeHandler = null)
    {
      this.addSchemeHandler = addSchemeHandler;
      this.removeSchemeHandler = removeSchemeHandler;
      this.getSchemeHandler = getSchemeHandler;
    }

    /// <inheritdoc />
    public void AddScheme(AuthenticationScheme scheme) => this.addSchemeHandler?.Invoke(scheme);

    /// <inheritdoc />
    public bool TryAddScheme(AuthenticationScheme scheme)
    {
      this.addSchemeHandler?.Invoke(scheme);
      return true;
    }

    /// <inheritdoc />
    public void RemoveScheme(string name) => this.removeSchemeHandler?.Invoke(name);

    /// <inheritdoc />
    public Task<AuthenticationScheme?> GetSchemeAsync(string name) => Task.FromResult(this.getSchemeHandler?.Invoke(name));

    /// <inheritdoc />
    public Task<IEnumerable<AuthenticationScheme>> GetAllSchemesAsync() => Task.FromResult(Enumerable.Empty<AuthenticationScheme>());

    /// <inheritdoc />
    public Task<IEnumerable<AuthenticationScheme>> GetRequestHandlerSchemesAsync() => Task.FromResult(Enumerable.Empty<AuthenticationScheme>());

    /// <inheritdoc />
    public Task<AuthenticationScheme?> GetDefaultAuthenticateSchemeAsync() => Task.FromResult<AuthenticationScheme?>(null);

    /// <inheritdoc />
    public Task<AuthenticationScheme?> GetDefaultChallengeSchemeAsync() => Task.FromResult<AuthenticationScheme?>(null);

    /// <inheritdoc />
    public Task<AuthenticationScheme?> GetDefaultForbidSchemeAsync() => Task.FromResult<AuthenticationScheme?>(null);

    /// <inheritdoc />
    public Task<AuthenticationScheme?> GetDefaultSignInSchemeAsync() => Task.FromResult<AuthenticationScheme?>(null);

    /// <inheritdoc />
    public Task<AuthenticationScheme?> GetDefaultSignOutSchemeAsync() => Task.FromResult<AuthenticationScheme?>(null);
  }
}
