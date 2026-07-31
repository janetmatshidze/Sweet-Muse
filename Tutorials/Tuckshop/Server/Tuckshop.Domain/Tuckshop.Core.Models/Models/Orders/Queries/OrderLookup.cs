#pragma warning disable
namespace Tuckshop.Core.Models.Orders.Queries
{
  using System;

  public class OrderLookup
  {
    public int OrderId { get; set; }
    public string CustomerName { get; set; }
    public DateTime OrderedOn { get; set; }
    public DateTime? CompletedOn { get; set; }
    public DateTime? CancelledOn { get; set; }
    public string CancelledReason { get; set; }
    public string CompletedBy { get; set; }
    public string CancelledBy { get; set; }
    public decimal OrderTotalExcl { get; set; }
    public decimal OrderTotal { get; set; }
    public object Items { get; set; }
  }
}
