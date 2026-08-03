namespace NeoTemplate.IdentityServer.Tests.Services
{
  using System;
  using System.Globalization;
  using System.Threading.Tasks;
  using Microsoft.EntityFrameworkCore;
  using Neo.AuthorisationServer.Client;
  using Neo.IdentityServer.Models.IdentityProviders;
  using Neo.Model.Exceptions;
  using Neo.Model.Identity.SystemUser;
  using Neo.Model.Services;
  using NeoTemplate.IdentityServer.App;
  using NeoTemplate.IdentityServer.App.Services.UserManagement;
  using NeoTemplate.IdentityServer.Contracts.UserManagement;
  using NeoTemplate.IdentityServer.Contracts.UserManagement.Commands;
  using NeoTemplate.IdentityServer.Contracts.UserManagement.Queries;
  using NeoTemplate.IdentityServer.Models;
  using NeoTemplate.IdentityServer.Models.UserManagement;
  using NeoTemplate.IdentityServer.Tests.Mocks;
  using Xunit;

  public class UserManagementServiceTests
  {
    private readonly IdentityDbContext dbContext;
    private readonly UserManagementService userManagementService;
    private readonly FakeRegistrationEmailService registrationEmailService;
    private readonly FakeMfaManager mfaManager;
    private readonly string UserJohnId = Guid.NewGuid().ToString();
    private readonly string UserJeffId = Guid.NewGuid().ToString();
    private readonly string UserSarahId = Guid.NewGuid().ToString();
    private readonly string UserSusanId = Guid.NewGuid().ToString();

    public UserManagementServiceTests()
    {
      this.dbContext = UnitTestHelper.InitIdentityDbContext();

      var authorisationService = new FakeAuthorisationService();
      this.registrationEmailService = new FakeRegistrationEmailService();
      this.mfaManager = new FakeMfaManager { UserRequiresTwoFactorResult = true };

      var userManager = new FakeUserManager<NeoTemplateApplicationUser>(this.dbContext);

      this.userManagementService = new UserManagementService(
        this.dbContext,
        authorisationService,
        this.registrationEmailService,
        new QueryService(),
        new SystemUserOptions<NeoTemplateApplicationUser>() { SystemUserGuid = Guid.NewGuid() },
        this.mfaManager,
        userManager);

      this.SetupTestData();
    }

    [Fact]
    public async Task FindUsersAsync()
    {
      var criteria = new UserLookupCriteria()
      {
        LastName = "User",
      };

      var request = new PageRequest<UserLookupCriteria>(criteria);
      var results = await this.userManagementService.FindUsersAsync(request);

      // Should be 4 users
      Assert.Equal(4, results.EntityList.Count);

      Assert.Collection(
        results.EntityList,
        userLookup => Assert.Equal("John", userLookup.FirstName),
        userLookup => Assert.Equal("Jeff", userLookup.FirstName),
        userLookup => Assert.Equal("Sarah", userLookup.FirstName),
        userLookup => Assert.Equal("Susan", userLookup.FirstName));

      // should find 0 XUsers
      criteria.LastName = "XUser";
      results = await this.userManagementService.FindUsersAsync(request);
      Assert.Empty(results.EntityList);

      // should find 2 Users
      criteria.LastName = string.Empty;
      criteria.FirstName = "J";

      // should find 2 different users
      results = await this.userManagementService.FindUsersAsync(request);
      Assert.Equal(2, results.EntityList.Count);

      Assert.Collection(
        results.EntityList,
        userLookup => Assert.Equal("John", userLookup.FirstName),
        userLookup => Assert.Equal("Jeff", userLookup.FirstName));

      // search on "contains" username
      criteria.FirstName = "";
      criteria.LastName = "";
      criteria.UserName = "@test.com";
      results = await this.userManagementService.FindUsersAsync(request);
      Assert.Equal(4, results.EntityList.Count);
    }

    [Fact]
    public Task PerformUserAction_ResendEmailVerificationLink()
    {
      return this.AssertUserActionAsync(
        UserManagementAction.ResendEmailVerificationLink,
        this.UserJohnId,
        user =>
        {
          var sentToUser = Assert.Single(this.registrationEmailService.SentToUsers);
          Assert.Equal(this.UserJohnId, sentToUser.Id);
        });
    }

    [Fact]
    public async Task PerformUserAction_ResetMFA()
    {
      // now make sure MFA is enabled and configured
      var user = await this.dbContext.Users.FirstAsync(appUser => appUser.Id == this.UserJohnId, TestContext.Current.CancellationToken);
      user.TwoFactorEnabled = true;
      user.ConfigureTwoFactor(true);
      this.dbContext.SaveChanges();

      await this.AssertUserActionAsync(
        UserManagementAction.ResetMFA,
        this.UserJohnId,
        user =>
        {
          Assert.False(user.TwoFactorConfigured);
        });
    }

    [Fact]
    public async Task PerformUserAction_ClearLockout()
    {
      // now make sure MFA is enabled and configured
      var user = await this.dbContext.Users.FirstAsync(appUser => appUser.Id == this.UserJohnId, TestContext.Current.CancellationToken);
      user.LockoutEnabled = true;
      user.LockoutEnd = DateTime.UtcNow.AddMinutes(10);
      this.dbContext.SaveChanges();

      await this.AssertUserActionAsync(
        UserManagementAction.ClearLockout,
        this.UserJohnId,
        user =>
        {
          Assert.Null(user.LockoutEnd);
        });
    }

    [Fact]
    public async Task PerformUserAction_Unblock()
    {
      // now make sure MFA is enabled and configured
      var user = await this.dbContext.Users.FirstAsync(appUser => appUser.Id == this.UserJohnId, TestContext.Current.CancellationToken);
      user.Deactivate();
      this.dbContext.SaveChanges();

      await this.AssertUserActionAsync(
        UserManagementAction.Activate,
        this.UserJohnId,
        user =>
        {
          Assert.True(user.IsActive);
        });
    }

    [Fact]
    public async Task PerformUserAction_Block()
    {
      // now make sure MFA is enabled and configured
      var user = await this.dbContext.Users.FirstAsync(appUser => appUser.Id == this.UserJohnId, TestContext.Current.CancellationToken);
      user.Activate();
      this.dbContext.SaveChanges();

      await this.AssertUserActionAsync(
        UserManagementAction.Deactivate,
        this.UserJohnId,
        user =>
        {
          Assert.False(user.IsActive);
        });
    }

    [Fact]
    public async Task PerformUserAction_EnableMFA()
    {
      // now make sure MFA is enabled and configured
      var user = await this.dbContext.Users.FirstAsync(appUser => appUser.Id == this.UserJohnId, TestContext.Current.CancellationToken);
      user.TwoFactorEnabled = false;
      this.dbContext.SaveChanges();

      await this.AssertUserActionAsync(
        UserManagementAction.EnableMFA,
        this.UserJohnId,
        user =>
        {
          Assert.True(user.TwoFactorEnabled);
        });
    }

    [Fact]
    public async Task PerformUserAction_DisableMFA()
    {
      // now make sure MFA is enabled and configured
      var user = await this.dbContext.Users.FirstAsync(appUser => appUser.Id == this.UserJohnId, TestContext.Current.CancellationToken);
      user.TwoFactorEnabled = true;
      this.dbContext.SaveChanges();

      var ex = await Assert.ThrowsAsync<InvalidDomainOperationException>(() => this.AssertUserActionAsync(
        UserManagementAction.DisableMFA,
        this.UserJohnId,
        user =>
        {
          Assert.False(user.TwoFactorEnabled);
        }));

      Assert.Equal(
        string.Format(CultureInfo.CurrentCulture, DomainExceptions.CannotPerformActionMFAIsRequired, "Credentials"),
        ex.Message);

      this.mfaManager.UserRequiresTwoFactorResult = false;

      await this.AssertUserActionAsync(
        UserManagementAction.DisableMFA,
        this.UserJohnId,
        user =>
        {
          Assert.False(user.TwoFactorEnabled);
        });
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Assertions", "xUnit2023:Do not use collection methods for single-item collections", Justification = "Test collection")]
    public async Task UserInvite()
    {
      var savedInvite = await this.userManagementService.SaveUserInviteAsync(new UserInvite()
      {
        EmailAddress = "John_User@test.com",
        AddToUserGroupId = 1,
        TrackingState = TrackableEntities.Common.Core.TrackingState.Added,
      });

      var user = await this.dbContext.Users.FirstAsync(appUser => appUser.Email == "John_User@test.com", TestContext.Current.CancellationToken);

      Assert.Equal(savedInvite.UserInviteId, user.UserInviteId);
      Assert.Collection(this.registrationEmailService.SentUserInvites, userInvite => Assert.Equal(userInvite.EmailAddress, savedInvite.EmailAddress));

      var ex = await Assert.ThrowsAsync<InvalidDomainOperationException>(() => this.userManagementService.RevokeUserInviteAsync(savedInvite.UserInviteId));

      Assert.Equal(DomainExceptions.UserInviteAlreadyRegistered, ex.Message);
    }

    private async Task AssertUserActionAsync(UserManagementAction action, string userId, Action<NeoTemplateApplicationUser> assertAction)
    {
      var command = new PerformUserActionCommand() { Action = action, UserId = userId };
      await this.userManagementService.PerformUserActionAsync(command);
      var user = await this.dbContext.Users.FirstAsync(appUser => appUser.Id == userId);
      assertAction(user);
    }

    private void SetupTestData()
    {
      this.dbContext.IdentityProviders.Add(
        new IdentityProvider()
        {
          IdentityProviderId = 1,
          IdentityProviderType = (int)IdentityProviderType.LoginCredentials,
          Name = "Credentials",
          DisplayName = "Credentials",
          NameSuffix = "creds",
        });

      this.AddUser(this.UserJohnId, "John", "User");
      this.AddUser(this.UserJeffId, "Jeff", "User");
      this.AddUser(this.UserSarahId, "Sarah", "User");
      this.AddUser(this.UserSusanId, "Susan", "User");

      this.dbContext.SaveChanges();
    }

    private void AddUser(string id, string firstName, string lastName)
    {
      var user = new NeoTemplateApplicationUser()
      {
        Id = id,
        FirstName = firstName,
        LastName = lastName,
        Email = $"{firstName}_{lastName}@test.com",
        UserName = $"{firstName}_{lastName}@test.com",
        IdentityProviderId = 1,
        IsActive = true,
      };

      this.dbContext.Users.Add(user);
    }
  }
}
