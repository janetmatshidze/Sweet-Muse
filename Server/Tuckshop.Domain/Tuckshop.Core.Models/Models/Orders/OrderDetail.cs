namespace Tuckshop.Core.Models.Orders
{
  using System;
  using System.ComponentModel.DataAnnotations.Schema;
  using Neo.Model;

  /// <summary>
  /// OrderDetail class to represent the details of an order in the system.
  /// </summary>
  [Table("OrderDetails")]
  public class OrderDetail : ModelBase<OrderDetail>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="OrderDetail"/> class.
    /// </summary>
    private OrderDetail()
    {
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="OrderDetail"/> class.
    /// </summary>
    /// <param name="productId">The unique identifier for the product.</param>
    /// <param name="quantity">The quantity of the product.</param>
    /// <param name="price">The price of the product.</param>
    internal OrderDetail(int productId, int quantity, decimal price)
    {
      this.ProductId = productId;
      this.Quantity = quantity;
      this.Value = price * quantity; // Calculate the total value based on price and quantity.
      this.VAT = Math.Round(this.Value - (this.Value / 1.15m), 2, MidpointRounding.AwayFromZero);// Calculate the VAT based on the value, assuming a VAT rate of 15%, two decimal places with rounding away from zero.
      this.TrackingState = TrackableEntities.Common.Core.TrackingState.Added;// Set the tracking state to Added for new entities
    }

    /// <summary>
    ///  Gets or sets OrderDetail Id.
    /// </summary>
    public int OrderDetailId { get; private set; }

    /// <summary>
    /// Gets or sets the unique identifier for the product.
    /// </summary>
    /// <remarks>Typically assigned by the backing store and used as the primary key; expected to be
    /// non-negative.</remarks>
    public int ProductId { get; private set; }

    /// <summary>
    /// Gets or sets the product.
    /// </summary>
    public Product? Product { get; private set; }

    /// <summary>
    /// Gets or sets the quantity of the product.
    /// </summary>
    public int Quantity { get; private set; }

    /// <summary>
    /// Gets or sets the value of the product.
    /// </summary>
    [Column(TypeName = "money")]
    public decimal Value { get; private set; }

    /// <summary>
    /// Gets or sets the VAT for the product.
    /// </summary>
    [Column(TypeName = "money")]
    public decimal VAT { get; private set; }
  }
}
