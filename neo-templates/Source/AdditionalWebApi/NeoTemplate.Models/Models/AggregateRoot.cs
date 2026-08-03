namespace NeoTemplate.Models
{
  using System;
  using System.ComponentModel.DataAnnotations;
  using System.ComponentModel.DataAnnotations.Schema;
  using Neo.Model;
  using Neo.Model.AuditTrail;
  using Neo.Model.ValueObjects;
  using Newtonsoft.Json;

  /// <summary>
  /// Remove this entity! It is here only as an example
  /// </summary>
  public class AggregateRoot : ModelBase<AggregateRoot>, IAuditTrailValueObjectEntity
  {
    /// <summary>
    /// Gets or sets the primary key
    /// </summary>
    public int AggregateRootId { get; set; }

    /// <summary>
    /// Gets or sets the Example Entity Name
    /// </summary>
    [Required]
    [StringLength(100)]
    public string AggregateRootName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Example Date
    /// </summary>
    [Required]
    [Column(TypeName = "date")]
    public DateTime? ExampleDate { get; set; }

    /// <inheritdoc/>
    [JsonIgnore]
    public AuditValues? Audit { get; set; }
  }
}
