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
						if (PluginConfig.AllowCities.Value && zone is Zone_Civilized city) {
							return PluginConfig.OnlyVisited.Value ? zone.GetTopZone().visitCount > 0 : true;
						}
						var topZone = zone.GetTopZone();
						return PluginConfig.OnlyVisited.Value ? (zone?.visitCount > 0 || topZone?.visitCount > 0) : true;
					})
					.Cast<Zone>()
					.ToList();

				EClass.debug.instaReturn = true;
			}
			__result.Sort((Zone a, Zone b) => a.GetSortVal() - b.GetSortVal());
        } catch (Exception ex) {
            Plugin.Logger.LogError(ex.Message + Environment.NewLine + ex.StackTrace);
        }
		return false;
	}
}
