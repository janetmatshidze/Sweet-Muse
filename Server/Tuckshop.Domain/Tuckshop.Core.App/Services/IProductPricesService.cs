namespace Tuckshop.Core.App.Services
{
  using System.Collections.Generic;
  using System.Threading.Tasks;

  /// <summary>
  /// An interface for retrieving products price.
  /// </summary>
  public interface IProductPricesService
  {
    /// <summary>
    /// Will get the price for the provided list of Product Ids.
    /// </summary>
    /// <param name="productIds">The list of product IDs for which to retrieve prices.</param>
    /// <returns>A dictionary mapping product IDs to their prices.</returns>
    Task<Dictionary<int, decimal>> GetProductPricesAsync(ICollection<int> productIds); // The int represents the ProductId and the decimal represents the Price.
  }
}
