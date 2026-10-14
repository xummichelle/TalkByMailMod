using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Menus;
using StardewValley;
using StardewModdingAPI;

namespace TalkByMailMod;

public class RecipientPickerMenu : IClickableMenu
{
    private List<ClickableTextureComponent> _npcButtons = new();
    public string SelectedRecipient = null;
    private Action<string> _onSelected;

    private static int _UIWidth = 632 + borderWidth * 2;
    private static int _UIHeight = 600 + borderWidth * 2 + Game1.tileSize;

    public RecipientPickerMenu(Action<string> onSelected)
        : base(
            (Game1.uiViewport.Width / 2) - (_UIWidth / 2),
            (Game1.uiViewport.Height / 2) - (_UIHeight / 2),
            _UIWidth, _UIHeight)
    {
        _onSelected = onSelected;

        int contentX = xPositionOnScreen + borderWidth + spaceToClearSideBorder;
        int contentY = yPositionOnScreen + borderWidth + spaceToClearTopBorder + 16;

        // Get all NPCs the player has met
        var npcs = Utility.getAllCharacters()
            .Where(n => n.isVillager() && Game1.player.friendshipData.ContainsKey(n.Name))
            .ToList();

        int col = 0;
        int row = 0;
        int buttonSize = Game1.tileSize; // was tileSize * 2, now just tileSize
        int colWidth = 150;  // wide enough for names, TODO: adjust if needed
        int rowHeight = buttonSize + 40;  // portrait + name space
        int padding = 8;

        foreach (var npc in npcs)
        {
            int x = contentX + col * colWidth;
            int y = contentY + row * (rowHeight + padding);

            _npcButtons.Add(new ClickableTextureComponent(
                npc.Name,
                new Rectangle(x, y, buttonSize, buttonSize),
                null,
                npc.displayName,
                npc.Sprite.Texture,
                npc.getMugShotSourceRect(),
                2f
            ));

            col++;
            if (col >= 4) { col = 0; row++; }
        }
    }

    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        foreach (var button in _npcButtons)
        {
            if (button.containsPoint(x, y))
            {
                _onSelected(button.name);
                Game1.exitActiveMenu();
                return;
            }
        }
    }

    public override void performHoverAction(int x, int y)
    {
        foreach (var button in _npcButtons)
        {
            button.scale = button.containsPoint(x, y)
                ? Math.Min(button.scale + 0.02f, button.baseScale + 0.1f)
                : Math.Max(button.scale - 0.02f, button.baseScale);
        }
    }

    public override void draw(SpriteBatch b)
    {
        Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, false, true);

        foreach (var button in _npcButtons)
        {
            button.draw(b);
            // draw NPC name below portrait
            Utility.drawTextWithShadow(b, button.hoverText,
                Game1.smallFont,
                new Vector2(button.bounds.X, button.bounds.Y + button.bounds.Height + 4),
                Game1.textColor);
        }

        drawMouse(b);
    }
}