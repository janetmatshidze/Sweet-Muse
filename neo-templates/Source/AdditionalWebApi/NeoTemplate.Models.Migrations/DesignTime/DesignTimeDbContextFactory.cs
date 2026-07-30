namespace NeoTemplate.Models.Migrations.DesignTime
{
  using System.IO;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.EntityFrameworkCore.Design;
  using Microsoft.Extensions.Configuration;
  using NeoTemplate.Models;

  /// <summary>
  /// Class to construct the DbContext at design time
  /// </summary>
  public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AggregateRootDbContext>
  {
    /// <summary>
    /// Will return an DbContext using the connection string from appsettings.json
    /// </summary>
    /// <param name="args">The design time args</param>
    /// <returns>A DbContext</returns>
    public AggregateRootDbContext CreateDbContext(string[] args)
    {
      var configurationBuilder = new ConfigurationBuilder()
       .SetBasePath(Directory.GetCurrentDirectory())
       .AddJsonFile("appsettings.json");

      var configuration = configurationBuilder.Build();
      var builder = new DbContextOptionsBuilder<AggregateRootDbContext>();
      var connectionString = configuration.GetConnectionString("AggregateRoot");
      builder.UseSqlServer(connectionString, builder => builder.MigrationsAssembly(typeof(DesignTimeDbContextFactory).Assembly.GetName().Name));
      return new AggregateRootDbContext(builder.Options, new Neo.Model.Processing.DbContextProcessingOptions<AggregateRootDbContext>());
    }
  }
}
