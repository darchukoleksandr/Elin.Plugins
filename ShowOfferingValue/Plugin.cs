using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;

namespace ShowOfferingValue;

[BepInPlugin("showOfferingValue", "ShowOfferingValue", "1.0.0.0")]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

	private void Awake()
    {
        try {
            Logger = base.Logger;
            Logger.LogInfo("Plugin [ShowOfferingValue] is loading!");
            Logger.LogInfo("Applying patches for [ShowOfferingValue] plugin.");
			Harmony.CreateAndPatchAll(typeof(Patches));
			Logger.LogInfo("Successfully applied patches for [ShowOfferingValue] plugin.");
            Logger.LogInfo("Plugin [ShowOfferingValue] is loaded!");
        } catch (Exception ex) {
            Logger.LogError(ex.Message + Environment.NewLine + ex.StackTrace);
        }
	}
}
