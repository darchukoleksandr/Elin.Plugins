using HarmonyLib;
using System;
using System.Collections.Generic;

namespace FullCodexBrowser.Patches
{
	[HarmonyPatch(typeof(WidgetCodex))]
	[HarmonyPatch(nameof(WidgetCodex.Search))]
	public static class WidgetCodexPatch
	{
		public static bool Prefix(WidgetCodex __instance, string s)
		{
			try
			{
				s = s.ToLower();
				__instance.buttonClear.SetActive(__instance.field.text != "");
				if (s == __instance.lastSearch)
				{
					return false;
				}
				RecipeManager.BuildList();
				HashSet<Recipe> hashSet = new HashSet<Recipe>();
				if (!s.IsEmpty())
				{
					foreach (RecipeSource item in RecipeManager.list)
					{
						if (!item.isChara && !item.noListing && !item.isBridgePillar &&
							(item.row.GetSearchName(jp: false).Contains(s) || item.row.GetSearchName(jp: true).Contains(s)))
						{
							hashSet.Add(Recipe.Create(item));
						}
					}
				}
				else
				{
					foreach (RecipeSource item2 in RecipeManager.list)
					{
						if (!item2.isChara && !item2.noListing && !item2.isBridgePillar)
						{
							hashSet.Add(Recipe.Create(item2));
						}
					}
				}
				if (!hashSet.SetEquals(__instance.recipes))
				{
					__instance.recipes = hashSet;
					__instance.RefreshList();
				}
				__instance.lastSearch = s;
			}
			catch (Exception ex)
			{
				Plugin.Log("Error: " + ex.Message);
				return true;
			}
			return false;
		}
	}
}
