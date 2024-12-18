using HarmonyLib;
using System;

namespace GoAroundThatTree.Patches
{
	[HarmonyPatch(typeof(Cell), nameof(Cell.Refresh))]
	public class CellRefreshPatch
	{
		public static void Postfix(Cell __instance)
		{
			try
			{
				var tile = Cell.objList[__instance.obj];
				if (__instance.blocked && tile != null && tile.tileType is TileTypeTree)
				{
					__instance.blocked = false;
				}
			}
			catch (Exception ex)
			{
				Plugin.Log("Error: " + ex.Message);
			}
		}
	}
}
