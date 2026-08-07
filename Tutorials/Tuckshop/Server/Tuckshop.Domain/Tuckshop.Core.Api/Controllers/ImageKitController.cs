namespace Tuckshop.Core.Api.Controllers
{
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;
  using Tuckshop.Core.App.Services;

  /// <summary>
  ///  A controller for performing ImageKit operations.
  /// </summary>
  [Route("api/imagekit")]
  [ApiController]
  [Authorize]
  public class ImageKitController : ControllerBase
  {
    private readonly ImageKitService imageKitService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageKitController"/> class.
    /// </summary>
    /// <param name="imageKitService">The image kit service.</param>
    public ImageKitController(ImageKitService imageKitService)
    {
      this.imageKitService = imageKitService;
    }

    [HttpGet]
    public IActionResult GetAuthParams()
    {
      var authParams = this.imageKitService.GetAuthParams();
      return this.Ok(authParams);
    }
  }
}
