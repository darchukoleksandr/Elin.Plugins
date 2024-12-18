using BepInEx;
using HarmonyLib;

namespace FullCodexBrowser;

[BepInPlugin("full.codex.browser", "Full codex browser", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
	internal static Plugin Instance { get; private set; }

	private void Awake()
	{
		Instance = this;
		ApplyPatches();
	}

	internal static void Log(object payload)
	{
		Instance!.Logger.LogInfo(payload);
	}

	private void ApplyPatches()
	{
		Logger.LogInfo("Applying patches for <Full codex browser> plugin.");
		new Harmony("full.codex.browser").PatchAll();
		Logger.LogInfo("Successfully applied patches for <Full codex browser> plugin.");
	}
}