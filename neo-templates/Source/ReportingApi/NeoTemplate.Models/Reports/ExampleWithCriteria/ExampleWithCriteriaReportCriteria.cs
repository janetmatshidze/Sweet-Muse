namespace NeoTemplate.Models.Reports.ExampleWithCriteria
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel.DataAnnotations;
  using Neo.Model;
  using Neo.Model.Validation;
  using static NeoTemplate.Models.Reports.Example.ExampleReportModel;

  /// <summary>
  /// The example report with criteria criteria.
  /// </summary>
  public class ExampleWithCriteriaReportCriteria : ValueObject<ExampleWithCriteriaReportCriteria>
  {
    /// <summary>
    /// Gets or sets the search string to filter by.
    /// </summary>
    [Required]
    public string? SearchString { get; set; }

    /// <summary>
    /// Gets the Filter Predicate
    /// </summary>
    /// <returns>Returns a boolean function value</returns>
    public Func<ExampleReportData, bool> FilterPredicate() =>
      (data) => this.SearchString == null
        || (data.Name != null && data.Name.Contains(this.SearchString, StringComparison.InvariantCulture))
        || (data.CreatedMonth != null && data.CreatedMonth.Contains(this.SearchString, StringComparison.InvariantCulture));

    /// <inheritdoc/>
    protected override void AddBusinessRules(ValidationRules<ExampleWithCriteriaReportCriteria> rules)
    {
      base.AddBusinessRules(rules);

      rules.FailWhen(criteria => criteria.SearchString == null || !criteria.SearchString.StartsWith("J", StringComparison.OrdinalIgnoreCase), "Search String must start with a J");
    }

    /// <inheritdoc/>
    protected override IEnumerable<object?> GetAtomicValues()
    {
      yield return this.SearchString;
    }
  }
}
