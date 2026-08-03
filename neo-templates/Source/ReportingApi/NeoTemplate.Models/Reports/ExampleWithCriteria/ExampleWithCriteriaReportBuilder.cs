namespace NeoTemplate.Models.Reports.ExampleWithCriteria
{
  using System.Threading.Tasks;
  using Neo.Reporting;

  /// <summary>
  /// Will build an ExampleReport model, using the criteria and any required Scoped Services.
  /// </summary>
  public class ExampleWithCriteriaReportBuilder : IReportModelBuilder<ExampleWithCriteriaReportCriteria, ExampleWithCriteriaReportModel>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="ExampleWithCriteriaReportBuilder"/> class.
    /// </summary>
    public ExampleWithCriteriaReportBuilder()
    {
      // Bring in and initialize any dependencies here
    }

    /// <inheritdoc/>
    public async Task<ExampleWithCriteriaReportModel> BuildModelAsync(ExampleWithCriteriaReportCriteria? criteria, ReportOptions reportOptions)
    {
      // simulate a delay
      await Task.Delay(2000);

      return ReportDataFactory.GetExampleWithCriteriaReportModel(criteria);
    }
  }
}