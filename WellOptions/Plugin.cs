using System;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace WellOptions
{
	public static class ModConfig
	{
		public static ConfigEntry<bool> IsNegativeEffectsDisabled;
		public static ConfigEntry<bool> IsMutationsDisabled;
		public static ConfigEntry<bool> IsAlwaysPositiveEffectEnabled;
	}

	[BepInPlugin("well.options", "Well options", "1.0.0")]
	public class Plugin : BaseUnityPlugin
	{
		internal static new ManualLogSource Logger;

		private ConfigFile customConfig;

		private void Awake()
		{
			try {
				ApplyPatches();
				GenerateConf();
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
			try {
				Logger = base.Logger;
				Logger.LogInfo((object)"Applying patches for <Well options> plugin.");
				Harmony.CreateAndPatchAll(typeof(TraitWell_TrySetAct));
				//TraitWell_BadEffect.DoPatching();
				Logger.LogInfo((object)"Successfully applied patches for <Well options> plugin.");
            } catch (Exception ex) {
                Logger.LogError(ex.Message + "\r\n" + ex.StackTrace);
            }
		}

		private void GenerateConf()
		{
			customConfig = new ConfigFile(Path.Combine(Paths.ConfigPath, "well.options.cfg"), true);
		}

		private void InitializeConfig()
		{
			Logger.LogInfo((object)"Generating configuration for <Well options> plugin..."); 
            ModConfig.IsNegativeEffectsDisabled = customConfig.Bind<bool>("config", nameof(ModConfig.IsNegativeEffectsDisabled), false, new ConfigDescription("Is sleed and negative attributes potential disabled"));
            ModConfig.IsMutationsDisabled = customConfig.Bind<bool>("config", nameof(ModConfig.IsMutationsDisabled), false, new ConfigDescription("Is mutations disabled"));
            ModConfig.IsAlwaysPositiveEffectEnabled = customConfig.Bind<bool>("config", nameof(ModConfig.IsAlwaysPositiveEffectEnabled), false, new ConfigDescription("If enabled you will always gain positive attributes potential"));
			Logger.LogInfo((object)"Successfully generated configuration for <Well options> plugin.");
		}
	}
}
