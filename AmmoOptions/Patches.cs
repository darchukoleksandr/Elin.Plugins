using System;
using HarmonyLib;

namespace AmmoOptions;

public class Patches
{
	[HarmonyPrefix]
	[HarmonyPatch(typeof(ActRanged), nameof(ActRanged.Perform))]
	public static bool Perform()
	{
		Thing ranged = Act.CC.ranged;
		if (ranged.trait is not TraitToolRange toolRange) {
			return true;
		}
		if (Act.CC.IsPC && PluginConfig.InfiniteAmmoForPc.Value) {
			ranged.c_ammo = toolRange.MaxAmmo;
		} else if (Act.CC.IsPCParty && PluginConfig.InfiniteAmmoForParty.Value) {
			ranged.c_ammo = toolRange.MaxAmmo;
		} else if (Act.CC.IsPCFaction && PluginConfig.InfiniteAmmoForFaction.Value) {
			ranged.c_ammo = toolRange.MaxAmmo;
		}
		return true;
	}
}
