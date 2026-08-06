namespace Tuckshop.Core.Models.Initializers
{
  using System.Collections.Generic;
  using System.Threading;
  using System.Threading.Tasks;
  using Extensions.Hosting.AsyncInitialization;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Hosting;
  using Neo.Model.Identity.SystemUser;
  using Neo.NotificationServer.Services;
  using Tuckshop.Core.Models.Identity;
  using Tuckshop.Core.Models.Orders;

  /// <summary>
  /// Seed data generation service.
  /// </summary>
  /// <remarks>
  /// Initializes a new instance of the <see cref="SeedDataAsyncInitializer"/> class.
  /// </remarks>
  /// <param name="context">The model database context.</param>
  /// <param name="systemUserService">The system user service.</param>
  /// <param name="environment">The host environment.</param>
  /// <param name="templateTypesService">The template types service.</param>
  public class SeedDataAsyncInitializer(
    ModelDbContext context,
    ISystemUserService<User> systemUserService,
    IHostEnvironment? environment,
    ITemplateTypesService? templateTypesService) : IAsyncInitializer
  {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1823:Avoid unused private fields", Justification = "Future use")]
    private readonly ModelDbContext context = context;
    private readonly ISystemUserService<User> systemUserService = systemUserService;
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1823:Avoid unused private fields", Justification = "Future use")]
    private readonly IHostEnvironment? environment = environment;
    private readonly ITemplateTypesService? templateTypesService = templateTypesService;

    /// <inheritdoc/>
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
      await this.systemUserService.RunWithSystemUserAsync(this.GenerateSeedDataAsync);

      await this.RegisterTemplateTypesAsync();
    }

    /// <summary> 
    /// Will generate the appropriate seed data for the given environment.
    /// </summary>
    /// <returns>A task awaiting the seed data generation.</returns>
    public async Task GenerateSeedDataAsync()
    {
      await this.GenerateProductSeedDataAsync();
      await this.GenerateOrderSeedDataAsync();
      await this.GenerateCategorySeedDataAsync();
    }


    private async Task GenerateProductSeedDataAsync()
    {
      if ((this.environment == null || this.environment.IsDevelopment()) && !await this.context.Products.AnyAsync().ConfigureAwait(false))
      {
        var products = new List<Product>()
        {
          new Product() { ProductName = "Coke", Price = 10, CategoryId = 1, Description = "A refreshing cola drink", ImageUrl = "https://ik.imagekit.io/klp8c8odo/Morning%20Theory/Coke.jpg", Stock = 100 },
          new Product() { ProductName = "Bar One", Price = 9, CategoryId = 2, Description = "A delicious chocolate bar", ImageUrl = "https://ik.imagekit.io/klp8c8odo/Morning%20Theory/Bar%20One.png", Stock = 10 },
          new Product() { ProductName = "Smarties", Price = 8.5M, CategoryId = 2, Description = "Colorful bite-sized candies", ImageUrl = "https://ik.imagekit.io/klp8c8odo/Morning%20Theory/Smarties.png", Stock = 68 },
          new Product() { ProductName = "Popcorn", Price = 2.5M, CategoryId = 2, Description = "Light and crispy snack", ImageUrl = "https://ik.imagekit.io/klp8c8odo/Morning%20Theory/Popcorn.png", Stock = 12 },
          new Product() { ProductName = "Peanuts", Price = 5, CategoryId = 2, Description = "Roasted and salted peanuts", ImageUrl = "https://ik.imagekit.io/klp8c8odo/Morning%20Theory/Salted%20Peanuts.png", Stock = 8 },
          new Product() { ProductName = "Cappuccino", Price = 10, CategoryId = 1, Description = "A rich and creamy coffee drink", ImageUrl = "https://ik.imagekit.io/klp8c8odo/Morning%20Theory/Cappuccino.png", Stock = 2 },
          new Product() { ProductName = "Tomato Chips", Price = 6, CategoryId = 2, Description = "Tangy and crispy tomato-flavored chips", ImageUrl = "https://ik.imagekit.io/klp8c8odo/Morning%20Theory/Tomato%20Chips.png", Stock = 30 },
        };

        if (this.environment == null)
        {
          int i = 1;

          // this is test, give these products Ids.
          foreach (var product in products)
          {
            product.ProductId = i++;
          }
        }

        this.context.Products.AddRange(products);

        await this.context.SaveChangesAsync().ConfigureAwait(false);
      }
    }

    private async Task GenerateOrderSeedDataAsync()
    {
      if (this.environment.IsDevelopment() && !await this.context.Orders.AnyAsync().ConfigureAwait(false)) // Here the code checks if the environment Isdevelopment() and the orders table is empty, and if so adds 3 orders (one for each state).

      {
        var pendingOrder = new Order("Pending Order");
        pendingOrder.AddDetail(1, 1, 10);
        pendingOrder.AddDetail(2, 4, 9);

        var completedOrder = new Order("Completed Order");
        completedOrder.AddDetail(3, 2, 8.5m);
        completedOrder.AddDetail(4, 1, 2.5m);
        completedOrder.Complete(1);

        var cancelledOrder = new Order("Cancelled Order");
        cancelledOrder.AddDetail(5, 1, 5);
        cancelledOrder.Cancel(1, "Don't like peanuts");

        this.context.Orders.AddRange(pendingOrder, completedOrder, cancelledOrder);

        await this.context.SaveChangesAsync().ConfigureAwait(false);
      }
    }

    private async Task GenerateCategorySeedDataAsync()
    {
      if ((this.environment == null || this.environment.IsDevelopment()) && !await this.context.Categories.AnyAsync().ConfigureAwait(false))
      {
        var categories = new List<Category>()
        {
          new Category() { CategoryName = "Beverages" },
          new Category() { CategoryName = "Snacks" },
          new Category() { CategoryName = "Desserts" },
        };
        this.context.Categories.AddRange(categories);
        await this.context.SaveChangesAsync().ConfigureAwait(false);
      }
    }

    /// <summary>
    /// Registers template types used by this service.
    /// </summary>
    public Task RegisterTemplateTypesAsync()
    {
      if (this.templateTypesService != null)
      {
        // await this.templateTypesService.RegisterTemplateTypesAsync(typeof(TemplateTypes));
      }

      return Task.CompletedTask;
    }
  }
}