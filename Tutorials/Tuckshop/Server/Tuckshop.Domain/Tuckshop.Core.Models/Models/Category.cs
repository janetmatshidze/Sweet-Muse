namespace Tuckshop.Core.Models
{
  using System.ComponentModel.DataAnnotations;
  using Neo.Model;

  /// <summary>
  /// Category class to represent a category in the system.
  /// </summary>
  public class Category : ModelBase<Category>
  {
    /// <summary>
    /// Gets or sets the Category Id.
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// Gets or sets the category name.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    [StringLength(100)]
    public string CategoryName { get; set; } = string.Empty;

  }
}
