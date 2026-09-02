#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace Tuckshop.Core.Models
{
  using Microsoft.EntityFrameworkCore;
  using Neo.Extensions;
  using Neo.Model.Identity;
  using Neo.Model.Processing;
  using Neo.Model.SqlServer;
  using Neo.OneTimeTokens;
  using Tuckshop.Core.Models.Customers;
  using Tuckshop.Core.Models.Files;
  using Tuckshop.Core.Models.Identity;
  using Tuckshop.Core.Models.Orders;
  using Tuckshop.Core.Models.Wallets;

  /// <summary>
  /// Main Tuckshop DbContext.
  /// </summary>
  /// <remarks>
  /// Initializes a new instance of the <see cref="ModelDbContext"/> class.
  /// </remarks>
  /// <param name="options">DbContext Options.</param>
  /// <param name="processingOptions">Processing Options.</param>
  public class ModelDbContext(
    DbContextOptions<ModelDbContext> options,
    DbContextProcessingOptions<ModelDbContext> processingOptions) : SqlServerDBContextBase<ModelDbContext>(options, processingOptions), IUsersDbContext<User>, IOneTimeTokenDbContext<OneTimeToken>
  {
    /// <summary>
    /// Gets or sets the file descriptors.
    /// </summary>
    public DbSet<FileDescriptor> FileDescriptors { get; set; }

    /// <inheritdoc/>
    public DbSet<OneTimeToken> OneTimeTokens { get; set; }

    /// <inheritdoc/>
    public DbSet<User> Users { get; set; }

    /// <summary>
    ///  Gets or sets the orders.
    /// </summary>
    public DbSet<Order> Orders { get; set; }

    public DbSet<Product> Products { get; set; }

    public DbSet<Customer> Customers { get; set; }

    public DbSet<Category> Categories { get; set; }

    public DbSet<WalletTransaction> WalletTransactions { get; set; }

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);

      modelBuilder.AddUtcDateConverters();
      modelBuilder.UseNeoForeignKeyDeleteBehaviour();

      modelBuilder.Entity<User>(builder =>
      {
        builder.HasIndex(u => new { u.IdentityGuid, u.ClientId, }).IsUnique().HasFilter(null);
      });

      // makes the product name unique.
      modelBuilder.Entity<Product>(builder =>
      {
        builder.HasIndex(p => p.ProductName).IsUnique();
      });
      // makes the customer name unique.
      modelBuilder.Entity<Customer>(builder =>
      {
        builder.HasIndex(c => c.Email).IsUnique();
      });

      modelBuilder.Entity<Customer>(builder =>
      {
        builder.HasMany(c => c.WalletTransactions)
        .WithOne()
        .HasForeignKey(wt => wt.CustomerId)
        .IsRequired();
      });
    }
  }
}