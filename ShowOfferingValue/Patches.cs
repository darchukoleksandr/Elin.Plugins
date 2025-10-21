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

	//[HarmonyPostfix]
	//[HarmonyPatch(typeof(ButtonElement), nameof(ButtonElement.Refresh))]
	//public static void Refresh(ButtonElement __instance) {
	//	try {
	//		//__instance.mainText.text += "test1";
	//		//__instance.subText.text += "test2";
	//		//if (__instance.tooltip != null) {
	//		//	__instance.tooltip.text += "test";
	//		//}
			
	//	} catch (Exception ex) {
	//		Plugin.Logger.LogInfo(ex.Message + Environment.NewLine + ex.StackTrace);
	//	}
	//}

	//[HarmonyPostfix]
	//[HarmonyPatch(typeof(WindowChara), nameof(WindowChara.RefreshInfo))]
	//public static void RefreshInfo(WindowChara __instance) {
	//	try {
	//		Element element = __instance.chara.elements.GetOrCreateElement(ELEMENT.piety);
	//		__instance.textFaith.text += $": {element.vBase} / {element.vExp}";
	//	} catch (Exception ex) {
	//		Plugin.Logger.LogInfo(ex.Message + Environment.NewLine + ex.StackTrace);
	//	}
	//}
}
