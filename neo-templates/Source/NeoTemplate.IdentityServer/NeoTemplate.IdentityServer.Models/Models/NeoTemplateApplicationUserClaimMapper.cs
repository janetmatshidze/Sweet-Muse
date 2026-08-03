namespace NeoTemplate.IdentityServer.Models
{
  using Neo.Identity;
  using Neo.Model.Identity;

  /// <summary>
  /// Claim mapper to map token claims to a user object.
  /// </summary>
  public class NeoTemplateApplicationUserClaimMapper : UserClaimMapperBase<NeoTemplateApplicationUser>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="NeoTemplateApplicationUserClaimMapper"/> class.
    /// </summary>
    /// <param name="user">The Claims Principal user.</param>
    public NeoTemplateApplicationUserClaimMapper(System.Security.Claims.ClaimsPrincipal user)
      : base(user)
    {
    }

    /// <summary>
    /// Will create a user from the information in the claims.
    /// </summary>
    /// <returns>A user populated with information in the claims.</returns>
    public override NeoTemplateApplicationUser? CreateNewUser()
    {
      // this returns the user from the claims
      if (this.IsHumanUser)
      {
        var user = new NeoTemplateApplicationUser()
        {
          Id = this.UserIdentifier,
          UserName = this.GetClaimValue(ClaimType.Email),
          FirstName = this.GetClaimValue(ClaimType.FirstName),
          LastName = this.GetClaimValue(ClaimType.LastName),
        };

        return user;
      }
      else if (this.IsClientAppUser)
      {
        return new NeoTemplateApplicationUser()
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
    public override void UpdateUser(NeoTemplateApplicationUser user)
    {
      // since IDS is the user source so user will always be up to date
    }
  }
}
