namespace NeoTemplate.Api.Config
{
  using System;

  /// <summary>
  /// Represents the AuthorizationSettings config class.
  /// </summary>
  public class AuthenticationSettings
  {
    /// <summary>
    /// Gets or sets the Authentication Authority Url value.
    /// </summary>
    public Uri AuthenticationAuthorityUrl { get; set; } = new Uri("http://localhost:5000");

    /// <summary>
    /// Gets or sets the Authorization Url value.
    /// </summary>
    public Uri AuthorizationUrl { get; set; } = new Uri("http://localhost:5000/connect/authorize");

    /// <summary>
    /// Gets or sets the Token Url value.
    /// </summary>
    public Uri TokenUrl { get; set; } = new Uri("http://localhost:5000/connect/token");
  }
}
