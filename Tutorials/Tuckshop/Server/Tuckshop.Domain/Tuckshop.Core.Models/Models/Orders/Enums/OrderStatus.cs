namespace Tuckshop.Core.Models.Orders.Enums
{
  /// <summary>
  /// Order Status query enum.
  /// </summary>
  public enum OrderStatus
  {
    /// <summary>
    /// A non-complete and non-cancelled order.
    /// </summary>
    Pending,

    /// <summary>
    /// A completed order.
    /// </summary>
    Completed,

    /// <summary>
    /// A cancelled order.
    /// </summary>
    Cancelled,
  }
}
