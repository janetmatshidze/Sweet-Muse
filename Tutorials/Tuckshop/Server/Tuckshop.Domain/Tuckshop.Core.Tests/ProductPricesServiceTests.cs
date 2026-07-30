namespace Tuckshop.Core.Tests
{
  using System.Linq;
  using System.Threading.Tasks;
  using Microsoft.EntityFrameworkCore;
  using Tuckshop.Core.App.Services;
  using Xunit;

  /// <summary>
  ///  Class for testing the ProductPricesService.
  /// </summary>
  public class ProductPricesServiceTests
  {

    [Fact]
    public async Task GetProductPricesAsync()
    {
      var unitTestHelper = await UnitTestHelper.InitWithContextAsync(generateSeedData: true).ConfigureAwait(false);
      var context = unitTestHelper.DbContext;
      var priceService = new ProductPricesService(context);

      var products = await context.Products.ToListAsync().ConfigureAwait(false);
      var prices = await priceService.GetProductPricesAsync(products.Select(products => products.ProductId).ToHashSet()).ConfigureAwait(false);

      var testPrices = products.ToDictionary(p => p.ProductId, p => p.Price);

      Assert.Equal(testPrices, prices);
    }
  }
}
