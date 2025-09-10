using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DisablePotentialDecrease;

public static class PluginConfig
{
    public static ConfigEntry<bool> IsModEnabled;

    public static ConfigEntry<bool> IsEnabledForSkills;

    public static ConfigEntry<bool> IsEnabledForAttributes;
}

[BepInPlugin("disablePotentialDecrease", "DisablePotentialDecrease", "1.0.0.0")]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    private ConfigFile configFile;

    private void Awake()
    {
        try {
            Logger = base.Logger;
            Logger.LogInfo("Plugin [DisablePotentialDecrease] is loading!");
            InitializeConfig();
            Logger.LogInfo("Applying patches for [DisablePotentialDecrease] plugin.");
            Harmony.CreateAndPatchAll(typeof(Patches));
            Logger.LogInfo("Successfully applied patches for [DisablePotentialDecrease] plugin.");
            Logger.LogInfo("Plugin [DisablePotentialDecrease] is loaded!");
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
        Logger.LogInfo("Generating configuration for <DisablePotentialDecrease> plugin...");
        configFile = new ConfigFile(Path.Combine(Paths.ConfigPath, "disablePotentialDecrease.cfg"), true);
        string isEnabledDesc = "Is mod enabled.\nDefault value: false";
        PluginConfig.IsModEnabled = InitializeConfig<bool>(configFile, "IsModEnabled", isEnabledDesc, false);
        string isEnabledForSkillsDesc = "If enabled for skills (Gathering, Carpentery etc).\nDefault value: false";
        PluginConfig.IsEnabledForSkills = InitializeConfig<bool>(configFile, "IsEnabledForSkills", isEnabledForSkillsDesc, false);
        string isEnabledForAttributesDesc = "If enabled for attributes (Endurance, Charisma etc).\nDefault value: false";
        PluginConfig.IsEnabledForAttributes = InitializeConfig<bool>(configFile, "IsEnabledForAttributes", isEnabledForAttributesDesc, false);
        Logger.LogInfo("Successfully generated configuration for <DisablePotentialDecrease> plugin.");
    }
}
