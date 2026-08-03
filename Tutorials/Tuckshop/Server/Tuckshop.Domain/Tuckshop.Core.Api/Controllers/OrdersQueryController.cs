namespace Tuckshop.Core.Api.Controllers
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;
  using Tuckshop.Core.App.Services;
  using Microsoft.EntityFrameworkCore;
  using Tuckshop.Core.Models.Orders.Queries;

  /// <summary>
  /// A controller for performing order queries.
  /// </summary>
  [Route("api/orders/query")]
  [ApiController]
  [Authorize]
  public class OrdersQueryController : ControllerBase
  {

    private readonly OrdersQueryService queryService;

    /// <summary>
    ///  Initializes a new instance of the <see cref="OrdersQueryController"/> class.
    /// </summary>
    /// <param name="queryService">The orders query service.</param>
    public OrdersQueryController(OrdersQueryService queryService)
    {
      this.queryService = queryService;
    }

    /// <summary>
    /// Gets the Orders for the given criteria.
    /// </summary>
    /// <param name="criteria">The order lookup criteria.</param>
    /// <returns>List of Orders.</returns>
    [HttpGet("lookup")]
    public async Task<List<OrderLookup>> GetOrderLookupAsync([FromQuery] OrderLookupCriteria criteria)
    {
      return await this.queryService.GetOrderLookupAsync(criteria);
    }
  }
}
