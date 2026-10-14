using StardewUI.Graphics;
using StardewValley;
using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Linq;
using Azure.AI.OpenAI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OpenAI.Chat;
using StardewValley.Menus;
using StardewModdingAPI;
using StardewUI.Framework;

namespace TalkByMailMod.UI;

public class MailComposeViewModel
{
    public Sprite LetterBackground { get; }

    public IReadOnlyList<string> Recipients { get; } =
        new[] { "Abigail", "Penny", "Sebastian", "Sam", "Leah", "Emily" };

    public string SelectedRecipient { get; set; } = "Abigail";

    public string LetterText { get; set; } = "";

    public MailComposeViewModel(Sprite letterBackground)
    {
        LetterBackground = letterBackground;
    }

    public async void SendTo(string recipientName)
    {
        // TODO: hand off SelectedRecipient + LetterText to your AI pipeline / MFM delivery.
        
        ModEntry.ModMonitor.Log($"Selected: {recipientName}", LogLevel.Debug);
        ModEntry.ModMonitor.Log($"Text: {LetterText}", LogLevel.Debug);
        // store it for later when sending
        SelectedRecipient = recipientName;
        
        if (ModEntry.MfmApi != null)
        {
            string letterId = $"TalkByMailMod.{SelectedRecipient}.{DateTime.Now.Ticks}";

            try
            {
                var chatClient = AzureHelper.GetChatClient();
                var msg = LetterText;
                string systemPrompt = AzureHelper.GetNpcSystemPrompt(SelectedRecipient);

                var response = await chatClient.CompleteChatAsync(
                    new SystemChatMessage(systemPrompt),
                    new UserChatMessage(msg)
                );

                var mailResponse = response.Value.Content.Last().Text;
                ModEntry.MfmApi.RegisterLetter(
                    new ApiLetter
                    {
                        Id   = letterId,
                        Text = mailResponse,
                        Title = $"Letter for {SelectedRecipient}"
                    },
                    condition: (l) => !Game1.player.mailReceived.Contains(l.Id),
                    callback:  (l) =>  Game1.player.mailReceived.Add(l.Id)
                );
            }
            catch (Exception ex)
            {
                // this runs in an async void callback, so an unhandled exception would crash the game
                ModEntry.ModMonitor.Log($"Couldn't get a reply letter: {ex}", LogLevel.Error);
            }
        }
    }

    public void Cancel()
    {
        Game1.exitActiveMenu();
    }
}