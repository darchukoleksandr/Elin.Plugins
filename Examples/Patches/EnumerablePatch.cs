using System;
using System.Collections;
using System.Collections.Generic;
using HarmonyLib;

namespace Examples;

public class EnumerablePatch
{
	class SimpleEnumerator : IEnumerable<AIAct.Status>
	{
		public IEnumerable<AIAct.Status> enumerator;
		public Action prefixAction, postfixAction;
		public Action<object> preItemAction, postItemAction;
		public Func<AIAct.Status, AIAct.Status> itemAction;

		public IEnumerator<AIAct.Status> GetEnumerator() {
			prefixAction();
			foreach (AIAct.Status status in enumerator) {
				preItemAction(status);
				yield return itemAction(status);
				postItemAction(status);
			}
			postfixAction();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}

	[HarmonyPostfix]
    [HarmonyPatch(typeof(AI_PassTime), nameof(AI_PassTime.Run))]
    public static void Postfix(AI_PassTime __instance, ref IEnumerable<AIAct.Status> __result)
    {
        try {
			Func<AIAct.Status, AIAct.Status> itemAction = (item) => {
				try {
					Console.WriteLine($"--> item {item}");
					return item;
				} catch (Exception ex) {
					Console.WriteLine(ex.Message + Environment.NewLine + ex.StackTrace);
					throw;
				}
			};
			var myEnumerator = new SimpleEnumerator() {
				enumerator = __result,
				prefixAction = () => { Console.WriteLine("--> beginning"); },
				postfixAction = () => { Console.WriteLine("--> ending"); },
				preItemAction = (item) => { Console.WriteLine($"--> before {item}"); },
				postItemAction = (item) => { Console.WriteLine($"--> after {item}"); },
				itemAction = itemAction
			};
			__result = myEnumerator;
        } catch (Exception ex) {
			Console.WriteLine(ex.Message + Environment.NewLine + ex.StackTrace);
        }
    }
}
