namespace NeoTemplate.Core.Api
{
  /// <summary>
  /// The NeoTemplate Setup action names class.
  /// </summary>
  public static class StartupActions
  {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public const string DataServices = "NeoTemplateDataServices";

    public const string UserDataServices = "NeoTemplateUserDataServices";

    public const string ModelServices = "NeoTemplateModelServices";

    public const string ProcessingServices = "NeoTemplateProcessingServices";

    public const string IntegrationServices = "NeoTemplateIntegrationServices";

    public const string IntegrityChecking = "NeoTemplateIntegrityChecking";

    public const string Jobs = "NeoTemplateJobs";

    public const string EntityChangePublishers = "NeoTemplateEntityChangePublishers";

    public const string FileStorageServices = "NeoTemplateFileStorageServices";

    public const string Caches = "NeoTemplateCaches";

    public const string SignalR = "NeoTemplateSignalR";

    public const string Modules = "NeoTemplateModules";

    public const string MultiTenancy = "NeoTemplateMultiTenancy";

    public const string Logging = "NeoTemplateLogging";

    public const string SecretVaults = "NeoTemplateSecretVault";

    public const string AlwaysEncrypted = "NeoTemplateAlwaysEncrypted";
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
  }
}