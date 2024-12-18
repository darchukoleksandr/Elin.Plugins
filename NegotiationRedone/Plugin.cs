using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.IO;

namespace NegotiationRedone;

public static class PluginConfig
{
	public static ConfigEntry<bool> CheckSuccess;

	public static ConfigEntry<int> MinAffinity;

	public static ConfigEntry<int> MaxAffinity;
}

[BepInPlugin("negotiation.redone", "Negotiation redone", "1.0.0")]
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
		Logger.LogInfo("Applying patches for <Negotiation redone> plugin.");
		new Harmony("negotiation.redone").PatchAll();
		Logger.LogInfo("Successfully applied patches for <Negotiation redone> plugin.");
	}

	private void GenerateConf()
	{
		customConfig = new ConfigFile(Path.Combine(Paths.ConfigPath, "negotiation.redone.cfg"), true);
	}

	private ConfigEntry<T> InitializeConfig<T>(string paramName, string description, T defaultValue)
	{
		ConfigFile configFile = this.customConfig;
		return configFile.Bind<T>("config", paramName, defaultValue, new ConfigDescription(description));
	}

	private void InitializeConfig()
	{
		Logger.LogInfo("Generating configuration for <Negotiation redone> plugin...");
		string successDesc = "Enable successful check in negotiations.\n" +
			"If disabled every \"Lets talk\" will be successful.\n" +
			"If enabled vanilla formula for difficulty will be used.\n" +
			"On unsuccesful attempt affinity will be not subtracted in any case.\nDefault vanilla value: true";
		PluginConfig.CheckSuccess = InitializeConfig<bool>("CheckSuccess", successDesc, true);
		string minDesc = "Mininim affinity value added on successful negotiation.\nDefault vanilla value: 1";
		PluginConfig.MinAffinity = InitializeConfig<int>("MinAffinity", minDesc, 1);
		string maxDesc = "Maximum affinity value added on successful negotiation.\nDefault vanilla value: 4";
		PluginConfig.MaxAffinity = InitializeConfig<int>("MaxAffinity", maxDesc, 4);
		Logger.LogInfo("Successfully generated configuration for <Negotiation redone> plugin.");
	}
}