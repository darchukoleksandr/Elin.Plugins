using System;
using System.Collections.Generic;
using HarmonyLib;

namespace DisablePotentialDecrease;

public class Patches
{
    private static readonly Dictionary<int, int> Values = new Dictionary<int, int>();

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ElementContainerCard), nameof(ElementContainerCard.OnLevelUp))]
    public static void ElementContainerCardOnLevelUp(ElementContainerCard __instance, Element e, int lastValue)
    {
        try {
            if (__instance?.Card?.Chara?.IsPC == false) {
                return;
            }
            if (e != null) {
                if (e is Skill && !PluginConfig.IsEnabledForSkills.Value) {
                    return;
                }
                if (e is AttbMain && !PluginConfig.IsEnabledForAttributes.Value) {
                    return;
                }
                if (!(e is Skill) && !(e is AttbMain)) {
                    return;
                }

                Plugin.Logger.LogInfo($"Skill {e.Name} leveled up. Old temp potential {e.vTempPotential};");
                Values[e.id] = e.vTempPotential;
            }
        } catch (Exception ex) {
            Plugin.Logger.LogError(ex.Message + Environment.NewLine + ex.StackTrace);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ElementContainer), nameof(ElementContainer.ModExp))]
    public static void ElementContainerModExp(ElementContainer __instance, int ele, bool chain)
    {
        try {
            if (__instance?.Card?.Chara?.IsPC == false) {
                return;
            }
            Element element = __instance.GetElement(ele);
            if (element == null || !Values.ContainsKey(ele)) {
                return;
            }
            Plugin.Logger.LogInfo($"Skill {element.Name}. Resetting {element.vTempPotential} with old value {Values[ele]}");
            element.vTempPotential = Values[ele];
            Values.Remove(ele);
        } catch (Exception ex) {
            Plugin.Logger.LogError(ex.Message + Environment.NewLine + ex.StackTrace);
        }
    }
}
