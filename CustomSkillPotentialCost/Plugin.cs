using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CustomSkillPotentialCost;

public static class PluginConfig
{
    public static ConfigEntry<bool> IsModEnabled;

    public static ConfigEntry<bool> IsRelationAffectsPrice;

    public static ConfigEntry<int> LearnCost;

    public static ConfigEntry<int> TrainCost;
}

[BepInPlugin("customSkillPotentialCost", "CustomSkillPotentialCost", "1.0.0.0")]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    private ConfigFile configFile;

    private void Awake() {
        try {
            Logger = base.Logger;
            Logger.LogInfo("Plugin [CustomSkillPotentialCost] is loading!");
            InitializeConfig();
            Logger.LogInfo("Applying patches for [CustomSkillPotentialCost] plugin.");
            Harmony.CreateAndPatchAll(typeof(Patches));
            Logger.LogInfo("Successfully applied patches for [CustomSkillPotentialCost] plugin.");
            Logger.LogInfo("Plugin [CustomSkillPotentialCost] is loaded!");
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
        Logger.LogInfo("Generating configuration for <CustomSkillPotentialCost> plugin...");
        configFile = new ConfigFile(Path.Combine(Paths.ConfigPath, "CustomSkillPotentialCost.cfg"), true);
        string isEnabledDesc = "Is mod enabled.\nDefault value: false";
        PluginConfig.IsModEnabled = InitializeConfig<bool>(configFile, "IsModEnabled", isEnabledDesc, false);
        string isRelationAffectsPriceDesc = "If relation with guild should affect price (count x2 price if not at 2 rank guild member).\nDefault value: true";
        PluginConfig.IsRelationAffectsPrice = InitializeConfig<bool>(configFile, "IsRelationAffectsPrice", isRelationAffectsPriceDesc, true);
        string learnCostDesc = "Cost to learn new skill.\nDefault value: 5";
        PluginConfig.LearnCost = InitializeConfig<int>(configFile, "LearnCost", learnCostDesc, 5);
        string trainCostDesc = "Cost to train known skill.\nDefault value: 1";
        PluginConfig.TrainCost = InitializeConfig<int>(configFile, "TrainCost", trainCostDesc, 1);
        Logger.LogInfo("Successfully generated configuration for <CustomSkillPotentialCost> plugin.");
    }
}