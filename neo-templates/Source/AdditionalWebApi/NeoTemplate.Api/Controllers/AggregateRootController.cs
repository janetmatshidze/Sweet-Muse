namespace NeoTemplate.Api.Controllers
{
  using Microsoft.AspNetCore.Mvc;
  using Neo.Model.Controllers;
  using Neo.Model.Services;
  using NeoTemplate.Models;

  /// <summary>
  /// An example entity controller (should be removed or renamed)
  /// </summary>
  [Route("api/aggregate-root")]
  [ApiController]
  public class AggregateRootController : UpdateableControllerBase<AggregateRoot, AggregateRootDbContext, int>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateRootController"/> class.
    /// </summary>
    /// <param name="modelService">The Example Entity ModelService</param>
    public AggregateRootController(
      IUpdateableModelService<AggregateRoot, AggregateRootDbContext, int> modelService)
      : base(modelService, ee => ee.AggregateRootId)
    {
    }
  }
}
