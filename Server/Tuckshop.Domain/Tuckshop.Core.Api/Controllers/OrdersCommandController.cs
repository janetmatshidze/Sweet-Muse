namespace Tuckshop.Core.Api.Controllers
{
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Mvc;
  using Tuckshop.Core.App.Services;
  using Tuckshop.Core.Models.Orders;
  using Tuckshop.Core.Models.Orders.Commands;

  /// <summary>
  /// Orders command controller.
  /// </summary>

  [ApiController]
  [Route("api/orders/commands")]
  public class OrdersCommandController : ControllerBase
  {
    private readonly OrdersCommandService _ordersCommandService;

    /// <summary>
    /// Creates a new instance of the <see cref="OrdersCommandController"/> class.
    /// </summary>
    /// <param name="ordersCommandService">Orders command service.</param>
    public OrdersCommandController(OrdersCommandService ordersCommandService)
    {
      this._ordersCommandService = ordersCommandService;
    }

    /// <summary>
    /// Creates a new order.
    /// </summary>
    /// <param name="command">The create order command.</param>
    /// <returns>The created order.</returns>
    [HttpPost("create")]
    public virtual Task<Order> CreateOrder([FromBody] CreateOrder command)
    {
      return this._ordersCommandService.CreateOrderAsync(command);
    }

    /// <summary>
    /// Completes an existing order.
    /// </summary>
    /// <param name="command">The complete order command.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Route("complete")]
    [HttpPut]
    public virtual Task CompleteOrder([FromBody] CompleteOrder command)
    {
      return this._ordersCommandService.CompleteOrderAsync(command);
    }

    /// <summary>
    /// Cancels an exisiting order.
    /// </summary>
    /// <param name="command">The cancel order command.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Route("cancel")]
    [HttpPut]
    public virtual Task CancelOrder([FromBody] CancelOrder command)
    {
      return this._ordersCommandService.CancelOrderAsync(command);
    }
  }
}
