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
    public Task GenerateSeedDataAsync()
    {
      return this.GenerateProductSeedDataAsync();
    }

    private async Task GenerateProductSeedDataAsync()
    {
      if ((this.environment == null || this.environment.IsDevelopment()) && !await this.context.Products.AnyAsync().ConfigureAwait(false))
      {
        var products = new List<Product>()
        {
          new Product() { ProductName = "Coke", Price = 10 },
          new Product() { ProductName = "Bar One", Price = 9 },
          new Product() { ProductName = "Smarties", Price = 8.5M },
          new Product() { ProductName = "Popcorn", Price = 2.5M },
          new Product() { ProductName = "Peanuts", Price = 5 },
          new Product() { ProductName = "Cappuccino", Price = 10 },
          new Product() { ProductName = "Tomato Chips", Price = 6 },
        };

        this.context.Products.AddRange(products);

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