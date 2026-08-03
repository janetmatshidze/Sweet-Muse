namespace NeoTemplate.AuthorisationServer.Models
{
  using Microsoft.EntityFrameworkCore;
  using Neo.AuthorisationServer.Models;
  using Neo.Model.MultiTenancy;
  using Neo.Model.Processing;

  public class AuthorisationDbContext : ModelDbContextBase<AuthorisationDbContext, NeoTemplateAuthorisationUser>
  {
    public AuthorisationDbContext(
      DbContextOptions options,
      DbContextProcessingOptions<AuthorisationDbContext> processingOptions,
      ITenantService tenantService)
      : base(options, processingOptions, tenantService)
    {
    }
  }
}