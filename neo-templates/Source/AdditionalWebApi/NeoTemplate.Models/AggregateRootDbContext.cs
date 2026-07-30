namespace NeoTemplate.Models
{
  using Microsoft.EntityFrameworkCore;
  using Neo.Model.Processing;
  using Neo.Model.SqlServer;

  /// <summary>
  /// Example DbContext that should be renamed or removed
  /// Rename if you are going to use Code-First and remove if you are using Db-First
  /// </summary>
  public class AggregateRootDbContext : SqlServerDBContextBase<AggregateRootDbContext>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateRootDbContext"/> class.
    /// </summary>
    /// <param name="options">DbContext Options</param>
    /// <param name="processingOptions">Processing Options</param>
    public AggregateRootDbContext(
      DbContextOptions<AggregateRootDbContext> options,
      DbContextProcessingOptions<AggregateRootDbContext> processingOptions)
      : base(options, processingOptions)
    {
    }

    /// <summary>
    /// Gets or sets the AggregateRoot Entities
    /// </summary>
    public DbSet<AggregateRoot> AggregateRoots { get; set; }
  }
}
