using System;
using System.Linq;
using System.Collections.Generic;
using HarmonyLib;

namespace TourismList;

public class Patches
{
	public static Dictionary<int, Dictionary<string, TourismItemModel>> MapItems = [];

	public class TourismItemModel
	{
		public string Name { get; set; }

		public int Price { get; set; }

		public string SourceRefId { get; set; }

		public int UId { get; set; }
	}

	public static string GetThingSourceRefId(Thing thing) {
		string refId = null;
		if (thing.category.id == "figure") {
			refId = $"{thing.category.id}_{thing.c_idRefCard}";
		} else if (thing.category.id == "hanging") {
			refId = $"{thing.category.id}_{thing.source.id}";
		} else {
			//Plugin.Logger.LogWarning($"GetThingSourceRefId item was not expected: trait {thing.trait.GetType().Name}, source {thing.source.id}");
			refId = $"{thing.category.id}_{thing.source.id}";
		}
		return refId;
	}

	public static int GetPrice(Thing thing, FactionBranch faction = null) {
		int price;
		if (faction != null) {
			price = faction.resources.worth.GetPrice(thing);
		} else {
			price = EMono.Branch.resources.worth.GetPrice(thing);
		}
		if (thing.trait is TraitFigure) {
			price *= 2;
		}
		return price;
	}

	public static TourismItemModel GetTourismModel(Thing item) {
		int price = GetPrice(item);
		string refId = GetThingSourceRefId(item);

		return new TourismItemModel {
			Name = $"{item.Name} ({item.material.name}, {price})",
			Price = price,
			SourceRefId = refId,
			UId = item.uid
		};
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(Zone), nameof(Zone.Activate))]
	public static void Postfix(ref Zone __instance) {
		try {
			var items = EMono.game.activeZone.map.things.Where((Thing t) => t.IsInstalled && t.HasTag(CTAG.tourism)).ToArray();
			IEnumerable<TourismItemModel> models = items.Select(GetTourismModel).ToArray();
			MapItems[__instance.uid] = [];
			foreach (TourismItemModel item in models) {
				MapItems[__instance.uid][item.SourceRefId] = item;
			}
		} catch (Exception ex) {
			Plugin.Logger.LogInfo(ex.Message + Environment.NewLine + ex.StackTrace);
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(Card), nameof(Card.SetPlaceState))]
	static void Postfix(ref Card __instance, PlaceState newState, bool byPlayer = false) {
		try {
			if (!__instance.HasTag(CTAG.tourism)) {
				return;
			}
			if (newState == PlaceState.none) { // thing picked up
				MapItems[EMono.game.activeZone.uid].Remove(GetThingSourceRefId(__instance.Thing));
			} else if (newState == PlaceState.installed && byPlayer) { // thing placed down
				MapItems[EMono.game.activeZone.uid][GetThingSourceRefId(__instance.Thing)] = GetTourismModel(__instance.Thing);
			}
		} catch (Exception ex) {
			Plugin.Logger.LogInfo(ex.Message + Environment.NewLine + ex.StackTrace);
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(Thing), nameof(Thing.WriteNote))]
	public static void Postfix(Thing __instance, UINote n) {
		try {
			if (!__instance.HasTag(CTAG.tourism)) {
				return;
			}
			foreach (FactionBranch faction in EClass.pc.faction.GetChildren()) {
				bool isZoneLoaded = MapItems.TryGetValue(faction.owner.uid, out Dictionary<string, TourismItemModel> mapItems);
				int price = GetPrice(__instance, faction);
				if (isZoneLoaded) {
					bool hasItem = mapItems.TryGetValue(GetThingSourceRefId(__instance), out TourismItemModel model);
					if (hasItem && model.UId == __instance.uid) {
						n.AddText($"{faction.owner.Name}: {price} this is the best one in current zone", FontColor.Default);
					} else if (hasItem) {
						FontColor color =
							price == model.Price
								? FontColor.Default
								: price > model.Price
									? FontColor.Good
									: FontColor.Bad;
						n.AddText($"{faction.owner.Name}: {price} ({model.Name})", color);
					} else {
						n.AddText($"{faction.owner.Name}: {price} no such item", FontColor.Good);
					}
				} else {
					n.AddText($"{faction.owner.Name}: {price} not loaded", FontColor.Flavor);
				}
			}
		} catch (Exception ex) {
			Plugin.Logger.LogInfo(ex.Message + Environment.NewLine + ex.StackTrace);
		}
	}
}
