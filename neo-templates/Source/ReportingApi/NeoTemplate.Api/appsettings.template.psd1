@{
  AppSettings = @{
    ASPNETCORE_ENVIRONMENT = "{Environment}"
    Authentication__AuthenticationAuthorityUrl = "https://{AppCoreSubDomain}.{AppDomain}{AppFolder}"
    Authentication__AuthorizationUrl = "https://{AppCoreSubDomain}.{AppDomain}{AppFolder}/connect/authorize"
    Authentication__TokenUrl = "https://{AppCoreSubDomain}.{AppDomain}{AppFolder}/connect/token"
    Routing__PathBase = "{AppFolder}"
    WEBSITE_HTTPLOGGING_RETENTION_DAYS = "7"
    WEBSITE_WEBDEPLOY_USE_SCM = "false"
  }
  ConnectionStrings = @{
    Main = @{ 
      Value = "Server={SqlServerName};Initial Catalog=NeoTemplate;Persist Security Info=False;User ID={SqlUserName};Password={SqlPassword};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;"
      Type = "SQLServer"
    }
  }
}
