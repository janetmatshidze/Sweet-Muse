namespace Tuckshop.IdentityServer.Tests.Services
{
  using System;
  using System.Threading.Tasks;
  using Tuckshop.IdentityServer.Models;
  using Tuckshop.IdentityServer.Tests.Mocks;
  using Xunit;

  /// <summary>
  /// Tests for <see cref="FakeUserManager{TUser}"/>. These guard the small amount of real
  /// behaviour in the fake - specifically the email lookup used by the services under test -
  /// so that a later "simplification" of the case-insensitive comparison does not silently
  /// change how the fake resolves users.
  /// </summary>
  public class FakeUserManagerTests
  {
    [Fact]
    public async Task FindByEmailAsync_WithDbContext_ReturnsMatchingUser()
    {
      var dbContext = UnitTestHelper.InitIdentityDbContext();
      var expected = AddUser(dbContext, "user@test.com");

      var userManager = new FakeUserManager<TuckshopApplicationUser>(dbContext);

      var user = await userManager.FindByEmailAsync("user@test.com");

      Assert.NotNull(user);
      Assert.Equal(expected.Id, user.Id);
    }

    [Fact]
    public async Task FindByEmailAsync_WithDbContext_MatchesEmailCaseInsensitively()
    {
      var dbContext = UnitTestHelper.InitIdentityDbContext();
      var expected = AddUser(dbContext, "user@test.com");

      var userManager = new FakeUserManager<TuckshopApplicationUser>(dbContext);

      var user = await userManager.FindByEmailAsync("USER@Test.CoM");

      Assert.NotNull(user);
      Assert.Equal(expected.Id, user.Id);
    }

    [Fact]
    public async Task FindByEmailAsync_WithDbContext_ReturnsNullWhenNoMatch()
    {
      var dbContext = UnitTestHelper.InitIdentityDbContext();
      AddUser(dbContext, "user@test.com");

      var userManager = new FakeUserManager<TuckshopApplicationUser>(dbContext);

      var user = await userManager.FindByEmailAsync("other@test.com");

      Assert.Null(user);
    }

    [Fact]
    public async Task FindByEmailAsync_WithDbContext_ReturnsNullWhenEmailIsNull()
    {
      var dbContext = UnitTestHelper.InitIdentityDbContext();
      AddUser(dbContext, "user@test.com");

      var userManager = new FakeUserManager<TuckshopApplicationUser>(dbContext);

      var user = await userManager.FindByEmailAsync(null);

      Assert.Null(user);
    }

    [Fact]
    public async Task FindByEmailAsync_WithSingleUser_MatchesConfiguredUserOnly()
    {
      var applicationUser = new TuckshopApplicationUser() { Email = "user@test.com" };

      var userManager = new FakeUserManager<TuckshopApplicationUser>(applicationUser);

      Assert.Same(applicationUser, await userManager.FindByEmailAsync("user@test.com"));
      Assert.Null(await userManager.FindByEmailAsync("other@test.com"));
    }

    private static TuckshopApplicationUser AddUser(IdentityDbContext dbContext, string email)
    {
      var user = new TuckshopApplicationUser()
      {
        Id = Guid.NewGuid().ToString(),
        FirstName = "Test",
        LastName = "User",
        Email = email,
        UserName = email,
        IdentityProviderId = 1,
        IsActive = true,
      };

      dbContext.Users.Add(user);
      dbContext.SaveChanges();

      return user;
    }
  }
}
