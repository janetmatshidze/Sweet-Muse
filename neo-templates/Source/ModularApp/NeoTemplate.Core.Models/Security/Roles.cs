namespace NeoTemplate.Core.Security
{
  using Neo.AuthorisationServer.Client;

  /// <summary>
  /// NeoTemplate domain roles.
  /// </summary>
  public class Roles : IRoles
  {
    /// <summary>
    /// Roles for the test controller.
    /// </summary>
    public enum TestApi
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
      Execute,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }

    /// <inheritdoc/>
    public string ResourceName => "NeoTemplate";

    /// <inheritdoc/>
    public string DisplayName => "NeoTemplate";
  }
}