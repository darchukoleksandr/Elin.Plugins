using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace InvulnerableCompanions;

public static class PluginConfig
{
	public static ConfigEntry<int> DebuffPower;
}

[BepInPlugin("invulnerableCompanions", "InvulnerableCompanions", "1.0.0.0")]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

	private ConfigFile configFile;
	
	private void Awake()
    {
        try {
            Logger = base.Logger;
            Logger.LogInfo("Plugin [InvulnerableCompanions] is loading!");
            Logger.LogInfo("Applying patches for [InvulnerableCompanions] plugin.");
			InitializeConfig();
			Logger.LogInfo("Applying patches for [InvulnerableCompanions] plugin.");
			Harmony.CreateAndPatchAll(typeof(Patches));
			Logger.LogInfo("Successfully applied patches for [InvulnerableCompanions] plugin.");
			Logger.LogInfo("Successfully applied patches for [InvulnerableCompanions] plugin.");
            Logger.LogInfo("Plugin [InvulnerableCompanions] is loaded!");
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
		Logger.LogInfo("Generating configuration for <InvulnerableCompanions> plugin...");
		configFile = new ConfigFile(Path.Combine(Paths.ConfigPath, "InvulnerableCompanions.cfg"), true);
		PluginConfig.DebuffPower = InitializeConfig<int>(configFile, "DebuffPower", "Unconscious debuff power.", 200);
		Logger.LogInfo("Successfully generated configuration for <InvulnerableCompanions> plugin.");
	}
}
