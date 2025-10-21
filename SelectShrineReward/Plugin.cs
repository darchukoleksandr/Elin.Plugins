using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SelectShrineReward;

public static class PluginConfig
{
	public static ConfigEntry<bool> CheckPartyMembers;
	public static ConfigEntry<bool> CheckLandsMembers;
}

[BepInPlugin("selectShrineReward", "SelectShrineReward", "1.0.0.0")]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

	private ConfigFile configFile;
	
	private void Awake()
    {
        try {
            Logger = base.Logger;
            Logger.LogInfo("Plugin [SelectShrineReward] is loading!");
			InitializeConfig();
			Logger.LogInfo("Applying patches for [SelectShrineReward] plugin.");
			Harmony.CreateAndPatchAll(typeof(Patches));
			Logger.LogInfo("Successfully applied patches for [SelectShrineReward] plugin.");
			Logger.LogInfo("Plugin [SelectShrineReward] is loaded!");
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
		Logger.LogInfo("Generating configuration for <SelectShrineReward> plugin...");
		configFile = new ConfigFile(Path.Combine(Paths.ConfigPath, "SelectShrineReward.cfg"), true);
		PluginConfig.CheckPartyMembers = InitializeConfig<bool>(configFile, "CheckPartyMembers", "Check party members skills when selecting skill book reward.", true);
		PluginConfig.CheckLandsMembers = InitializeConfig<bool>(configFile, "CheckLandsMembers", "Check home zone members skills when selecting skill book reward.", true);
		Logger.LogInfo("Successfully generated configuration for <SelectShrineReward> plugin.");
	}
}
