namespace NeoTemplate.Models.Reports.ExampleDeferred
{
  using System.Threading.Tasks;
  using Neo.Reporting;

  /// <summary>
  /// Will build an ExampleReport model, using the criteria and any required Scoped Services.
  /// </summary>
  public class ExampleDeferredReportBuilder : IReportModelBuilder<ExampleDeferredReportCriteria, ExampleDeferredReportModel>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="ExampleDeferredReportBuilder"/> class.
    /// </summary>
    public ExampleDeferredReportBuilder()
    {
      // Bring in and initialize any dependencies here
    }

    /// <inheritdoc/>
    public async Task<ExampleDeferredReportModel> BuildModelAsync(ExampleDeferredReportCriteria? criteria, ReportOptions reportOptions)
    {
      // simulate a delay
      await Task.Delay(2000);

      return ReportDataFactory.GetExampleDeferredReportModel(criteria);
    }
  }
}