using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AmmoOptions;

public static class PluginConfig
{
    public static ConfigEntry<bool> IsModEnabled;

    public static ConfigEntry<bool> InfiniteAmmoForPc;

    public static ConfigEntry<bool> InfiniteAmmoForParty;

    public static ConfigEntry<bool> InfiniteAmmoForFaction;
}

[BepInPlugin("ammoOptions", "AmmoOptions", "1.0.0.0")]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    private ConfigFile configFile;

    private void Awake()
    {
        try {
            Logger = base.Logger;
            Logger.LogInfo("Plugin [AmmoOptions] is loading!");
            InitializeConfig();
			if (PluginConfig.IsModEnabled.Value) {
				Logger.LogInfo("Applying patches for [AmmoOptions] plugin.");
				Harmony.CreateAndPatchAll(typeof(Patches));
				Logger.LogInfo("Successfully applied patches for [AmmoOptions] plugin.");
			} else {
				Logger.LogInfo("Mod is disabled, patches are not applied.");
			}
            Logger.LogInfo("Plugin [AmmoOptions] is loaded!");
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

    private ConfigEntry<T> InitializeConfig<T>(ConfigFile config, string paramName, string description, T defaultValue)
    {
        return config.Bind<T>("config", paramName, defaultValue, new ConfigDescription(description));
    }

    private void InitializeConfig()
    {
        Logger.LogInfo("Generating configuration for <AmmoOptions> plugin...");
        configFile = new ConfigFile(Path.Combine(Paths.ConfigPath, "AmmoOptions.cfg"), true);
		PluginConfig.IsModEnabled = InitializeConfig<bool>(configFile, "IsModEnabled", "Is mod enabled.\nDefault value: false", false);
		PluginConfig.InfiniteAmmoForPc = InitializeConfig<bool>(configFile, "InfiniteAmmoForPc", "Infinite ammo for player.\nDefault value: false", false);
        PluginConfig.InfiniteAmmoForParty = InitializeConfig<bool>(configFile, "InfiniteAmmoForParty", "Infinite ammo for party.\nDefault value: false", false);
        PluginConfig.InfiniteAmmoForFaction = InitializeConfig<bool>(configFile, "InfiniteAmmoForFaction", "Infinite ammo for faction.\nDefault value: false", false);
        Logger.LogInfo("Successfully generated configuration for <AmmoOptions> plugin.");
    }
}
