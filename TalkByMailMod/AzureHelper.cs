using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace TalkByMailMod;

/// <summary>Helpers for talking to Azure: reading secrets from Key Vault and building the NPC prompts.</summary>
public static class AzureHelper
{
    /// <summary>Secret values already read from Key Vault, keyed by secret name.</summary>
    private static readonly Dictionary<string, string> SecretCache = new();

    /// <summary>Get the Azure OpenAI key from Key Vault, signing in with the local Azure login (e.g. az login).</summary>
    public static Task<string> GetOpenAiKeyAsync() => GetSecretAsync(s => s.OpenAiKeySecretName);

    /// <summary>Get the Azure OpenAI endpoint URL from Key Vault.</summary>
    public static Task<string> GetOpenAiEndpointAsync() => GetSecretAsync(s => s.OpenAiEndpointSecretName);

    /// <summary>Get the Azure OpenAI deployment name from Key Vault.</summary>
    public static Task<string> GetOpenAiDeploymentNameAsync() => GetSecretAsync(s => s.OpenAiDeploymentNameSecretName);

    /// <summary>Read a secret from Key Vault, or return it from the cache if it was already read.</summary>
    /// <param name="pickName">Picks which secret name to read from the Key Vault settings.</param>
    private static async Task<string> GetSecretAsync(Func<KeyVaultSettings, string> pickName)
    {
        var settings = ModEntry.ModHelper.Data.ReadJsonFile<AppSettings>("appsettings.json")
                       ?? throw new InvalidOperationException("appsettings.json was not found in the mod folder.");
        string secretName = pickName(settings.KeyVault);
        if (SecretCache.TryGetValue(secretName, out var cached))
            return cached;

        var secretClient = new SecretClient(new Uri(settings.KeyVault.VaultUri), new DefaultAzureCredential());
        KeyVaultSecret secret = await secretClient.GetSecretAsync(secretName);
        SecretCache[secretName] = secret.Value;
        return secret.Value;
    }

    /// <summary>Get the system prompt that tells the model how an NPC should write their reply letter.</summary>
    public static string GetNpcSystemPrompt(string npcName)
    {
        return npcName switch
        {
            "Abigail" => @"You are Abigail from Stardew Valley.
            You are adventurous, love the occult and dark aesthetics, play video games, and have purple hair.
            You sometimes feel stifled by your parents' expectations.
            You are writing a letter back to the farmer who wrote to you.
            Keep your response to 3-4 sentences.
            Write in a casual, slightly edgy tone.
            Sign off with '- Abigail'.",

            "Penny" => @"You are Penny from Stardew Valley.
            You are gentle, caring, and love books and teaching children.
            You live in a trailer with your mom Pam and dream of a better life.
            You are writing a letter back to the farmer who wrote to you.
            Keep your response to 3-4 sentences.
            Write in a warm, sincere tone.
            Sign off with '- Penny'.",

            "Harvey" => @"You are Harvey from Stardew Valley.
            You are the town doctor, kind but nervous and awkward.
            You are passionate about medicine and vintage radio equipment.
            You are writing a letter back to the farmer who wrote to you.
            Keep your response to 3-4 sentences.
            Write in a formal but warm tone.
            Sign off with '- Dr. Harvey'.",

            _ => $@"You are {npcName} from Stardew Valley.
            You are writing a letter back to the farmer who wrote to you.
            Keep your response to 3-4 sentences.
            Write in character.
            Sign off with '- {npcName}'."
        };
    }
}
