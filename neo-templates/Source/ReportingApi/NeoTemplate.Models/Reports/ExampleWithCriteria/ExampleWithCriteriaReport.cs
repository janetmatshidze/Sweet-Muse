namespace NeoTemplate.Models.Reports.ExampleWithCriteria
{
  using System;
  using Neo.Reporting;
  using NeoTemplate.Security;

  /// <summary>
  /// The example entity report with criteria.
  /// </summary>
  public class ExampleWithCriteriaReport : ReportBase<ExampleWithCriteriaReportBuilder, ExampleWithCriteriaReportCriteria, ExampleWithCriteriaReportModel>
  {
    /// <inheritdoc/>
    public override Enum RequireRole => Roles.ExampleReport.View;

    /// <inheritdoc/>
    public override Enum RequireRoleForExcelDownload => Roles.ExampleReport.Download;
  }
}