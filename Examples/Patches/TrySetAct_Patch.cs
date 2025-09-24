using System;
using HarmonyLib;

namespace QuestBoardTax
{
	[HarmonyPatch(typeof(TraitQuestBoard), "TrySetAct")]
	public static class TrySetAct_Patch
	{
		[HarmonyPostfix]
		public static void Postfix(ref ActPlan p, ref TraitQuestBoard __instance)
		{
			Card owner = ((Trait)__instance).owner;
			p.TrySetAct("stat_tax", (Func<bool>)delegate
			{
				LayerDragGrid.CreateDeliver(1, owner);
				return true;
			}, (CursorInfo)null, 1);
		}
	}
}
