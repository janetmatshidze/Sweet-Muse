namespace Tuckshop.Core.Models.Orders
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;
  using Neo.Model;

  /// <summary>
  /// Order class to represent an order in the system.
  /// </summary>
  public class Order : ModelBase<Order>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="Order"/> class.
    /// </summary>
    /// <remarks>Performs no additional initialization.</remarks>
    public Order()
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
    /// Gets or sets Customer Name.
    /// </summary>

    [Required(AllowEmptyStrings = false)]
    [MaxLength(100)]
    public string CustomerName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets or sets Completed On.
    /// </summary>
    [Column(TypeName = "datetime")]
    public DateTime? CompletedOn { get; private set; }

    /// <summary>
    /// Gets or sets the Order Details.
    /// </summary>
    public ICollection<OrderDetail> OrderDetails { get; private set; } = new List<OrderDetail>();

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
      this.OrderedOn = DateTime.Now;
      this.TrackingState = TrackableEntities.Common.Core.TrackingState.Added; // Set the tracking state to Added for new entities
    }
  }
}