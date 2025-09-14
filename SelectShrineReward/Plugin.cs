using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;

namespace SelectShrineReward;

[BepInPlugin("selectShrineReward", "SelectShrineReward", "1.0.0.0")]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    private void Awake()
    {
        try {
            Logger = base.Logger;
            Logger.LogInfo("Plugin [SelectShrineReward] is loading!");
            Logger.LogInfo("Applying patches for [SelectShrineReward] plugin.");
            Harmony.CreateAndPatchAll(typeof(Patches));
            Logger.LogInfo("Successfully applied patches for [SelectShrineReward] plugin.");
            Logger.LogInfo("Plugin [SelectShrineReward] is loaded!");
        } catch (Exception ex) {
            Logger.LogError(ex.Message + Environment.NewLine + ex.StackTrace);
        }
    }
}
