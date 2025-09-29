using System;
using HarmonyLib;

namespace ShowOfferingValue;

public class Patches
{
	[HarmonyPostfix]
	[HarmonyPatch(typeof(Thing), nameof(Thing.WriteNote))]
	public static void Postfix(Thing __instance, UINote n) {
		try {
			var value = EClass.pc.faith.GetOfferingValue(__instance, 1);
			if (value > 0) {
				n.AddText($"Offer value: {EClass.pc.faith.GetOfferingValue(__instance, 1)}", FontColor.Default);
			}
		} catch (Exception ex) {
			Plugin.Logger.LogInfo(ex.Message + Environment.NewLine + ex.StackTrace);
		}
	}
}
