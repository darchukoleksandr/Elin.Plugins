using BepInEx;
using HarmonyLib;

namespace GoAroundThatTree;

[BepInPlugin("go.around.that.tree", "Go around that tree", "1.0.0")]
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
		Logger.LogInfo("Applying patches for <Go around that tree> plugin.");
		new Harmony("go.around.that.tree").PatchAll();
		Logger.LogInfo("Successfully applied patches for <Go around that tree> plugin.");
	}
}