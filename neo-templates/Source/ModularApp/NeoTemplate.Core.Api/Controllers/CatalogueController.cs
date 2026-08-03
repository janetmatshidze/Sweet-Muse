namespace NeoTemplate.Core.Api.Controllers
{
  using Microsoft.AspNetCore.Mvc;
  using NeoTemplate.Core.App.Services;

  /// <summary>
  /// Controller for catalogue data.
  /// </summary>
  /// <param name="catalogueModelService">Catalogue model service.</param>
  [ApiController]
  [Route("api/catalogue")]
  public class CatalogueController(CatalogueModelService catalogueModelService) : ControllerBase
  {
  }
}