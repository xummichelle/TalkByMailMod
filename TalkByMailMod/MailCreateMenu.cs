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
using StardewValley;
using StardewModdingAPI;
using StardewUI.Framework;


namespace TalkByMailMod;

public class MailCreateMenu : IClickableMenu
{
    private static int _UIWidth = 632 + borderWidth * 2;
    private static int _UIHeight = 600 + borderWidth * 2 + Game1.tileSize;

    private TextBox ContentTextBox;
    /// <summary>The OK button to draw.</summary>
    private ClickableTextureComponent DoneButton;
    private ClickableTextureComponent RecipientButton;
    private string _selectedRecipient = null;

    public MailCreateMenu()
        : base((Game1.uiViewport.Width / 2) - (_UIWidth / 2), (Game1.uiViewport.Height / 2) - (_UIHeight / 2), _UIWidth,
            _UIHeight)
    {
        ModEntry.ModMonitor.Log("initializing ui elements.", LogLevel.Debug);
        ContentTextBox = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox")
            , null, Game1.smallFont, Game1.textColor);
        var contentX = xPositionOnScreen + borderWidth + spaceToClearSideBorder;
        var contentY = yPositionOnScreen + borderWidth + spaceToClearTopBorder;
        ContentTextBox.X = contentX;
        ContentTextBox.Y = contentY;
        ContentTextBox.Width = _UIWidth - (borderWidth + spaceToClearSideBorder) * 2;
        ContentTextBox.Height = _UIHeight - (borderWidth + spaceToClearSideBorder);
        ContentTextBox.Selected = true;
        
        ContentTextBox.OnEnterPressed += (sender) => handleButtonClick("Done");
        Game1.keyboardDispatcher.Subscriber = ContentTextBox;
        
        // Game1.keyboardDispatcher.Subscriber = ContentTextBox;
        
        DoneButton = new ClickableTextureComponent("Done", 
            new Rectangle(this.xPositionOnScreen + this.width - borderWidth - spaceToClearSideBorder - Game1.tileSize, 
                this.yPositionOnScreen + this.height - borderWidth - spaceToClearTopBorder + Game1.tileSize / 4, 
                Game1.tileSize, 
                Game1.tileSize), 
            "", 
            null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 
            1f);
        RecipientButton = new ClickableTextureComponent(
            "Recipient",
            new Rectangle(
                xPositionOnScreen + borderWidth + spaceToClearSideBorder,
                yPositionOnScreen + height - borderWidth - spaceToClearTopBorder + Game1.tileSize / 4,
                Game1.tileSize * 4,
                Game1.tileSize),
            "", "Select Recipient",
            Game1.mouseCursors,
            Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46),
            1f);

    }
    
    

    private void handleButtonClick(string name)
    {
        if (name == null)
        {
            ModEntry.ModMonitor.Log("handle button click was null.", LogLevel.Debug);
            return;
        }
        string letterText = ContentTextBox.Text.Trim();
        switch (name)
        {
            case "Done":
                ModEntry.ModMonitor.Log($"Letter text: {letterText}", LogLevel.Debug);
                Game1.exitActiveMenu();
                break;
            case "Recipient":
                string currentText = ContentTextBox.Text.Trim();
                Game1.activeClickableMenu = new RecipientPickerMenu(async (recipientName) =>
                {
                    ModEntry.ModMonitor.Log($"Selected: {recipientName}", LogLevel.Debug);
                    ModEntry.ModMonitor.Log($"Text: {letterText}", LogLevel.Debug);
                    // store it for later when sending
                    _selectedRecipient = recipientName;
                    
                    if (ModEntry.MfmApi != null)
                    {
                        string letterId = $"TalkByMailMod.{_selectedRecipient}.{DateTime.Now.Ticks}";

                        try
                        {
                            var chatClient = AzureHelper.GetChatClient();
                            var msg = ContentTextBox.Text.Trim();
                            string systemPrompt = AzureHelper.GetNpcSystemPrompt(_selectedRecipient);

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
                                    Title = $"Letter for {_selectedRecipient}"
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
                });
                break;
            default:
                break;
        }
    }

    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        ContentTextBox.Update();
        if (this.DoneButton.containsPoint(x, y))
        {
            ModEntry.ModMonitor.Log("receiveleftclick on donebutton", LogLevel.Debug);
            this.handleButtonClick(this.DoneButton.name);
            this.DoneButton.scale -= 0.25f;
            this.DoneButton.scale = Math.Max(0.75f, this.DoneButton.scale);
        }
        if (this.RecipientButton.containsPoint(x, y))
        {
            this.handleButtonClick(this.RecipientButton.name);
        }
    }
    
    /// <summary>The method invoked when the player hovers the cursor over the menu.</summary>
    /// <param name="x">The X-position of the cursor.</param>
    /// <param name="y">The Y-position of the cursor.</param>
    public override void performHoverAction(int x, int y)
    {
        this.DoneButton.scale = this.DoneButton.containsPoint(x, y)
            ? Math.Min(this.DoneButton.scale + 0.02f, this.DoneButton.baseScale + 0.1f)
            : Math.Max(this.DoneButton.scale - 0.02f, this.DoneButton.baseScale);
        this.RecipientButton.scale = this.RecipientButton.containsPoint(x, y)
            ? Math.Min(this.RecipientButton.scale + 0.02f, this.RecipientButton.baseScale + 0.1f)
            : Math.Max(this.RecipientButton.scale - 0.02f, this.RecipientButton.baseScale);

    }
    /// <summary>Draw the menu to the screen.</summary>
    /// <param name="b">The sprite batch.</param>
    public override void draw(SpriteBatch b)
    {
        // draw menu box
        Game1.drawDialogueBox(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, false, true);
        //b.Draw(Game1.daybg, new Vector2((this.xPositionOnScreen + Game1.tileSize + Game1.tileSize * 2 / 3 - 2), (this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - Game1.tileSize / 4)), Color.White);
        //Game1.player.FarmerSprite.draw(b, new Vector2((this.xPositionOnScreen + Game1.tileSize + Game1.tileSize * 2 / 3 - 2), (this.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - Game1.tileSize / 4)),1f);

        
        // draw labels
        // foreach (ClickableComponent label in this.Labels)
        // {
        //     Color color = Color.Violet;
        //     Utility.drawTextWithShadow(b, label.name, Game1.smallFont, new Vector2(label.bounds.X, label.bounds.Y), color);
        // }
        // foreach (ClickableComponent label in this.Labels)
        // {
        //     string text = "";
        //     Color color = Game1.textColor;
        //     Utility.drawTextWithShadow(b, label.name, Game1.smallFont, new Vector2(label.bounds.X, label.bounds.Y), color);
        //     if (text.Length > 0)
        //         Utility.drawTextWithShadow(b, text, Game1.smallFont, new Vector2(label.bounds.X + Game1.tileSize / 3 - Game1.smallFont.MeasureString(text).X / 2f, label.bounds.Y + Game1.tileSize / 2), color);
        // }
        
        ContentTextBox.Draw(b);
        // draw OK and recipient button
        DoneButton.draw(b);
        RecipientButton.draw(b);
        Utility.drawTextWithShadow(b, 
            "Select Recipient",
            Game1.smallFont,
            new Vector2(
                RecipientButton.bounds.X + Game1.tileSize + 8,
                RecipientButton.bounds.Y + 12),
            Game1.textColor);
        // draw cursor
        this.drawMouse(b);
    }
}