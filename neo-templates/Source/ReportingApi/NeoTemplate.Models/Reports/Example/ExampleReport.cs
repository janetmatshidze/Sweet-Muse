namespace NeoTemplate.Models.Reports.Example
{
  using System;
  using Neo.Reporting;
  using NeoTemplate.Security;

  /// <summary>
  /// The example entity report.
  /// </summary>
  public class ExampleReport : ReportBase<ExampleReportBuilder, EmptyCriteria, ExampleReportModel>
  {
    /// <inheritdoc/>
    public override Enum RequireRole => Roles.ExampleReport.View;

    /// <inheritdoc/>
    public override bool EnablePdfDownload => true;

    /// <inheritdoc/>
    public override Enum RequireRoleForPdfDownload => Roles.ExampleReport.Download;

    /// <inheritdoc/>
    public override Enum RequireRoleForExcelDownload => Roles.ExampleReport.Download;

    /// <summary>
    /// Gets a value indicating whether the report is deferred or not.
    /// </summary>
    public override bool IsDeferred => false;

    /// <summary>
    /// Get the name of the report
    /// </summary>
    /// <param name="criteria">The criteria to be used with the report.</param>
    /// <returns>A string containing the report locations.</returns>
    public override string GetReportViewName(object? criteria) => "/Areas/Reports/Views/Example.cshtml";
  }
}