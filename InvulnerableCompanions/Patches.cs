using System;
using HarmonyLib;

namespace InvulnerableCompanions;

public class Patches
{
	[HarmonyPrefix]
	[HarmonyPatch(typeof(Chara), nameof(Chara.Die))]
	public static bool Prefix(Chara __instance) {
		if (__instance.IsPCParty) {
			__instance.hp = 0;
			if (!__instance.Chara.HasCondition<ConFaint>()) {
				__instance.Chara.AddCondition<ConFaint>(PluginConfig.DebuffPower.Value);
			}
			return false;
		}
		return true;
	}
}
