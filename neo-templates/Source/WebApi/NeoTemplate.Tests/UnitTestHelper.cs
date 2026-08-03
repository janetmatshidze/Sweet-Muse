namespace NeoTemplate.Tests
{
  using System;
  using System.Collections.Generic;
  using System.Threading;
  using System.Threading.Tasks;
  using Microsoft.EntityFrameworkCore;
  using Neo.Model.Processing;
  using Neo.Model.Validation;
  using Neo.Testing;
  using Neo.Testing.Identity;
  using NeoTemplate.Models;
  using NeoTemplate.Models.Identity;
  using NeoTemplate.Models.Initializers;

  public static class UnitTestHelper
  {
    /// <summary>
    /// Gets the model validator.
    /// </summary>
    public static ModelValidator ModelValidator { get; } = new ModelValidator(new Neo.Model.Metadata.MetadataService());

    /// <summary>
    /// Gets the user resolver.
    /// </summary>
    public static TestUserResolver<User> UserResolver { get; } = new TestUserResolver<User>(1);

    /// <summary>
    /// Initialise the database context.
    /// </summary>
    /// <returns>The database context.</returns>
    public static async Task<ModelDbContext> InitContextAsync()
    {
      DbContextOptionsBuilder<ModelDbContext> builder =
        new DbContextOptionsBuilder<ModelDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString());

      var processingOptions = new DbContextProcessingOptions<ModelDbContext>(
      new List<IDbContextProcessor>() { new Neo.Model.AuditTrail.AuditTrailProcessor<User>(UserResolver) });

      ModelDbContext context = new ModelDbContext(builder.Options, processingOptions);

      await PopulateDbContextAsync(context);

      return context;
    }

    private static async Task PopulateDbContextAsync(ModelDbContext context)
    {
      var systemUserService = new SystemUserServiceMock<User>(UserResolver);
      await new SeedDataAsyncInitializer(context, systemUserService, null).InitializeAsync(CancellationToken.None);

      context.DetachEntities(context.AggregateRoots);
    }
  }
}
