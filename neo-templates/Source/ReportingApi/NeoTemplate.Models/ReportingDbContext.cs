namespace NeoTemplate
{
  using Microsoft.EntityFrameworkCore;
  using Neo.Model.MultiTenancy;
  using Neo.Model.Processing;
  using Neo.Reporting.Models;
  using NeoTemplate.Models.Identity;

  /// <summary>
  /// Reporting Db Context.
  /// </summary>
  public class ReportingDbContext : ReportingDbContextBase<ReportingDbContext, User>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="ReportingDbContext"/> class.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <param name="processingOptions">The processing options.</param>
    /// <param name="tenantService">The tenant service.</param>
    public ReportingDbContext(
      DbContextOptions options,
      DbContextProcessingOptions<ReportingDbContext> processingOptions,
      ITenantService tenantService)
      : base(options, processingOptions, tenantService)
    {
    }
  }
}
