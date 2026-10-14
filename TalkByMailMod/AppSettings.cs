namespace TalkByMailMod;

/// <summary>The settings read from appsettings.json in the mod folder.</summary>
public class AppSettings
{
    public KeyVaultSettings KeyVault { get; set; } = new();
}

public class KeyVaultSettings
{
    public string VaultUri { get; set; } = "https://stardew-key-vault.vault.azure.net/";
    public string OpenAiKeySecretName { get; set; } = "stardew-azure-openai-api-key";
    public string OpenAiEndpointSecretName { get; set; } = "stardew-azure-openai-api-endpoint";
    public string OpenAiDeploymentNameSecretName { get; set; } = "stardew-azure-openai-api-deployment-name";
}
