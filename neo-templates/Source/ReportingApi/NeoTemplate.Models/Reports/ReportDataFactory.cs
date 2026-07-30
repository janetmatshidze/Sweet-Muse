namespace NeoTemplate.Models.Reports
{
  using System;
  using System.Collections.Generic;
  using System.Globalization;
  using System.Linq;
  using NeoTemplate.Models.Reports.Example;
  using NeoTemplate.Models.Reports.ExampleDeferred;
  using NeoTemplate.Models.Reports.ExampleWithCriteria;

  /// <summary>
  /// The report data factory
  /// </summary>
  public static class ReportDataFactory
  {
    /// <summary>
    /// Method to get the example report model
    /// </summary>
    /// <returns>the example report model</returns>
    public static ExampleReportModel GetExampleReportModel()
    {
      List<ExampleReportModel.ExampleReportData> data = PopulateData<ExampleReportModel.ExampleReportData>();
      return new ExampleReportModel() { Data = data };
    }

    /// <summary>
    /// Method to get the deferred example report model
    /// </summary>
    /// <param name="criteria">The criteria to be used when populating the model.</param>
    /// <returns>The example report model</returns>
    public static ExampleDeferredReportModel GetExampleDeferredReportModel(ExampleDeferredReportCriteria? criteria)
    {
      _ = criteria ?? throw new ArgumentNullException(nameof(criteria));

      List<ExampleReportModel.ExampleReportData> data = PopulateData<ExampleReportModel.ExampleReportData>();
      return new ExampleDeferredReportModel() { Data = data.Where(criteria.FilterPredicate()).ToList() };
    }

    /// <summary>
    /// Method to get the example report model with criteria
    /// </summary>
    /// <param name="criteria">The criteria to be used when populating the model.</param>
    /// <returns>The example report model</returns>
    public static ExampleWithCriteriaReportModel GetExampleWithCriteriaReportModel(ExampleWithCriteriaReportCriteria? criteria)
    {
      _ = criteria ?? throw new ArgumentNullException(nameof(criteria));

      List<ExampleReportModel.ExampleReportData> data = PopulateData<ExampleReportModel.ExampleReportData>();
      return new ExampleWithCriteriaReportModel() { Data = data.Where(criteria.FilterPredicate()).ToList() };
    }

    private static List<TRecord> PopulateData<TRecord>()
      where TRecord : ExampleReportModel.ExampleReportData, new()
    {
      var data = new List<TRecord>();

      // generate some data
      for (int i = 1; i <= 1000; i++)
      {
        var record = new TRecord();
        SetupDataRecord(record, i);
        data.Add(record);
      }

      return data;
    }

    private static void SetupDataRecord(ExampleReportModel.ExampleReportData record, int index)
    {
      record.ExampleReportDataId = index;
      record.Name = $"Example Data {index}";
      record.CreatedOn = DateTime.UtcNow.AddDays(-index).AddHours(-1);
      record.CreatedMonth = record.CreatedOn.ToString("MMMM", CultureInfo.InvariantCulture);
    }
  }
}
