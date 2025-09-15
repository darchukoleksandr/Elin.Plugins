using System;
using System.Collections.Generic;
using HarmonyLib;

namespace RuneOptions;

public class DisenchantingPatches
{
	private static Thing LastDisenchantedItem;

	/// <summary>
	/// Change earch rune disenchanting rarity.
	/// </summary>
	[HarmonyPrefix]
	[HarmonyPatch(typeof(TraitRuneMoldEarth), nameof(TraitRuneMoldEarth.MaxRarity), MethodType.Getter)]
	public static bool TraitRuneMoldEarthMaxRarity(ref TraitRuneMoldEarth __instance, ref Rarity __result) {
		__result = ClassExtension.ToEnum<Rarity>(PluginConfig.EarthRuneMaxRarity.Value, true);
		return false;
	}

	/// <summary>
	/// Change sun rune disenchanting rarity.
	/// </summary>
	[HarmonyPrefix]
	[HarmonyPatch(typeof(TraitRuneMoldSun), nameof(TraitRuneMoldSun.MaxRarity), MethodType.Getter)]
	public static bool TraitRuneMoldSunMaxRarity(ref TraitRuneMoldSun __instance, ref Rarity __result) {
		__result = ClassExtension.ToEnum<Rarity>(PluginConfig.SunRuneMaxRarity.Value, true);
		return false;
	}

	/// <summary>
	/// Change mana rune disenchanting rarity.
	/// </summary>
	[HarmonyPrefix]
	[HarmonyPatch(typeof(TraitRuneMoldMana), nameof(TraitRuneMoldMana.MaxRarity), MethodType.Getter)]
	public static bool TraitRuneMoldManaMaxRarity(ref TraitRuneMoldMana __instance, ref Rarity __result) {
		__result = ClassExtension.ToEnum<Rarity>(PluginConfig.ManaRuneMaxRarity.Value, true);
		return false;
	}

	/// <summary>
	/// Allow disenchanting canes.
	/// </summary>
	[HarmonyPostfix]
	[HarmonyPatch(typeof(TraitCrafter), nameof(TraitCrafter.IsIngredient), [typeof(int), typeof(SourceRecipe.Row), typeof(Card)])]
	public static void IsIngredientPostfix(ref TraitCrafter __instance, int idx, SourceRecipe.Row r, Card c, ref bool __result) {
		if (__result || __instance is not TraitRuneMold instance || !PluginConfig.AllowMoreItemDisenchantment.Value) {
			return;
		}
		if (new[] { "cane", "guns", "bow" }.Contains(c.category.id)) {
			__result = c.rarity <= instance.MaxRarity && !c.c_isImportant;
		}
	}

	/// <summary>
	/// After pick up any item. Check if it is a enchanted rune and compare enchantment with <see cref="LastDisenchantedItem"/>.
	/// Creating a dublicate from item that will be destroyed and placing it in inventory.
	/// </summary>
	[HarmonyPostfix]
	[HarmonyPatch(typeof(Chara), nameof(Chara.Pick))]
	public static void Pick(ref Chara __instance, Thing t, bool msg = true, bool tryStack = true) {
		try {
			if (t.source.id == "rune" && t.refVal != 0 && t.encLV != 0 &&
				!LastDisenchantedItem.Equals(default(KeyValuePair<Thing, Thing>))) {

				var enchantment = LastDisenchantedItem.elements.dict.TryGetValue(t.refVal);
				if (enchantment == null || t.encLV != enchantment.Value) { // enchantment or level is different
					Plugin.Logger.LogInfo("Rune enchantments is not found or different from last disenchanted item. No item will be created");
					LastDisenchantedItem = null; // something went wrong
					return;
				}
				LastDisenchantedItem.elements.Remove(enchantment.id);
				Thing dublicate = LastDisenchantedItem.Duplicate(1);
				EClass.pc.Pick(dublicate, false);
				LastDisenchantedItem = default;
			}
		} catch (Exception ex) {
			Plugin.Logger.LogError(ex.Message + Environment.NewLine + ex.StackTrace);
		}
	}

	/// <summary>
	/// Before applying empty rune on item to disenchant.
	/// Changing rune material hardness to remove disenchant hardness restriction from rune.
	/// </summary>
	/// <returns></returns>
	[HarmonyPrefix]
	[HarmonyPatch(typeof(TraitCrafter), nameof(TraitCrafter.Craft))]
	public static bool CraftPrefix(ref TraitCrafter __instance, ref AI_UseCrafter ai) {
		if (!PluginConfig.IgnoreRuneMaterialHardness.Value) {
			return true;
		}

		if (ClassExtension.ToEnum<TraitCrafter.MixType>(__instance.GetSource(ai).type, true) == TraitCrafter.MixType.RuneMold) {
			Thing thing = ai.ings[0];
			__instance.owner.material.hardness = thing.material.hardness + 1;
		}
		return true;
	}

	/// <summary>
	/// After applying empty rune on item to disenchant. But before selecting ecnhantment from list.
	/// Save source and dublicate item to <see cref="LastDisenchantedItem"/> to further remove enchant from dublicate item in player inventory.
	/// </summary>
	[HarmonyPostfix]
	[HarmonyPatch(typeof(TraitCrafter), nameof(TraitCrafter.Craft))]
	public static void CraftPostfix(ref TraitCrafter __instance, AI_UseCrafter ai) {
		try {
			if (ClassExtension.ToEnum<TraitCrafter.MixType>(__instance.GetSource(ai).type, true) == TraitCrafter.MixType.RuneMold) {
				LastDisenchantedItem = ai.ings[0];
			}
		} catch (Exception ex) {
			Plugin.Logger.LogError(ex.Message + Environment.NewLine + ex.StackTrace);
		}
	}
}
