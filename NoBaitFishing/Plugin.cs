using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace NoBaitFishing;

[BepInPlugin("noBaitFishing", "NoBaitFishing", "1.0.0.0")]
public class Plugin : BaseUnityPlugin
{
	internal static new ManualLogSource Logger;

	private ConfigFile configFile;

	private void Awake() {
		try {
			Logger = base.Logger;
			Logger.LogInfo("Plugin [NoBaitFishing] is loading!");
			Logger.LogInfo("Applying patches for [NoBaitFishing] plugin.");
			Harmony.CreateAndPatchAll(typeof(Patches));
			Logger.LogInfo("Successfully applied patches for [NoBaitFishing] plugin.");
			Logger.LogInfo("Plugin [NoBaitFishing] is loaded!");
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
}
