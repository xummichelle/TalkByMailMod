using Azure.AI.OpenAI;
using OpenAI.Chat;
using System.ClientModel;

namespace TalkByMailMod;

/// <summary>Helpers for talking to Azure OpenAI and building the NPC prompts.</summary>
public static class AzureHelper
{
    /// <summary>Create a chat client from the API key, endpoint and deployment name in config.json.</summary>
    /// <exception cref="InvalidOperationException">config.json is missing a value.</exception>
    public static ChatClient GetChatClient()
    {
        var config = ModEntry.Config;
        if (string.IsNullOrWhiteSpace(config.ApiKey) || string.IsNullOrWhiteSpace(config.Endpoint) || string.IsNullOrWhiteSpace(config.DeploymentName))
            throw new InvalidOperationException("Fill in ApiKey, Endpoint and DeploymentName in the mod's config.json, then restart the game.");

        var client = new AzureOpenAIClient(new Uri(config.Endpoint), new ApiKeyCredential(config.ApiKey));
        return client.GetChatClient(config.DeploymentName);
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
