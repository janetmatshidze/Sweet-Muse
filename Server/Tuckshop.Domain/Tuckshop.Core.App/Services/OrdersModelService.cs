namespace Tuckshop.Core.App.Services
{
  using Microsoft.AspNetCore.Authorization;
  using Neo.Model.Services;
  using Tuckshop.Core.Models;
  using Tuckshop.Core.Models.Orders;

  [Authorize]
  /// <summary>
  /// Service to working with the Orders aggregate.
  /// </summary>
  public class OrdersModelService : UpdateableModelService<Order, ModelDbContext, int>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="OrdersModelService"/> class.
    /// </summary>
    /// <param name="context">The db context.</param>
    public OrdersModelService(ModelDbContext context)
      : base(context, new ModelServiceOptions<Order>(o => o.OrderDetails))
    {
    }
  }
}
