namespace Tuckshop.Core.Models
{
  using System.ComponentModel.DataAnnotations;
  using Neo.Model;

  /// <summary>
  /// Customer class to represent a customer in the system.
  /// </summary>
  public class Customer : ModelBase<Customer>
  {
    /// <summary>
    /// Gets or sets the Customer Id.
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the Customer Name.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Customer Last Name.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets Email.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets Phone Number.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    [StringLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

  }
}
