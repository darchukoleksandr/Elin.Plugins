using System;
using System.Linq;
using System.Collections.Generic;
using HarmonyLib;

namespace BetterReturn;

public class Patches
{
	[HarmonyPrefix]
    [HarmonyPatch(typeof(SpatialManager), nameof(SpatialManager.ListReturnLocations))]
    public static bool Prefix(SpatialManager __instance, ref List<Zone> __result)
    {
        try {
			if (EClass.debug.returnAnywhere) { // default game logic
				return true;
			} else {
				__result = __instance.map.Values
					.Where((Spatial a) => {
						if (a is not Zone zone || zone == EClass._zone) {
							return false;
						}
						if (zone.IsPCFaction || EClass.pc.homeZone == zone) {
							return true;
						}
						Zone topZone = zone.GetTopZone();
						if (PluginConfig.AllowCities.Value && zone is Zone_Civilized city) {
							if (topZone != null && topZone.uid != city.uid) { // do not display basements and undergrounds of cities
								return false;
							}
							return PluginConfig.OnlyVisited.Value ? zone.GetTopZone().visitCount > 0 : true;
						}
						if (zone is Zone_DungeonPuppy && EClass.game.quests.GetPhase<QuestPuppy>() == Quest.PhaseComplete) {
							return !PluginConfig.HideQuestZones.Value;
						}
						if (zone is Zone_DungeonFairy && EClass.game.quests.GetPhase<QuestNasu>() == Quest.PhaseComplete) {
							return !PluginConfig.HideQuestZones.Value;
						}
						if (zone is Zone_Nymelle && EClass.game.quests.GetPhase<QuestExploration>() == Quest.PhaseComplete) {
							return !PluginConfig.HideQuestZones.Value;
						}
						if (zone is Zone_VernisMine && EClass.game.quests.GetPhase<QuestVernis>() == Quest.PhaseComplete) {
							return !PluginConfig.HideQuestZones.Value;
						}
						if (zone is Zone_CursedManorDungeon && EClass.game.quests.GetPhase<QuestCursedManor>() == Quest.PhaseComplete) {
							return !PluginConfig.HideQuestZones.Value;
						}
						if (zone is Zone_UnderseaTemple && EClass.game.quests.GetPhase<QuestNegotiationDarkness>() == Quest.PhaseComplete) {
							return !PluginConfig.HideQuestZones.Value;
						}
						if (zone is Zone_Lysanas || zone is Zone_Lesimas) { // not implemented quest ?
							return !PluginConfig.HideQuestZones.Value;
						}
						if (zone is Zone_Void) {
							return topZone.FindDeepestZone() == a;
						}
						//if (zone is Zone_Field) {
						//	return false;
						//}
						return PluginConfig.OnlyVisited.Value ? (zone?.visitCount > 0 || topZone?.visitCount > 0) : true;
					})
					.Cast<Zone>()
					.GroupBy(item => item.uid).Select(item => item.First()) // remove dungeon dublicates
					.ToList();

				if (PluginConfig.InstaReturn.Value) {
					EClass.debug.instaReturn = true;
				}
			}
			__result = __result.OrderByDescending((Zone a) => a.uid == EClass.pc.homeZone.uid)
				.ThenByDescending((Zone a) => a.IsPCFaction)
				.ThenByDescending((Zone a) => a is Zone_Civilized)
				.ThenBy((Zone a) => a.Name)
				.ThenBy((Zone a) => a.GetTopZone()?.GetSortVal())
				.ThenBy((Zone a) => a.GetSortVal()).ToList();
		} catch (Exception ex) {
            Plugin.Logger.LogError(ex.Message + Environment.NewLine + ex.StackTrace);
        }
		return false;
	}
}
