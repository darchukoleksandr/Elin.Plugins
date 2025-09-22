using System;
using HarmonyLib;
using static AI_Fish;

namespace NoBaitFishing
{
	public class Patches
	{
		[HarmonyPrefix]
		[HarmonyPatch(typeof(ProgressFish), nameof(ProgressFish.OnProgressComplete))]
		public static bool Prefix(ref ProgressFish __instance)
		{
			if (__instance.owner.IsPC) {
				Plugin.Logger.LogInfo("Current " + EClass.player.eqBait.Num);
				EClass.player.eqBait.ModNum(1);
			}
			return true;
		}
	}
}
