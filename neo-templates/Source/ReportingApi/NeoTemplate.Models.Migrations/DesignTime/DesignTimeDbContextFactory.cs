namespace NeoTemplate.Models.Migrations.DesignTime
{
  using System.IO;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.EntityFrameworkCore.Design;
  using Microsoft.Extensions.Configuration;
  using Neo.Model.MultiTenancy;

  /// <summary>
  /// Class to construct the DbContext at design time
  /// </summary>
  public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ReportingDbContext>
  {
    /// <summary>
    /// Will return an DbContext using the connection string from appsettings.json
    /// </summary>
    /// <param name="args">The design time args</param>
    /// <returns>A DbContext</returns>
    public ReportingDbContext CreateDbContext(string[] args)
    {
      IConfigurationRoot configuration = new ConfigurationBuilder()
       .SetBasePath(Directory.GetCurrentDirectory())
       .AddJsonFile("appsettings.json")
       .Build();
      var builder = new DbContextOptionsBuilder<ReportingDbContext>();
      var connectionString = configuration.GetConnectionString("Main");

      builder.UseSqlServer(connectionString, builder => builder.MigrationsAssembly(typeof(DesignTimeDbContextFactory).Assembly.GetName().Name));
      return new ReportingDbContext(
        builder.Options,
        new Neo.Model.Processing.DbContextProcessingOptions<ReportingDbContext>(),
        new CustomTenantService());
    }
  }
}
