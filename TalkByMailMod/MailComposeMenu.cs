using StardewUI;         // ViewMenu, IView — both declared in the plain StardewUI namespace
using StardewUI.Layout;  // LayoutParameters, Length
using StardewUI.Widgets;
using TalkByMailMod;
using TalkByMailMod.UI;
using StardewValley;

namespace TalkByMailMod;
public class MailComposeMenu : ViewMenu
{
    private static MailComposeViewModel? pendingViewModel;

    public static MailComposeMenu Create(MailComposeViewModel viewModel)
    {
        pendingViewModel = viewModel;
        return new MailComposeMenu();
    }

    private MailComposeMenu() { }

    protected override IView CreateView()
    {
        var viewModel = pendingViewModel
                        ?? throw new InvalidOperationException("Use MailComposeMenu.Create(viewModel) instead of new.");
        pendingViewModel = null;

        var textInput = new MultilineTextInput
        {
            Layout = new LayoutParameters { Width = Length.Px(620), Height = Length.Px(400) },
        };
        textInput.TextChanged += (_, _) => viewModel.LetterText = textInput.Text;

        var sendButton = new Button { Text = "Send" };
        sendButton.Click += (_, _) => 
            Game1.activeClickableMenu = new RecipientPickerMenu(recipient => viewModel.SendTo(recipient));

        var cancelButton = new Button { Text = "Cancel" };
        cancelButton.Click += (_, _) => viewModel.Cancel();

        return new Frame
        {
            Background = viewModel.LetterBackground,
            Layout = new LayoutParameters { Width = Length.Px(640), Height = Length.Px(480) },
            Padding = new Edges(32, 48, 32, 32),
            Content = new Lane
            {
                Orientation = Orientation.Vertical,
                Children = new List<IView>
                {
                    textInput,
                    new Lane { Children = new List<IView> { cancelButton, sendButton } },
                },
            },
        };
    }
}