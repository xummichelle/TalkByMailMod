namespace TalkByMailMod;

/// <summary>The player's settings, read from config.json in the mod folder. SMAPI creates the file with these defaults on first launch.</summary>
public class ModConfig
{
    /// <summary>The Azure OpenAI API key, from the resource's Keys and Endpoint page.</summary>
    public string ApiKey { get; set; } = "";

    /// <summary>The Azure OpenAI endpoint URL, like https://your-resource.services.ai.azure.com/.</summary>
    public string Endpoint { get; set; } = "";

    /// <summary>The name of the model deployment to send letters to.</summary>
    public string DeploymentName { get; set; } = "";
}
