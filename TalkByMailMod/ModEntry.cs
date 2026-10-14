using StardewModdingAPI.Events;
using StardewModdingAPI;
using HarmonyLib;
using StardewValley;
using StardewUI.Framework;
using StardewUI;
using StardewUI.Graphics;         // Sprite
using Microsoft.Xna.Framework;    // Rectangle
using Microsoft.Xna.Framework.Graphics;
using TalkByMailMod.UI;

namespace TalkByMailMod;

using StardewModdingAPI;

/// <summary>The mod entry point.</summary>
internal sealed class ModEntry : Mod
{
    public static IModHelper ModHelper;
    public static IMonitor ModMonitor;
    public static IManifest Manifest;
    public static IMailFrameworkModApi MfmApi;
    private IViewEngine viewEngine;
    
    /// <summary>The mod entry point, called after the mod is first loaded.</summary>
    /// <param name="helper">Provides simplified APIs for writing mods.</param>
    public override void Entry(IModHelper helper)
    {
        ModHelper = helper;
        ModMonitor = Monitor;
        Manifest = ModManifest;
        
        helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        helper.Events.Input.ButtonPressed += (sender, e) =>
        {
            if (e.Button.IsActionButton())
            {
                ModMonitor.Log($"Action button pressed at tile {e.Cursor.GrabTile}", LogLevel.Debug);
                ModMonitor.Log($"Location type: {Game1.currentLocation.GetType().Name}", LogLevel.Debug);
                ModMonitor.Log($"Tile: {e.Cursor.GrabTile}", LogLevel.Debug);
            }
            if (e.Button == SButton.K)
            {
                var letterBackground = new Sprite(Game1.content.Load<Texture2D>("LooseSprites\\letterBG"), new Rectangle(0, 0, 320, 180));
                var viewModel = new MailComposeViewModel(letterBackground);
                Game1.activeClickableMenu = MailComposeMenu.Create(viewModel);
            }
        };
        
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        MfmApi = Helper.ModRegistry.GetApi<IMailFrameworkModApi>("DIGUS.MailFrameworkMod");
        viewEngine = Helper.ModRegistry.GetApi<IViewEngine>("focustense.StardewUI");
        ModMonitor.Log($"MFM Api: {(MfmApi == null ? "NULL" : "found")}", LogLevel.Debug);
        var harmony = new Harmony("meeelle.TalkByMailMod");
        // harmony.Patch(
        //     original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.checkAction)),
        //     prefix: new HarmonyMethod(typeof(MailOverrides), nameof(MailOverrides.checkAction))
        // );
        harmony.Patch(
            original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.mailbox)),
            prefix: new HarmonyMethod(typeof(MailOverrides), nameof(MailOverrides.MailboxPrefix))
        );
        
        ModMonitor.Log("Harmony patch applied.", LogLevel.Debug);

    }
}