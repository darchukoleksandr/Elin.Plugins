using HarmonyLib;
using System;

namespace NegotiationRedone.Patches
{
	[HarmonyPatch(typeof(Affinity), nameof(Affinity.OnTalkRumor))]
	public class AffinityOnTalkRumorPatch
	{
		public static bool Prefix(Affinity __instance)
		{
			try
			{
				bool isSuccessful = PluginConfig.CheckSuccess.Value
					? EClass.rnd(60 + EClass.pc.CHA * 2 + EClass.pc.Evalue(SKILL.negotiation) * 3) >
						50 + __instance.difficulty + EClass.rnd(Affinity.CC.CHA + 1)
					: true;

				if (isSuccessful)
				{
					int result = EClass.rnd(PluginConfig.MaxAffinity.Value) + PluginConfig.MinAffinity.Value;
					//Plugin.Log("OnTalkRumorPrefix result: " + result);
					if (result > 0)
					{
						Affinity.CC.ModAffinity(EClass.pc, result, show: false);
					}
				}
				if (!EClass.debug.unlimitedInterest)
				{
					Affinity.CC.interest -= 10 + EClass.rnd(10);
				}

				EClass.pc.ModExp(SKILL.negotiation, 20);
				return false;
			}
			catch (Exception ex)
			{
				Plugin.Log("Error: " + ex.Message);
				return true;
			}
		}
	}
}
