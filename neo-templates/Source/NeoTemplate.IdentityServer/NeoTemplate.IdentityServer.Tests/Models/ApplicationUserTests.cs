namespace NeoTemplate.IdentityServer.Tests.Models
{
  using System;
  using NeoTemplate.IdentityServer.Models;
  using Xunit;

  public class ApplicationUserTests
  {
    [Fact]
    public void ConstructApplicationUser()
    {
      // Arrange
      string userID = Guid.NewGuid().ToString();

      // Act
      var applicationUser = new NeoTemplateApplicationUser
      {
        Id = userID,
        UserName = "user@test.com",
        FirstName = "User",
        LastName = "Test",
        IsActive = true,
      };

      // Assert
      Assert.True(applicationUser.IsActive);
      Assert.Equal(0, applicationUser.UserId);
      Assert.Equal(userID, applicationUser.UserIdentifier);
      Assert.True(applicationUser.IdentityGuid.HasValue);
    }
  }
}
