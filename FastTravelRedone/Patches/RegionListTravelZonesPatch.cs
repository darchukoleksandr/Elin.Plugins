using HarmonyLib;
using System;
using System.Collections.Generic;

namespace FastTravelRedone.Patches
{
	[HarmonyPatch(typeof(Region), nameof(Region.ListTravelZones))]
	public class RegionListTravelZonesPatch
	{
		public static bool Prefix(Region __instance, ref List<Zone> __result, int radius = 100)
		{
			try
			{
				bool isRegion = EClass.pc.currentZone.IsRegion;
				__result = new List<Zone>();
				if (!isRegion) {
					new Point(EClass.pc.currentZone.x - EClass.scene.elomap.minX, EClass.pc.currentZone.y - EClass.scene.elomap.minY);
				} else {
					_ = EClass.pc.pos;
				}
				foreach (Zone zone in EClass.game.spatials.Zones) {
					if ((zone.CanFastTravel || PluginConfig.IgnoreCanFastTravel.Value) &&
						(!zone.IsClosed || PluginConfig.IgnoreIsClosed.Value) &&
						!zone.IsInstance && (zone.isKnown || EClass.debug.returnAnywhere || PluginConfig.IgnoreIsKnown.Value) && zone.parent == __instance)
					{
						__result.Add(zone);
						zone.tempDist = zone.Dist(EClass.pc.pos);
						//Plugin.Logger.LogInfo(string.Format("Added Name {0}, CanFastTravel {1}, isKnown {2}, isRandomSite {3}, IsTown {4}",
						//	zone.Name, zone.CanFastTravel, zone.isKnown, zone.isRandomSite, zone.IsTown));
					}
				}
			} catch (Exception ex) {
				Plugin.Logger.LogInfo("Error: " + ex.Message);
				return true;
			}

			return false;
		}
	}
}
