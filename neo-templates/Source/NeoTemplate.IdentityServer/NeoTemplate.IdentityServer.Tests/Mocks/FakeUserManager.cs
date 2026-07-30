namespace NeoTemplate.IdentityServer.Tests.Mocks
{
  using System;
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Identity;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Logging.Abstractions;
  using NeoTemplate.IdentityServer.Models;

  public class FakeUserManager<TUser> : UserManager<TUser>
    where TUser : NeoTemplateApplicationUser
  {
    private readonly Func<string?, Task<TUser?>> findByEmail;

    public FakeUserManager(TUser applicationUser)
      : this(email => Task.FromResult<TUser?>(string.Equals(email, applicationUser.Email, StringComparison.OrdinalIgnoreCase) ? applicationUser : null))
    {
    }

    public FakeUserManager(IdentityDbContext identityDbContext)
      : this(async email =>
        {
          if (email == null)
          {
            return null;
          }

          var user = await identityDbContext.Users
            .FirstOrDefaultAsync(appUser => string.Equals(appUser.Email, email, StringComparison.OrdinalIgnoreCase));
          return user as TUser;
        })
    {
    }

    private FakeUserManager(Func<string?, Task<TUser?>> findByEmail)
      : base(new FakeUserStore<TUser>(),
            Microsoft.Extensions.Options.Options.Create(new IdentityOptions()),
            new FakePasswordHasher<TUser>(),
            new IUserValidator<TUser>[0],
            new IPasswordValidator<TUser>[0],
            new FakeLookupNormalizer(),
            new IdentityErrorDescriber(),
            new FakeServiceProvider(),
            NullLogger<UserManager<TUser>>.Instance)
    {
      this.findByEmail = findByEmail;
    }

    /// <inheritdoc />
    public override Task<TUser?> FindByEmailAsync(string? email)
    {
      return this.findByEmail(email);
    }
  }
}
