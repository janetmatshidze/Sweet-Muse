namespace Tuckshop.Core.Models
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;
  using System.Text;
  using Neo.Model;
  using Neo.Model.Validation;

  /// <summary>
  /// Product class to represent a product in the system.
  /// </summary>
  public class Product : ModelBase<Product>
  {
    /// <summary>
    /// Gets or sets a Product Id.
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Gets or sets Product Name.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets Price.
    /// </summary>
    [Column(TypeName = "money")]
    public decimal Price { get; set; }

    /// <inheritdoc/>
    protected override void AddBusinessRules(ValidationRules<Product> rules)
    {
      base.AddBusinessRules(rules);

      rules.FailWhen(c => c.Price <= 0, "Price must be greater than zero.");
    }
  }
}

