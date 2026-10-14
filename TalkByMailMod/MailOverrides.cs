using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using xTile.Dimensions;

namespace TalkByMailMod;


public class MailOverrides
{
    public static bool MailboxPrefix()
    {
        ModEntry.ModMonitor.Log("mailbox opened!", LogLevel.Debug);
        if (Game1.player.mailbox.Count > 0)
        {
            ModEntry.ModMonitor.Log("mailbox has letters in it", LogLevel.Debug);
            return true;
        }

        Game1.activeClickableMenu = new MailCreateMenu();
        // skip the rest of the mail method
        return false;
    }
    // public static bool checkAction(GameLocation __instance, xTile.Dimensions.Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
    // {
    //     var layers = new[] { "Back", "Buildings", "Front", "AlwaysFront" };
    //     foreach (var layerName in layers)
    //     {
    //         var layer = __instance.map.GetLayer(layerName);
    //         if (layer == null) continue;
    //         var tile = layer.Tiles[tileLocation.X, tileLocation.Y];
    //         if (tile == null) continue;
    //     
    //         ModEntry.ModMonitor.Log($"Layer {layerName} tile index: {tile.TileIndex}", LogLevel.Debug);
    //         foreach (var prop in tile.Properties)
    //             ModEntry.ModMonitor.Log($"  prop: {prop.Key} = {prop.Value}", LogLevel.Debug);
    //     }
    //
    //     return true;
    // }
    // public static bool checkAction(GameLocation __instance, xTile.Dimensions.Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
    // {
    //     var backLayer = __instance.map.GetLayer("Back");
    //     if (backLayer != null)
    //     {
    //         var tile = backLayer.Tiles[tileLocation.X, tileLocation.Y];
    //         if (tile != null && tile.TileIndex == 256)
    //         {
    //             ModEntry.ModMonitor.Log("Mailbox tile detected!", LogLevel.Debug);
    //             if (Game1.player.mailbox.Count > 0)
    //             {
    //                 // return true; // true = run the original method
    //                 ModEntry.ModMonitor.Log("help0", LogLevel.Debug);
    //                 // __runOriginal = true;
    //                 return true;
    //             }
    //
    //             if (Game1.activeClickableMenu != null)
    //                 return true; // something already opened, let it be
    //         
    //             if (!Context.IsWorldReady || !Context.IsPlayerFree)
    //                 return true;
    //
    //             Game1.activeClickableMenu = new MailCreateMenu();
    //             return false; // skip original
    //         }
    //     }
    //
    //     return true; // not mailbox, run original
    // }
    
}