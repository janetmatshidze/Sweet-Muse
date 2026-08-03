namespace NeoTemplate.Models.DummyUser
{
  using System;
  using Neo.Identity;

  // NB: NB: THIS IS A DUMMY FILE AND MUST BE DELTED.
  // The project is supposed to the main modular app User class.
  // Eg the Core User's class for the Domain project.

  /// <inheritdoc/>
  public class User : IUser, IClientUser
  {
    /// <inheritdoc/>   
    public string UserIdentifier
    {
      get
      {
        throw new NotImplementedException();
      }
    }

    /// <inheritdoc/> 
    public int UserId
    {
      get
      {
        throw new NotImplementedException();
      }
    }

    /// <inheritdoc/> 
    public Guid? IdentityGuid
    {
      get
      {
        throw new NotImplementedException();
      }
    }

    /// <inheritdoc/> 
    public string ClientId
    {
      get
      {
        throw new NotImplementedException();
      }
    }
  }
}
