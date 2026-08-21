namespace Tuckshop.Core.Models.Customers.Commands
{
  using Neo.Model;

  /// <summary>
  /// Class representing a request to delete a customer.
  /// </summary>
  public class DeleteCustomer : ModelBase<DeleteCustomer>
  {
    /// <summary>
    /// Gets and sets the CustomerId.
    /// </summary>
    public int CustomerId { get; set; }
  }
}
