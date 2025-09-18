using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace BetterReturn;

public static class PluginConfig
{
	public static ConfigEntry<bool> IsModEnabled;

	public static ConfigEntry<bool> AllowCities;

	public static ConfigEntry<bool> OnlyVisited;
}

[BepInPlugin("betterReturn", "BetterReturn", "1.0.0.0")]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

	private ConfigFile configFile;
	
	private void Awake()
    {
        try {
            Logger = base.Logger;
            Logger.LogInfo("Plugin [BetterReturn] is loading!");
            Logger.LogInfo("Applying patches for [BetterReturn] plugin.");
			InitializeConfig();
			if (PluginConfig.IsModEnabled.Value) {
				Logger.LogInfo("Applying patches for [BetterReturn] plugin.");
				Harmony.CreateAndPatchAll(typeof(Patches));
				Logger.LogInfo("Successfully applied patches for [BetterReturn] plugin.");
			} else {
				Logger.LogInfo("Mod is disabled, patches are not applied.");
			}
			Logger.LogInfo("Successfully applied patches for [BetterReturn] plugin.");
            Logger.LogInfo("Plugin [BetterReturn] is loaded!");
        } catch (Exception ex) {
            Logger.LogError(ex.Message + Environment.NewLine + ex.StackTrace);
        }
	}

	private void Start() {
		try {
			if (AppDomain.CurrentDomain.GetAssemblies().Any((Assembly a) => a.GetName().Name == "ModConfigGUI")) {
				ModConfigGUI.RegisterModConfigGUI(configFile);
			} else {
				Logger.LogInfo("ModConfigGUI not found - skipping GUI initialization");
			}
		} catch (Exception ex) {
			Logger.LogError("Failed to initialize ModConfigGUI: " + ex.Message);
		}
	}

	private ConfigEntry<T> InitializeConfig<T>(ConfigFile config, string paramName, string description, T defaultValue) {
		return config.Bind<T>("config", paramName, defaultValue, new ConfigDescription(description));
	}

	private void InitializeConfig() {
		Logger.LogInfo("Generating configuration for <BetterReturn> plugin...");
		configFile = new ConfigFile(Path.Combine(Paths.ConfigPath, "BetterReturn.cfg"), true);
		PluginConfig.IsModEnabled = InitializeConfig<bool>(configFile, "IsModEnabled", "Is mod enabled.", false);
		PluginConfig.AllowCities = InitializeConfig<bool>(configFile, "AllowCities", "Cities will be listed in locations list.", false);
		PluginConfig.OnlyVisited = InitializeConfig<bool>(configFile, "OnlyVisited", "Only visited will be in the list.", true);
		Logger.LogInfo("Successfully generated configuration for <BetterReturn> plugin.");
	}
}
