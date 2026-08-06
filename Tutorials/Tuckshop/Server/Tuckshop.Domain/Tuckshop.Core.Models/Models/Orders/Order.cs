namespace Tuckshop.Core.Models.Orders
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;
  using Neo.Model;
  using Neo.Model.Exceptions;
  using Neo.Model.ValueObjects;

  /// <summary>
  /// Order class to represent an order in the system.
  /// </summary>
  public class Order : ModelBase<Order>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="Order"/> class.
    /// </summary>
    /// <remarks>Performs no additional initialization.</remarks>
    private Order()
    {
    }

    /// <summary>
    /// Gets or sets Order Id.
    /// </summary>
    public int OrderId { get; private set; }

    /// <summary>
    /// Gets or sets Ordered On.
    /// </summary>
    [Column(TypeName = "datetime")]
    public DateTime OrderedOn { get; private set; }

    /// <summary>
    /// Gets or sets Customer Id.
    /// </summary>
    public int? CustomerId { get; private set; } // nullable FK to Customer , null when IsCashSale is true then CustomerId is not applicable.

    /// <summary>
    /// Gets or sets a value indicating whether the order is a cash sale.
    /// If true, CustomerId is not applicable because the customer is either one time or makeing a cash sale.
    /// </summary>
    public bool IsCashSale { get; set; } = false;
    /// <summary>
    /// Gets or sets Customer Name.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    [StringLength(100)]
    public string CustomerName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the Completed user event, which indicates when the order was completed and by whom.
    /// </summary>
    public UserEvent Completed { get; private set; } = UserEvent.None();

    /// <summary>
    /// Gets the Cancelled user event, which indicates when the order was cancelled, by whom, and the reason for cancellation.
    /// </summary>
    public ReasonedUserEvent Cancelled { get; private set; } = ReasonedUserEvent.None();

    /// <summary>
    /// Gets or sets the Order Details.
    /// </summary>
    public List<OrderDetail> OrderDetails { get; private set; } = new List<OrderDetail>();

    /// <summary>
    /// Will add a product to the order details.
    /// </summary>
    /// <param name="productId">The unique identifier for the product.</param>
    /// <param name="quantity">The quantity of the product.</param>
    /// <param name="price">The price of the product.</param>
    /// <returns>The created order detail.</returns>
    public OrderDetail AddDetail(int productId, int quantity, decimal price)
    {
      var orderDetail = new OrderDetail(productId, quantity, price);
      this.OrderDetails.Add(orderDetail);
      return orderDetail;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Order"/> class.
    /// </summary>
    /// <param name="customerName">The name of the customer.</param>
    public Order(string customerName)
    {
      this.CustomerName = customerName;
      this.OrderedOn = DateTime.UtcNow;
      this.TrackingState = TrackableEntities.Common.Core.TrackingState.Added; // Set the tracking state to Added for new entities
    }

    /// <summary>
    /// Will complete the order.
    /// </summary>
    /// <param name="userId">The user who completed the order.</param>
    public void Complete(int userId)
    {
      this.AssertNotCompletedOrCancelled();
      this.Completed = new UserEvent(userId);
    }

    /// <summary>
    /// Will cancel the order.
    /// </summary>
    /// <param name="userId">The user who cancelled the order.</param>
    /// <param name="reason">The reason for cancellation.</param>
    /// <exception cref="InvalidDomainOperationException">Thrown when the cancellation reason is not provided.</exception>
    public void Cancel(int userId, string reason)
    {
      this.AssertNotCompletedOrCancelled();
      if (string.IsNullOrWhiteSpace(reason))
      {
        throw new InvalidDomainOperationException($"A reason is required when cancelling an order.");
      }
      this.Cancelled = new ReasonedUserEvent(userId, reason);
    }

    private void AssertNotCompletedOrCancelled()
    {
      if (this.Completed.IsCompleted)
      {
        throw new InvalidDomainOperationException($"{this} has already been completed.");
      }

      if (this.Cancelled.IsCompleted)
      {
        throw new InvalidDomainOperationException($"{this} has already been cancelled.");
      }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      return $"Order No: {this.OrderId} for {this.CustomerName} on {this.OrderedOn:dd-MMM-yy HH:mm}";
    }
  }
}