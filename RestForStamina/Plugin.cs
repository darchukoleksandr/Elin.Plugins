using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;

namespace RestForStamina;

[BepInPlugin("restForStamina", "RestForStamina", "1.0.0.0")]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    private void Awake()
    {
        try {
            Logger = base.Logger;
            Logger.LogInfo("Plugin [RestForStamina] is loading!");
            Logger.LogInfo("Applying patches for [RestForStamina] plugin.");
            Harmony.CreateAndPatchAll(typeof(Patches));
            Logger.LogInfo("Successfully applied patches for [RestForStamina] plugin.");
            Logger.LogInfo("Plugin [RestForStamina] is loaded!");
        } catch (Exception ex) {
            Logger.LogError(ex.Message + Environment.NewLine + ex.StackTrace);
        }
    }
}
