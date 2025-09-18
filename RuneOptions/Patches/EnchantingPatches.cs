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
		if (__result) {
			return;
		}
		SourceElement.Row source = mod.source;
		if (source.category == "resist" && PluginConfig.AllowMoreResistEnchantment.Value) {
			bool hasSameResist = false;
			foreach (Element item in __instance.elements.ListElements()) {
				if (item.id == source.id) {
					hasSameResist = true;
					break;
				}
			}
			if (!__result) {
				__result = !hasSameResist;
			}
		}
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