using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using FastTravelRedone.Patches;
using HarmonyLib;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace FastTravelRedone;

public static class PluginConfig
{
	public static ConfigEntry<bool> IgnoreIsKnown;

	public static ConfigEntry<bool> IgnoreCanFastTravel;

	public static ConfigEntry<bool> IgnoreIsClosed;

	public static ConfigEntry<bool> SortDungeonToBottom;
}

[BepInPlugin("fast.travel.redone", "Fast travel redone", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
	private ConfigFile customConfig;

	internal static new ManualLogSource Logger;

	internal static Plugin Instance { get; private set; }

	private void Awake()
	{
		try {
			Logger = base.Logger;
			GenerateConf();
			ApplyPatches();
			InitializeConfig();
		} catch (Exception ex) {
			Logger.LogError(ex.Message + Environment.NewLine + ex.StackTrace);
		}
	}

	private void Start() {
		try {
			if (AppDomain.CurrentDomain.GetAssemblies().Any((Assembly a) => a.GetName().Name == "ModConfigGUI")) {
				ModConfigGUI.RegisterModConfigGUI(customConfig);
			} else {
				Logger.LogInfo("ModConfigGUI not found - skipping GUI initialization");
			}
		} catch (Exception ex) {
			Logger.LogError("Failed to initialize ModConfigGUI: " + ex.Message);
		}
	}

	private void ApplyPatches()
	{
		Logger.LogInfo("Applying patches for <Fast travel redone> plugin.");
		Harmony.CreateAndPatchAll(typeof(RegionListTravelZonesPatch));
		Harmony.CreateAndPatchAll(typeof(LayerTravelGetSortValPatch));
		Logger.LogInfo("Successfully applied patches for <Fast travel redone> plugin.");
	}

	private void GenerateConf()
	{
		customConfig = new ConfigFile(Path.Combine(Paths.ConfigPath, "fast.travel.redone.cfg"), true);
	}

	private ConfigEntry<T> InitializeConfig<T>(string paramName, string description, T defaultValue)
	{
		return this.customConfig.Bind<T>("config", paramName, defaultValue, new ConfigDescription(description));
	}

	private void InitializeConfig()
	{
		Logger.LogInfo("Generating configuration for <Fast travel redone> plugin...");
		string isKnownDesc = "If enabled all zones will be listed in fast travel list.";
		PluginConfig.IgnoreIsKnown = InitializeConfig<bool>("IgnoreIsKnown", isKnownDesc, false);
		string canFastTravelDesc = "If enabled fast travel to random dungeons becomes available.";
		PluginConfig.IgnoreCanFastTravel = InitializeConfig<bool>("IgnoreCanFastTravel", canFastTravelDesc, true);
		string isClosedDesc = "If enabled places that are \"Closed\" will also be listed in fast travel.";
		PluginConfig.IgnoreIsClosed = InitializeConfig<bool>("IgnoreIsClosed", isClosedDesc, false);
		string sortDungeonToBottom = "Sort dungeons to the bottom of the list.";
		PluginConfig.SortDungeonToBottom = InitializeConfig<bool>("SortDungeonToBottom", sortDungeonToBottom, false);
		Logger.LogInfo("Successfully generated configuration for <Fast travel redone> plugin.");
	}
}