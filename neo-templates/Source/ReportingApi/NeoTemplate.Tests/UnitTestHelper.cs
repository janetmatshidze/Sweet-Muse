namespace NeoTemplate.Tests
{
  using System;
  using System.Collections.Generic;
  using System.Threading;
  using System.Threading.Tasks;
  using Microsoft.EntityFrameworkCore;
  using Neo.Model.MultiTenancy;
  using Neo.Model.Processing;
  using Neo.Model.Validation;
  using Neo.Testing;
  using Neo.Testing.Identity;
  using NeoTemplate.Models.Identity;
  using NeoTemplate.Models.Initializers;

  public static class UnitTestHelper
  {
    private static readonly TestUserResolver<User> userResolver = new TestUserResolver<User>(1);

    /// <summary>
    /// Gets the model validator.
    /// </summary>
    public static ModelValidator ModelValidator { get; } = new ModelValidator(new Neo.Model.Metadata.MetadataService());

    /// <summary>
    /// Initialise the database context.
    /// </summary>
    /// <returns>The database context.</returns>
    public static async Task<ReportingDbContext> InitContextAsync()
    {
      DbContextOptionsBuilder<ReportingDbContext> builder =
        new DbContextOptionsBuilder<ReportingDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString());

      var processingOptions = new DbContextProcessingOptions<ReportingDbContext>(
      new List<IDbContextProcessor>() { new Neo.Model.AuditTrail.AuditTrailProcessor<User>(userResolver) });

      var context = new ReportingDbContext(builder.Options, processingOptions, new CustomTenantService());

      await PopulateDbContextAsync(context);

      return context;
    }

    private static Task PopulateDbContextAsync(ReportingDbContext context)
    {
      var systemUserService = new SystemUserServiceMock<User>(userResolver);
      return new SeedDataAsyncInitializer(context, systemUserService, null).InitializeAsync(CancellationToken.None);
    }
  }
}
