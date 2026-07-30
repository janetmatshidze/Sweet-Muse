namespace NeoTemplate.Models
{
  using Microsoft.EntityFrameworkCore;
  using Neo.Model.Identity;
  using Neo.Model.Processing;
  using Neo.Model.SqlServer;
  using NeoTemplate.Models.Identity;

  /// <summary>
  /// Example DbContext that should be renamed or removed
  /// Rename if you are going to use Code-First and remove if you are using Db-First
  /// </summary>
  public class ModelDbContext : SqlServerDBContextBase<ModelDbContext>, IUsersDbContext<User>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="ModelDbContext"/> class.
    /// </summary>
    /// <param name="options">DbContext Options</param>
    /// <param name="processingOptions">Processing Options</param>
    public ModelDbContext(
      DbContextOptions options,
      DbContextProcessingOptions<ModelDbContext> processingOptions)
      : base(options, processingOptions)
    {
    }

    /// <inheritdoc/>
    public DbSet<User> Users { get; set; }

    /// <summary>
    /// Gets or sets the AggregateRoot Entities
    /// </summary>
    public DbSet<AggregateRoot> AggregateRoots { get; set; }
  }
}
