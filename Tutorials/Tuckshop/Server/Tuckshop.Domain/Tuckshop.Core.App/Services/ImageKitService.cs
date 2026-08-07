namespace Tuckshop.Core.App.Services
{
  using Imagekit;
  using Microsoft.Extensions.Configuration;

  /// <summary>
  /// Service for generating ImageKit URLs for images.
  /// </summary>
  public class ImageKitService
  {
    private readonly ImageKitClient client;
    private readonly IConfiguration config;

    /// <summary>
    /// Constructor that initializes the ImageKitService with the provided configuration.
    /// </summary>
    /// <param name="config"> The configuration object containing ImageKit settings.</param>
    public ImageKitService(IConfiguration config)
    {
      this.config = config;
      this.client = new ImageKitClient
      {
        PrivateKey = config["ImageKit:PrivateKey"]
      };
    }

    /// <summary>
    ///  Gets authentication parameters for a client-side ImageKit upload. 
    /// </summary>
    /// <returns>Return the authentication parameters.</returns>
    public object GetAuthParams()
    {
      var authParams = this.client.Helper.GetAuthenticationParameters();

      return new
      {
        token = authParams.Token,
        expire = authParams.Expire,
        signature = authParams.Signature,
        PublicKey = this.config["ImageKit:PublicKey"]
      };
    }
  }
}
