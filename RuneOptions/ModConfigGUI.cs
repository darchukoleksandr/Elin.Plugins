using System.Linq;
using ModConfigGUI.UI;
using BepInEx;
using BepInEx.Configuration;

namespace RuneOptions;

internal class ModConfigGUI
{
    public static void RegisterModConfigGUI(ConfigFile configFile) {
        if (FindModConfigGUIPlugin() != null) {
            LayerBuilder.RegisterDefaultBuilder("runeOptions", "RuneOptions", configFile);
        }
    }

    private static BaseUnityPlugin FindModConfigGUIPlugin()
    {
        return ModManager.ListPluginObject.OfType<BaseUnityPlugin>().FirstOrDefault((BaseUnityPlugin p) => p.Info.Metadata.GUID == "me.xtracr.modconfiggui");
    }
}
