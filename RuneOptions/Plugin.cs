using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace RuneOptions;

public static class PluginConfig
{
    public static ConfigEntry<bool> IsModEnabled;

    public static ConfigEntry<bool> IgnoreRuneMaterialHardness;

	public static ConfigEntry<bool> EnableReusableRunes;

	public static ConfigEntry<bool> AllowMoreItemDisenchantment;

	public static ConfigEntry<bool> AllowMoreResistEnchantment;

	public static ConfigEntry<int> MaxRuneAmount;

	public static ConfigEntry<string> EarthRuneMaxRarity;

	public static ConfigEntry<string> SunRuneMaxRarity;

	public static ConfigEntry<string> ManaRuneMaxRarity;
}

[BepInPlugin("runeOptions", "RuneOptions", "1.0.0.0")]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    private ConfigFile configFile;

    private void Awake()
    {
        try {
            Logger = base.Logger;
            Logger.LogInfo("Plugin [RuneOptions] is loading!");
            InitializeConfig();
			if (PluginConfig.IsModEnabled.Value) {
				Logger.LogInfo("Applying patches for [RuneOptions] plugin.");
				Harmony.CreateAndPatchAll(typeof(EnchantingPatches));
				Harmony.CreateAndPatchAll(typeof(DisenchantingPatches));
				Logger.LogInfo("Successfully applied patches for [RuneOptions] plugin.");
			} else {
				Logger.LogInfo("Mod is disabled, patches are not applied.");
			}
            Logger.LogInfo("Plugin [RuneOptions] is loaded!");
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
        return config.Bind<T>("Config", paramName, defaultValue, new ConfigDescription(description));
    }

    private void InitializeConfig()
    {
        Logger.LogInfo("Generating configuration for <RuneOptions> plugin...");

        configFile = new ConfigFile(Path.Combine(Paths.ConfigPath, "RuneOptions.cfg"), true);
        PluginConfig.IsModEnabled = InitializeConfig<bool>(configFile, "IsModEnabled", "Is mod enabled.", false);
        PluginConfig.IgnoreRuneMaterialHardness = InitializeConfig<bool>(configFile, "IgnoreRuneMaterialHardness", "If enabled when disenchanting item, the hardness requirement of rune is ignored.", false);
        PluginConfig.EnableReusableRunes = InitializeConfig<bool>(configFile, "EnableReusableRunes", "After enchanting item you will get empty rune back. Material of enchanted rune is not saved so you will receive a Earth rune.", false);
        PluginConfig.MaxRuneAmount = InitializeConfig<int>(configFile, "MaxRuneAmount", "Maximum amount of runes that can be enchanted.", 1);
		PluginConfig.AllowMoreItemDisenchantment = InitializeConfig<bool>(configFile, "AllowMoreItemDisenchantment", "Allow to disenchant previously unallowed (canes, bows, guns). Base game allows to disenchant only armour and melee weapons.", false);
		PluginConfig.AllowMoreResistEnchantment = InitializeConfig<bool>(configFile, "AllowMoreResistEnchantment", "Allow to enchant additional resists. Base game only allows 1 resits per item.", false);

		AcceptableValueList<string> runeRarities = new AcceptableValueList<string>(["Superior", "Legendary", "Mythical", "Artifact"]);
		PluginConfig.EarthRuneMaxRarity = configFile.Bind<string>(new ConfigDefinition("Rune rarity", "Earth rune"), "Superior", new ConfigDescription($"Earth rune. Affect a maximum rarity of the item that can be disenchanted.", runeRarities, Array.Empty<object>()));
		PluginConfig.SunRuneMaxRarity = configFile.Bind<string>(new ConfigDefinition("Rune rarity", "Sun rune"), "Legendary", new ConfigDescription($"Earth rune. Affect a maximum rarity of the item that can be disenchanted.", runeRarities, Array.Empty<object>()));
		PluginConfig.ManaRuneMaxRarity = configFile.Bind<string>(new ConfigDefinition("Rune rarity", "Mana rune"), "Mythical", new ConfigDescription($"Mana rune. Affect a maximum rarity of the item that can be disenchanted.", runeRarities, Array.Empty<object>()));
		
		Logger.LogInfo("Successfully generated configuration for <RuneOptions> plugin.");
    }
}
