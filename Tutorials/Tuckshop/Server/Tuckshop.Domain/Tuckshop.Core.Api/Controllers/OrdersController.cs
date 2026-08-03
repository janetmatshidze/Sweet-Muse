namespace Tuckshop.Core.Api.Controllers
{
  using Neo.Model.Controllers;
  using Tuckshop.Core.Models.Orders;
  using Tuckshop.Core.Models;
  using Tuckshop.Core.App.Services;

  /// <summary>
  /// The OrdersController class is an API controller that provides endpoints for managing orders in the system. It inherits from UpdateableControllerBase, which provides basic CRUD operations for the Order model.
  /// </summary>
  public class OrdersController : UpdateableControllerBase<Order, ModelDbContext, int>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="OrdersController"/> class.
    /// </summary>
    /// <param name="modelService">The model service for handling order-related operations.</param>
    public OrdersController(OrdersModelService modelService)
      : base(modelService, o => o.OrderId)
    {
    }
  }
}

