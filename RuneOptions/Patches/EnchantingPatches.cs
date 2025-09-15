using System;
using HarmonyLib;

namespace RuneOptions;

public class EnchantingPatches
{
	/// <summary>
	/// Allow enchant more than 1 resist enchant
	/// </summary>
	[HarmonyPostfix]
	[HarmonyPatch(typeof(Card), nameof(Card.CanAddRune))]
	public static void CanAddRune(ref Card __instance, TraitMod mod, ref bool __result) {
		SourceElement.Row source = mod.source;
		//Plugin.Logger.LogInfo($"Card.CanAddRune name {__instance.Name} category.slot {__instance.category.slot} HasEnc {__instance.material.HasEnc(source.id)}");
		//SourceElement.Row source = mod.source;
		//if (__instance.material.HasEnc(source.id)) {
		//	Plugin.Logger.LogInfo("source.id HasEnc " + source.id);
		//	return false;
		//}
		if (__result) {
			return;
		}

		//if (!__instance.IsWeapon && source.IsWeaponEnc) {
		//	return;
		//}

		//Plugin.Logger.LogInfo($"item {__instance.Name} source.category {source.category} enchantment {source.id} {source.name}");
		if (source.category == "resist" && PluginConfig.AllowMoreResistEnchantment.Value) {
			bool hasSameResist = false;
			foreach (Element item in __instance.elements.ListElements()) {
				//Plugin.Logger.LogInfo($"enchantment {item.id} {item.source.name} {source.id}");
				if (item.id == source.id) {
					hasSameResist = true;
					break;
				}
			}
			if (!__result) {
				__result = !hasSameResist;
			}
		}

		//var category = __instance.category;
		//if (category.IsChildOf("weapon") || category.IsChildOf("armor") || category.IsChildOf("ranged")) {
		//	__result = true;
		//	return false;
		//}
	}


	/// <summary>
	/// Create empty rune after enchanting if reusable runes are enabled.
	/// </summary>
	[HarmonyPostfix]
	[HarmonyPatch(typeof(Card), nameof(Card.AddRune), argumentTypes: [typeof(Card)])]
	public static void AddRune(ref Card __instance, Card rune) {
		if (!PluginConfig.EnableReusableRunes.Value) {
			return;
		}
		Thing emptyRune = ThingGen.Create("rune_mold_earth");
		emptyRune.ChangeMaterial(rune.material);
		EClass.pc.Pick(emptyRune);
	}

	/// <summary>
	/// Changing the max amount of runes can be applied on item.
	/// </summary>
	[HarmonyPostfix]
	[HarmonyPatch(typeof(Card), nameof(Card.MaxRune))]
	public static void MaxRune(ref Card __instance, ref int __result) {
		__result = PluginConfig.MaxRuneAmount.Value;
	}
}