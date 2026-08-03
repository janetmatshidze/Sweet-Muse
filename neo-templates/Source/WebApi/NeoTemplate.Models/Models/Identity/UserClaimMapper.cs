namespace NeoTemplate.Models.Identity
{
  using System;
  using Neo.Identity;
  using Neo.Model.Identity;

  /// <summary>
  /// Will map the identity claims to the User entity.
  /// </summary>
  public class UserClaimMapper : UserClaimMapperBase<User>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="UserClaimMapper"/> class.
    /// </summary>
    /// <param name="user">The Claims Principal user.</param>
    public UserClaimMapper(System.Security.Claims.ClaimsPrincipal? user)
      : base(user)
    {
    }

    /// <summary>
    /// Will create a user from the information in the claims.
    /// </summary>
    /// <returns>A user populated with information in the claims.</returns>
    public override User? CreateNewUser()
    {
      if (this.IsHumanUser)
      {
        return new User()
        {
          IdentityGuid = Guid.Parse(this.UserIdentifier),
          UserName = this.GetClaimValue(ClaimType.Email),
          FirstName = this.GetClaimValue(ClaimType.FirstName),
          LastName = this.GetClaimValue(ClaimType.LastName),
        };
      }
      else if (this.IsClientAppUser)
      {
        return new User()
        {
          ClientId = this.UserIdentifier,
          UserName = $"{this.UserIdentifier}@client.com",
          FirstName = "Client",
          LastName = this.UserIdentifier,
        };
      }
      return null;
    }

    /// <summary>
    /// Will update the user from the information in the claims.
    /// </summary>
    /// <param name="user">The user to be updated.</param>
    public override void UpdateUser(User user)
    {
      if (user == null)
      {
        throw new ArgumentNullException(nameof(user));
      }

      var claimUser = this.CreateNewUser() ?? throw new InvalidOperationException("A user is required but there is no user in scope. Override the user using the IOverridableUserResolver or the ISystemUserService services.");
      user.UserName = claimUser.UserName;
      user.FirstName = claimUser.FirstName;
      user.LastName = claimUser.LastName;
    }
  }
}
