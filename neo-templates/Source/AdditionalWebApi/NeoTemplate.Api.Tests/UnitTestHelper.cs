namespace NeoTemplate.Api.Tests
{
  using System;
  using System.Collections.Generic;
  using System.Threading;
  using System.Threading.Tasks;
  using Microsoft.EntityFrameworkCore;
  using Neo.Model.Metadata;
  using Neo.Model.Processing;
  using Neo.Model.Validation;
  using Neo.Testing;
  using Neo.Testing.Identity;
  using NeoTemplate.Models;
  using NeoTemplate.Models.DummyUser;

  public class UnitTestHelper
  {
    private AggregateRootDbContext? dbContext;

    public UnitTestHelper()
    {
      this.ModelValidator = new ModelValidator(this.MetadataService);
      this.NotificationService = new();
    }

    /// <summary>
    /// Gets the metadata service.
    /// </summary>
    public IMetadataService MetadataService { get; } = new MetadataService();

    /// <summary>
    /// Gets the model validator.
    /// </summary>
    public ModelValidator ModelValidator { get; }

    /// <summary>
    /// Gets the user resolver.
    /// </summary>
    public TestUserResolver<User> UserResolver { get; } = new TestUserResolver<User>(1);

    /// <summary>
    /// Gets the notification service.
    /// </summary>
    public Neo.NotificationServer.Mocks.MockNotificationService NotificationService { get; }

    /// <summary>
    /// Gets the db context. You must call <see cref="InitContextAsync"/> before accessing this property.
    /// </summary>
    public AggregateRootDbContext DbContext
    {
      get
      {
        if (this.dbContext == null)
        {
          throw new InvalidOperationException("You must call InitContextAsync before accessing the DbContext property.");
        }

        return this.dbContext;
      }
    }

    /// <summary>
    /// Create the test helper and initialise the db context.
    /// </summary>
    /// <returns>The unit test helper.</returns>
    public static async Task<UnitTestHelper> InitWithContextAsync()
    {
      var testHelper = new UnitTestHelper();
      await testHelper.InitContextAsync();
      return testHelper;
    }

    /// <summary>
    /// Create the test helper and initialise the db context.
    /// </summary>
    /// <returns>The unit test helper.</returns>
    public static UnitTestHelper InitWithContext()
    {
      var testHelper = new UnitTestHelper();
      testHelper.InitContextAsync().GetAwaiter().GetResult();
      return testHelper;
    }

    /// <summary>
    /// Initialise the database context.
    /// </summary>
    /// <param name="generateSeedData">A value indicating whether to generate seed data.</param>
    /// <returns>The database context.</returns>
    public async Task<AggregateRootDbContext> InitContextAsync(bool generateSeedData = true)
    {
      DbContextOptionsBuilder<AggregateRootDbContext> builder =
        new DbContextOptionsBuilder<AggregateRootDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString());

      var processingOptions = new DbContextProcessingOptions<AggregateRootDbContext>(
       new List<IDbContextProcessor>() { new Neo.Model.AuditTrail.AuditTrailProcessor<User>(this.UserResolver) });

      AggregateRootDbContext context = new(builder.Options, processingOptions);
      context.Database.EnsureCreated();

      if (generateSeedData)
      {
        await this.PopulateDbContextAsync(context);
      }

      this.dbContext = context;
      return context;
    }

    private async Task PopulateDbContextAsync(AggregateRootDbContext context)
    {
      var systemUserService = new SystemUserServiceMock<User>(this.UserResolver);
      await new Models.Initializers.AggregateRootSeedDataAsyncInitializer(context, systemUserService, null).InitializeAsync(CancellationToken.None);
      context.ChangeTracker.Clear();
    }
  }
}