namespace Tuckshop.Core.Models.Orders.Commands
{
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations;
  using Neo.Model;
  using Neo.Model.Validation;

  /// <summary>
  /// Class representing a new order command.
  /// </summary>
  public class CreateOrder : ModelBase<CreateOrder>
  {

    /// <summary>
    /// Gets or sets the Customer Id. Null when IsCashSale is true.
    /// </summary>
    public int? CustomerId { get; set; }

    /// <summary>
    /// Gets and sets a value indicating whether this is a cash sale (one-ime / walk-in customer).
    /// </summary>
    public bool IsCashSale { get; set; }

    /// <summary>
    /// Gets and sets the Customer Name.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// Gets and sets the Order Details.
    /// </summary>
    public ICollection<NewOrderDetail> OrderDetails { get; set; } = new List<NewOrderDetail>();

    /// <summary>
    /// Represents a new order detail, which includes the product ID  and the Quantityfor the order.
    /// </summary>
    public class NewOrderDetail : ModelBase<NewOrderDetail>
    {
      /// <summary>
      /// Gets or sets the Order Detail Id.
      /// </summary>
      public int ProductId { get; set; }

      /// <summary>
      /// Gets or sets the Quantity.
      /// </summary>
      public int Quantity { get; set; }
    }

    /// <inheritdoc/>
    protected override void AddBusinessRules(ValidationRules<CreateOrder> rules)
    {
      rules.FailWhen(
      ord => ord.OrderDetails == null || ord.OrderDetails.Count == 0,
      "Order details are required");

      rules.FailWhen(
        ord => !ord.IsCashSale && ord.CustomerId == null,
        "A customer must be selected unless this is a cash sale.");
    }
  }
}
