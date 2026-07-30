namespace NeoTemplate.Models.Reports.Example
{
  using System;
  using System.Collections.Generic;

  /// <summary>
  /// The Participants Statement Report Model
  /// </summary>
  public class ExampleReportModel
  {
    /// <summary>
    /// Gets or sets the data set for the model.
    /// </summary>
    public List<ExampleReportData>? Data { get; set; }

    /// <summary>
    /// The example report data.
    /// </summary>
    public class ExampleReportData
    {
      /// <summary>
      /// Gets or sets the example report data Id
      /// </summary>
      public int ExampleReportDataId { get; set; }

      /// <summary>
      /// Gets or sets the name used for the report.
      /// </summary>
      public string? Name { get; set; }

      /// <summary>
      /// Gets or sets the created on date time of the report.
      /// </summary>
      public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

      /// <summary>
      /// Gets or sets the created month of the report.
      /// </summary>
      public string? CreatedMonth { get; set; }
    }
  }
}