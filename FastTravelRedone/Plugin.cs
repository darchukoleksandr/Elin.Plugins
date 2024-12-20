using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.IO;

namespace FastTravelRedone;

public static class PluginConfig
{
	public static ConfigEntry<bool> IgnoreIsKnown;

	public static ConfigEntry<bool> IgnoreCanFastTravel;

	public static ConfigEntry<bool> IgnoreIsClosed;
}

[BepInPlugin("fast.travel.redone", "Fast travel redone", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
	private ConfigFile customConfig;

	internal static Plugin Instance { get; private set; }

	private void Awake()
	{
		Instance = this;
		ApplyPatches();
		GenerateConf();
		InitializeConfig();
	}

	internal static void Log(object payload)
	{
		Instance!.Logger.LogInfo(payload);
	}

	private void ApplyPatches()
	{
		Logger.LogInfo("Applying patches for <Fast travel redone> plugin.");
		new Harmony("fast.travel.redone").PatchAll();
		Logger.LogInfo("Successfully applied patches for <Fast travel redone> plugin.");
	}

	private void GenerateConf()
	{
		customConfig = new ConfigFile(Path.Combine(Paths.ConfigPath, "fast.travel.redone.cfg"), true);
	}

	private ConfigEntry<T> InitializeConfig<T>(string paramName, string description, T defaultValue)
	{
		ConfigFile configFile = this.customConfig;
		return configFile.Bind<T>("config", paramName, defaultValue, new ConfigDescription(description));
	}

	private void InitializeConfig()
	{
		Logger.LogInfo("Generating configuration for <Fast travel redone> plugin...");
		string isKnownDesc = "If enabled all zones will be listed in fast travel list.\nDefault vanilla value: false";
		PluginConfig.IgnoreIsKnown = InitializeConfig<bool>("IgnoreIsKnown", isKnownDesc, false);
		string canFastTravelDesc = "If enabled fast travel to random dungeons becomes available.\nDefault vanilla value: false";
		PluginConfig.IgnoreCanFastTravel = InitializeConfig<bool>("IgnoreCanFastTravel", canFastTravelDesc, true);
		string isClosedDesc = "If enabled places that are \"Closed\" will also be listed in fast travel.\nDefault vanilla value: false";
		PluginConfig.IgnoreIsClosed = InitializeConfig<bool>("IgnoreIsClosed", isClosedDesc, false);
		Logger.LogInfo("Successfully generated configuration for <Fast travel redone> plugin.");
	}
}