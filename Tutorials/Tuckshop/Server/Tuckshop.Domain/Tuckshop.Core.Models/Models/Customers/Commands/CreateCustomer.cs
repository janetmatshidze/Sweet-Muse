namespace Tuckshop.Core.Models.Customers.Commands
{
  using Neo.Model;
  using Neo.Model.Validation;
  using System.ComponentModel.DataAnnotations;

  /// <summary>
  /// Class representing a request to create a customer.
  /// </summary>
  public class CreateCustomer : ModelBase<CreateCustomer>
  {
    /// <summary>
    /// Gets and sets the customer's first name.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets and sets the customer's last name.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets and sets the customer's email address.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets and sets the customer's phone number.
    /// </summary>
    ///  [Required(AllowEmptyStrings = false)]
    [StringLength(100)]
    public string PhoneNumber { get; set; } = string.Empty;

    /// <inheritdoc />
    protected override void AddBusinessRules(ValidationRules<CreateCustomer> rules)
    {
      rules.FailWhen(
        c => !System.Text.RegularExpressions.Regex.IsMatch(c.PhoneNumber ?? "", @"^\d{10}$"),
        "Phone number must be exactly 10 digits.");

      rules.FailWhen(
   c => System.Text.RegularExpressions.Regex.IsMatch(c.PhoneNumber ?? "", @"^\d{10}$")
        && System.Text.RegularExpressions.Regex.IsMatch(c.PhoneNumber ?? "", @"^(\d)\1{9}$"),
   "Please enter a valid phone number.");

      rules.FailWhen(
        c => !System.Text.RegularExpressions.Regex.IsMatch(c.Email ?? "", @"^[^\s@]+@[^\s@]+\.[^\s@]+$"),
        "Please enter a valid email address");
    }
  }
}
