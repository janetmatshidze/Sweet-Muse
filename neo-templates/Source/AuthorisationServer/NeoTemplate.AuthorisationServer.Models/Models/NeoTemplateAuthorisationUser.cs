namespace NeoTemplate.AuthorisationServer.Models
{
  using System;
  using System.ComponentModel.DataAnnotations.Schema;
  using Neo.AuthorisationServer.Models;

  [Table("Users")]
  [Serializable]
  public class NeoTemplateAuthorisationUser : AuthorisationUserBase<NeoTemplateAuthorisationUser>
  {
    /// <summary>
    /// Gets or sets a value indicating whether this user is linked to a user invite.
    /// </summary>
    public bool IsInvitedUser { get; set; }
  }
}