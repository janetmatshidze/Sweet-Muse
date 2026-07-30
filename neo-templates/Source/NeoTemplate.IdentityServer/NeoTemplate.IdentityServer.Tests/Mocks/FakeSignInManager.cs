namespace NeoTemplate.IdentityServer.Tests.Mocks
{
  using System.Net;
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Http;
  using Microsoft.AspNetCore.Identity;
  using Microsoft.Extensions.Logging.Abstractions;
  using Neo.IdentityServer.App.Services;
  using Neo.IdentityServer.App.Services.IdentityProviders;
  using NeoTemplate.IdentityServer;
  using NeoTemplate.IdentityServer.App.Services;
  using NeoTemplate.IdentityServer.Models;

  public class FakeSignInManager : SignInManager
  {
    private const string UserAgent = "User-Agent";

    public FakeSignInManager(NeoTemplateApplicationUser applicationUser, IdentityDbContext identityDbContext, IIdentityProviderLookupService identityProviderService)
          : base(new FakeUserManager<NeoTemplateApplicationUser>(applicationUser),
                new HttpContextAccessor(),
                new FakeUserClaimsPrincipalFactory<NeoTemplateApplicationUser>(),
                Microsoft.Extensions.Options.Options.Create(new IdentityOptions()),
                NullLogger<SignInManager>.Instance,
                new FakeAuthenticationSchemeProvider(),
                new FakeUserConfirmation<NeoTemplateApplicationUser>(),
                new SignInAuditService<IdentityDbContext>(identityDbContext),
                new MfaManager(),
                identityProviderService)
    {
      this.Context = new DefaultHttpContext();
      this.Context.Request.Headers[UserAgent] = "TestAgent";
      this.Context.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
    }

    /// <inheritdoc />
    public override Task<SignInResult> PasswordSignInAsync(NeoTemplateApplicationUser user, string password, bool isPersistent, bool lockoutOnFailure)
    {
      if (password == "Password")
      {
        return Task.FromResult(SignInResult.Success);
      }
      return Task.FromResult(SignInResult.Failed);
    }
  }
}
