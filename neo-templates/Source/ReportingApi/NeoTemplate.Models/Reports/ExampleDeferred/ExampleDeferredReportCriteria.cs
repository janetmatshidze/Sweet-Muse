namespace NeoTemplate.Models.Reports.ExampleDeferred
{
  using System;
  using static NeoTemplate.Models.Reports.Example.ExampleReportModel;

  /// <summary>
  /// The example deferred report criteria
  /// </summary>
  public class ExampleDeferredReportCriteria
  {
    /// <summary>
    /// Gets or sets the search string value
    /// </summary>
    public string? SearchString { get; set; }

    /// <summary>
    /// Gets or sets Filter Predicate
    /// </summary>
    /// <returns>Returns a boolean function value</returns>
    public Func<ExampleReportData, bool> FilterPredicate() =>
      (data) => this.SearchString == null
        || (data.Name != null && data.Name.Contains(this.SearchString, StringComparison.InvariantCulture))
        || (data.CreatedMonth != null && data.CreatedMonth.Contains(this.SearchString, StringComparison.InvariantCulture));
  }
}
