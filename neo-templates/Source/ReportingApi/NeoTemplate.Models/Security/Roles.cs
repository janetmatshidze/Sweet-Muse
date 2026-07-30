namespace NeoTemplate.Security
{
  using Neo.AuthorisationServer.Client;

  /// <summary>
  /// The reporting roles.
  /// </summary>
  public class Roles : IRoles
  {
    /// <summary>
    /// The example report enum.
    /// </summary>
    public enum ExampleReport
    {
      /// <summary>
      /// The view report enum rule value.
      /// </summary>
      View,

      /// <summary>
      /// The download report enum rule value.
      /// </summary>
      Download,
    }

    /// <inheritdoc/>
    public string ResourceName => "Reporting";

    /// <inheritdoc/>
    public string DisplayName => "Reporting";
  }
}
