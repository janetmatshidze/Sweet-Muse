namespace NeoTemplate.Models.Reports.Example
{
  using System.Threading.Tasks;
  using Neo.Reporting;

  /// <summary>
  /// Will build an ExampleReport model, using the criteria and any required Scoped Services.
  /// </summary>
  public class ExampleReportBuilder : IReportModelBuilder<EmptyCriteria, ExampleReportModel>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="ExampleReportBuilder"/> class.
    /// </summary>
    public ExampleReportBuilder()
    {
      // Bring in and initialize any dependencies here
    }

    /// <inheritdoc/>
    public async Task<ExampleReportModel> BuildModelAsync(EmptyCriteria? criteria, ReportOptions reportOptions)
    {
      // simulate a delay
      await Task.Delay(250);

      return ReportDataFactory.GetExampleReportModel();
    }
  }
}