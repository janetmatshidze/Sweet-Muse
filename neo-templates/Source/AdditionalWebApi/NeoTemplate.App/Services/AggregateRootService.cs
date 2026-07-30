namespace NeoTemplate.App.Services
{
  using Neo.Model.Services;
  using NeoTemplate.Models;

  /// <summary>
  /// AggregateRoot Service (should be removed or renamed)
  /// </summary>
  public class AggregateRootService : UpdateableModelService<AggregateRoot, AggregateRootDbContext, int>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateRootService"/> class.
    /// </summary>
    /// <param name="context">The DbContext</param>
    public AggregateRootService(AggregateRootDbContext context)
      : base(context)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateRootService"/> class.
    /// </summary>
    /// <param name="context">The DbContext</param>
    /// <param name="options">The ModelServiceOptions</param>
    public AggregateRootService(AggregateRootDbContext context, ModelServiceOptions<AggregateRoot> options)
      : base(context, options)
    {
    }
  }
}
