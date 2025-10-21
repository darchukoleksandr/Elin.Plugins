using HarmonyLib;
using System;

namespace CustomSkillPotentialCost;

internal class Patches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Element), nameof(Element.CostTrain), MethodType.Getter)]
    public static bool CostTrain(Element __instance, ref int __result)
    {
        try {
		    __result = PluginConfig.TrainCost.Value;
		    return false;
        } catch (Exception ex) {
            Plugin.Logger.LogError(ex.Message + Environment.NewLine + ex.StackTrace);
            return true;
        }
	}

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Element), nameof(Element.CostLearn), MethodType.Getter)]
    public static bool CostLearn(Element __instance, ref int __result)
    {
        try
        {
            __result = PluginConfig.LearnCost.Value;
            return false;
        } catch (Exception ex) {
            Plugin.Logger.LogError(ex.Message + Environment.NewLine + ex.StackTrace);
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CalcPlat), nameof(CalcPlat.Learn), MethodType.Normal)]
    public static bool Learn(Element __instance, Chara c, Element e, ref int __result)
    {
        try {
            if (PluginConfig.IsRelationAffectsPrice.Value && e.source.tag.Contains("guild") && Guild.Current.relation.rank < 2) {
                __result = PluginConfig.LearnCost.Value * 2;
            } else {
                __result = PluginConfig.LearnCost.Value;
            }
            return false;
        } catch (Exception ex) {
            Plugin.Logger.LogError(ex.Message + Environment.NewLine + ex.StackTrace);
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CalcPlat), nameof(CalcPlat.Train), MethodType.Normal)]
    public static bool Train(Element __instance, Chara c, Element _e, ref int __result)
    {
        try {
            if (PluginConfig.IsRelationAffectsPrice.Value && _e.source.tag.Contains("guild") && Guild.Current.relation.rank < 2) {
                __result = PluginConfig.TrainCost.Value * 2;
            } else {
                __result = PluginConfig.TrainCost.Value;
            }
            return false;
        } catch (Exception ex) {
            Plugin.Logger.LogError(ex.Message + Environment.NewLine + ex.StackTrace);
            return true;
        }
    }
}
