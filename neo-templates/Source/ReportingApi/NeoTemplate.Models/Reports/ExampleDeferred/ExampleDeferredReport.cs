namespace NeoTemplate.Models.Reports.ExampleDeferred
{
  using System;
  using Neo.Reporting;
  using NeoTemplate.Security;

  /// <summary>
  /// The example deferred report.
  /// </summary>
  public class ExampleDeferredReport : ReportBase<ExampleDeferredReportBuilder, ExampleDeferredReportCriteria, ExampleDeferredReportModel>
  {
    /// <inheritdoc/>
    public override Enum RequireRole => Roles.ExampleReport.View;

    /// <inheritdoc/>
    public override bool EnablePdfDownload => true;

    /// <inheritdoc/>
    public override Enum RequireRoleForPdfDownload => Roles.ExampleReport.Download;

    /// <inheritdoc/>
    public override Enum RequireRoleForExcelDownload => Roles.ExampleReport.Download;

    /// <inheritdoc/>
    public override bool IsDeferred => true;

    /// <inheritdoc/>
    public override string GetReportViewName(object? criteria) => "/Areas/Reports/Views/ExampleDeferred.cshtml";
  }
}