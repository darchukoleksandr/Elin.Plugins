using System;
using System.Collections;
using System.Collections.Generic;
using HarmonyLib;

namespace RestForStamina;

public class Patches
{
	class SimpleEnumerator : IEnumerable<AIAct.Status>
	{
		public IEnumerable<AIAct.Status> enumerator;
		public Func<AIAct.Status, AIAct.Status> itemAction;

		public IEnumerator<AIAct.Status> GetEnumerator() {
			var enums = enumerator.GetEnumerator();
			while (true) {
				AIAct.Status status;
				try {
					if (!enums.MoveNext()) {
						break;
					}
					status = enums.Current;
				} catch (NullReferenceException) { // base game issue?
					yield break;
				} catch (Exception ex) {
					Plugin.Logger.LogInfo(ex.Message + Environment.NewLine + ex.StackTrace);
					yield break;
				}
				yield return itemAction(status);
			}
		}

		IEnumerator IEnumerable.GetEnumerator() {
			try {
				return GetEnumerator();
			} catch (Exception ex) {
				Plugin.Logger.LogInfo(ex.Message + Environment.NewLine + ex.StackTrace);
				throw;
			}
		}
	}

	[HarmonyPostfix]
    [HarmonyPatch(typeof(AI_PassTime), nameof(AI_PassTime.Run))]
    public static void Postfix(AI_PassTime __instance, ref IEnumerable<AIAct.Status> __result)
    {
        try {
			if (__instance.type != AI_PassTime.Type.meditate) {
                return;
            }

			Func<AIAct.Status, AIAct.Status> itemAction = (item) => {
				try {
					if (item != AIAct.Status.Running) {
						return item;
					}
					foreach (Chara member in EClass.pc.party.members) {
						foreach (Condition condition in member.conditions) {
							if (!condition.PreventRegen && member.stamina.value < member.stamina.max) {
								int num = 1 + EClass.pc.Evalue(ABILITY.AI_Meditate) / 5;
								var value = num * (member.IsPC ? 1 : 2);
								member.stamina.Mod(value);
							}
						}
					}
					return item;
				} catch (Exception ex) {
					Plugin.Logger.LogError(ex.Message + Environment.NewLine + ex.StackTrace);
					throw;
				}
			};
			var myEnumerator = new SimpleEnumerator() {
				enumerator = __result,
				itemAction = itemAction
			};
			__result = myEnumerator;
        } catch (Exception ex) {
            Plugin.Logger.LogError(ex.Message + Environment.NewLine + ex.StackTrace);
        }
    }
}
