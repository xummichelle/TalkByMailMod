using StardewUI.Graphics;
using StardewValley;

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

    public void Send()
    {
        // TODO: hand off SelectedRecipient + LetterText to your AI pipeline / MFM delivery.
        Game1.exitActiveMenu();
    }

    public void Cancel()
    {
        Game1.exitActiveMenu();
    }
}