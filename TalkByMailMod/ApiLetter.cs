using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace TalkByMailMod;

public class ApiLetter : ILetter
{
    public string Id { get; set; }
    public string Text { get; set; }
    public string GroupId { get; set; }
    public string Title { get; set; }
    public List<Item> Items { get; set; }
    public string Recipe { get; set; }
    public int WhichBG { get; set; }
    public Texture2D LetterTexture { get; set; }
    public int? TextColor { get; set; }
    public Texture2D UpperRightCloseButtonTexture { get; set; }
    public bool AutoOpen { get; set; }
    public ITranslationHelper I18N { get; set; }
}