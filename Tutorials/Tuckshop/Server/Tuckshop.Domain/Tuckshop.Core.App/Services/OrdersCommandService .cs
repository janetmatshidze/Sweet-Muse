namespace Tuckshop.Core.App.Services
{
  using System;
  using System.Linq;
  using System.Threading.Tasks;
  using Neo.Identity;
  using Tuckshop.Core.Models.Identity;
  using Tuckshop.Core.Models.Orders;
  using Tuckshop.Core.Models.Orders.Commands;

  /// <summary>
  ///  An Orders Command Service that handles the creation of orders and their details.
  /// </summary>
  public class OrdersCommandService
  {

    private readonly OrdersModelService modelService;
    private readonly IProductPricesService priceService;
    private readonly IUserResolver<User> userResolver;

    public OrdersCommandService(
      OrdersModelService modelService,
      IProductPricesService priceService,
      IUserResolver<User> userResolver)
    {
      this.modelService = modelService;
      this.priceService = priceService;
      this.userResolver = userResolver;
    }

    /// <summary>
    ///  Will create a new order with the provided command.
    ///  Sets the order details and saves the order to the database.
    /// </summary>
    /// <param name="command">The create order command.</param>
    /// <returns>A task awaiting the order creation.</returns>
    public async Task<Order> CreateOrderAsync(CreateOrder command)
    {
      Order order = await this.CreateOrderEntityAsync(command).ConfigureAwait(false);
      this.modelService.AddEntity(order);
      await this.modelService.SaveAsync(order).ConfigureAwait(false);
      return order;
    }

    private async Task<Order> CreateOrderEntityAsync(CreateOrder command)
    {
      var order = new Order(command.CustomerName);
      var productIds = command.OrderDetails.Select(od => od.ProductId).ToHashSet();
      var prices = await this.priceService.GetProductPricesAsync(productIds).ConfigureAwait(false);

      foreach (var od in command.OrderDetails)
      {
        order.AddDetail(od.ProductId, od.Quantity, prices[od.ProductId]);
      }
      return order;
    }

    /// <summary>
    /// Will complete an order from the provided command.
    /// </summary>
    /// <param name="command">The complete order command.</param>
    /// <returns>A task awaiting the order completion.</returns>
    public async Task CompleteOrderAsync(CompleteOrder command)
    {
      await this.ProcessUserEvent(
        command.OrderId,
        (order, user) => order.Complete(user.UserId))
        .ConfigureAwait(false);
    }

    /// <summary>
    ///  Will cancel the order from the provided command.
    /// </summary>
    /// <param name="command">The cancel order command.</param>
    /// <returns>A task awaiting the order cancellation.</returns>
    public async Task CancelOrderAsync(CancelOrder command)
    {
      await this.ProcessUserEvent(
        command.OrderId,
        (order, user) => order.Cancel(user.UserId, command.Reason))
        .ConfigureAwait(false);
    }

    private async Task ProcessUserEvent(int orderId, Action<Order, User> handler)
    {
      var order = await this.modelService.GetByIdAsync(orderId).ConfigureAwait(false);
      var user = await this.userResolver.GetUserAsync().ConfigureAwait(false);
      handler(order, user);
      await this.modelService.SaveChangesAsync().ConfigureAwait(false);
    }
  }
}
