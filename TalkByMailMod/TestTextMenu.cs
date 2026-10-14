using StardewUI;         // ViewMenu, IView — both declared in the plain StardewUI namespace
using StardewUI.Layout;  // LayoutParameters, Length
using TalkByMailMod.UI;

namespace TalkByMailMod;

public class TestTextMenu : ViewMenu
{
    protected override IView CreateView()
    {
        return new MultilineTextInput
        {
            Layout = new LayoutParameters { Width = Length.Px(500), Height = Length.Px(300) },
        };
    }
}