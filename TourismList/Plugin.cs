using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace TourismList;

public static class PluginConfig
{
	public static ConfigEntry<string> SortRule;
}

[BepInPlugin("tourismList", "TourismList", "1.0.0.0")]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

	private ConfigFile configFile;
	
	private void Awake()
    {
        try {
            Logger = base.Logger;
            Logger.LogInfo("Plugin [TourismList] is loading!");
            Logger.LogInfo("Applying patches for [TourismList] plugin.");
			InitializeConfig();
			Logger.LogInfo("Applying patches for [TourismList] plugin.");
			Harmony.CreateAndPatchAll(typeof(Patches));
			Logger.LogInfo("Successfully applied patches for [TourismList] plugin.");
            Logger.LogInfo("Plugin [TourismList] is loaded!");
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
		Logger.LogInfo("Generating configuration for <TourismList> plugin...");
		configFile = new ConfigFile(Path.Combine(Paths.ConfigPath, "TourismList.cfg"), true);
		AcceptableValueList<string> sortBy = new AcceptableValueList<string>(["ByNumber", "ByPrice", "ByName"]);
		PluginConfig.SortRule = configFile.Bind<string>("config", "SortRule", "ByName", new ConfigDescription($"Sorting rule for list of tourism items.", sortBy, Array.Empty<object>()));
		Logger.LogInfo("Successfully generated configuration for <TourismList> plugin.");
	}
}
