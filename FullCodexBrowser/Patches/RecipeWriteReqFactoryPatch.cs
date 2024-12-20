using HarmonyLib;
using System;

namespace FullCodexBrowser.Patches
{
	[HarmonyPatch(typeof(Recipe))]
	[HarmonyPatch(nameof(Recipe.WriteReqFactory))]
	public static class RecipeWriteReqFactoryPatch
	{
		public static bool Prefix(Recipe __instance, UINote n)
		{
			try
			{
				if (__instance.source.NeedFactory)
				{
					int num = EMono.player.recipes.knownRecipes.TryGetValue(__instance.id, 0);
					if (num > 0)
					{
						//note.AddHeaderTopic("reqFactory".lang("Recipe learned".TagColor(FontColor.Bad)));
						n.AddHeaderTopic("Recipe learned".TagColor(FontColor.Good));
					}
					else
					{
						n.AddHeaderTopic("Recipe not learned".TagColor(FontColor.Bad));
					}
				}
				return true;
			}
			catch (Exception ex)
			{
				Plugin.Log("Error: " + ex.Message);
				return true;
			}
		}
	}

}
