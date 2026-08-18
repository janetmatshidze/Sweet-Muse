namespace Tuckshop.Core.Models
{
  using Neo.Model;
  using Neo.Model.Exceptions;
  using Neo.Model.Validation;
  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;

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
    [StringLength(100)]
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Product description.
    /// </summary>
    [Required]
    [StringLength(250)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets stock which is the available quantity of the product.
    /// </summary>
    public int Stock { get; set; }

    public int CategoryId { get; set; }

    /// <summary>
    /// Gets or sets the Image url.
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets Price.
    /// </summary>
    [Column(TypeName = "money")]
    public decimal Price { get; set; }

    /// <summary>
    /// Reduces stock by the given quantity. Will not allow stock to go negetive.
    /// </summary>
    /// <param name="quantity">The quantity to reduce the stock by.</param>
    /// <exception cref="InvalidDomainOperationException">Thrown when the quantity is invalid or there is insufficient stock.</exception>
    public void ReduceStock(int quantity)
    {
      if(quantity <= 0)
      {
        throw new InvalidDomainOperationException("Quantity must be greater than zero.");
      }

      if (this.Stock - quantity < 0)
      {
        throw new InvalidDomainOperationException($"Insufficient stock for product {this.ProductName}. Available stock: {this.Stock}.");
      }
      this.Stock -= quantity;
    }

    /// <inheritdoc/>
    protected override void AddBusinessRules(ValidationRules<Product> rules)
    {
      base.AddBusinessRules(rules);

      rules.FailWhen(p => p.Price <= 0, "Price must be greater than zero.");
    }
  }
}

