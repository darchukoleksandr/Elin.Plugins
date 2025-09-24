using System;
using System.Linq;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using static ActPlan;

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

	public static void ShowZoneTourismItems() {
		try {
			IEnumerable<IGrouping<string, (Thing thing, int)>> items = EMono.game.activeZone.map.things
				.Where((Thing thing) => thing.IsInstalled && thing.HasTag(CTAG.tourism))
				.Select((Thing thing) => (thing, GetPrice(thing)))
				.GroupBy(item => GetThingSourceRefId(item.thing));

			var sortRule = ClassExtension.ToEnum<UIList.SortMode>(PluginConfig.SortRule.Value, true);
			switch (sortRule) {
				case UIList.SortMode.ByNumber:
					items = items.OrderByDescending(item => item.Count());
					break;
				case UIList.SortMode.ByPrice:
					items = items.OrderByDescending(item => item.Max(group => group.Item2));
					;
					break;
				default: // ByName
					items = items.OrderBy(item => item.First().thing.trait.Name);
					break;
			}
			IGrouping<string, (Thing thing, int)>[] sortedList = items.ToArray();

			EClass.ui.AddLayer<LayerList>().SetList2(sortedList,
				getText: (IGrouping<string, (Thing, int)> group) => {
					Thing thing = group.First().Item1;
					return thing.trait.Name;
				},
				onClick: delegate (IGrouping<string, (Thing, int)> group, ItemGeneral b) {
					ShowZoneTourismItemsBytCategory(group.First().Item1);
				},
				onInstantiate: delegate (IGrouping<string, (Thing, int)> group, ItemGeneral b) {
					int maxTourismValue = group.Max(item => item.Item2);
					int categoryCount = group.Count();
					b.SetSubText($"({maxTourismValue}, {categoryCount})", 200, FontColor.Default, TextAnchor.MiddleRight);
					b.Build();
					b.button1.mainText.rectTransform.sizeDelta = new Vector2(350f, 20f);
				}
			).SetSize(900f)
				.SetOnKill(delegate { });
		} catch (Exception ex) {
			Plugin.Logger.LogError(ex.Message + Environment.NewLine + ex.StackTrace);
			throw;
		}
	}

	public static void ShowZoneTourismItemsBytCategory(Thing source) {
		try {
			Thing[] items = EMono.game.activeZone.map.things
				.Where(thing => thing.IsInstalled && thing.HasTag(CTAG.tourism) &&
					GetThingSourceRefId(thing) == GetThingSourceRefId(source))
				.ToArray();

			EClass.ui.AddLayer<LayerList>().SetList2(items,
				getText: (Thing thing) => {
					return thing.Name;
				},
				onClick: delegate (Thing thing, ItemGeneral b) { },
				onInstantiate: delegate (Thing thing, ItemGeneral b) {
					int price = GetPrice(thing);
					b.SetSubText($"({price})", 200, FontColor.Default, TextAnchor.MiddleRight);
					b.Build();
					b.button1.mainText.rectTransform.sizeDelta = new Vector2(350f, 20f);
				}
			).SetSize(900f)
				.SetOnKill(delegate { });
		} catch (Exception ex) {
			Plugin.Logger.LogError(ex.Message + Environment.NewLine + ex.StackTrace);
			throw;
		}
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
			var items = EMono.game.activeZone.map.things.Where((Thing thing) => thing.IsInstalled && thing.HasTag(CTAG.tourism)).ToArray();
			IEnumerable<TourismItemModel> models = items.Select(GetTourismModel)
				.GroupBy(item => item.SourceRefId)
				.Select(group => group.OrderByDescending(item => item.Price).First()).ToArray();
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
			if (!__instance.HasTag(CTAG.tourism) || (newState != PlaceState.none && newState != PlaceState.installed)) {
				return;
			}
			var thing = __instance.Thing;

			var newItem = EMono.game.activeZone.map.things
				.Where((Thing t) => t.IsInstalled && GetThingSourceRefId(t) == GetThingSourceRefId(thing))
				.OrderByDescending((Thing t) => GetPrice(t))
				.FirstOrDefault();
			var sourceRefId = GetThingSourceRefId(thing);

			if (newState == PlaceState.none) { // thing picked up
				if (newItem == null) {
					MapItems[EMono.game.activeZone.uid].Remove(sourceRefId);
				} else {
					MapItems[EMono.game.activeZone.uid][sourceRefId] = GetTourismModel(newItem);
				}
			} else if (newState == PlaceState.installed && byPlayer) { // thing placed down
				MapItems[EMono.game.activeZone.uid][sourceRefId] = GetTourismModel(newItem);
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
						n.AddText($"{faction.owner.Name}: this is the best one in current zone", FontColor.Default);
					} else if (hasItem) {
						FontColor color =
							price == model.Price
								? FontColor.Default
								: price > model.Price
									? FontColor.Good
									: FontColor.Bad;
						n.AddText($"{faction.owner.Name}: {price} ({model.Name})", color);
					} else {
						n.AddText($"{faction.owner.Name}: zone does not have this item", FontColor.Good);
					}
				} else {
					n.AddText($"{faction.owner.Name}: zone was not loaded", FontColor.Flavor);
				}
			}
		} catch (Exception ex) {
			Plugin.Logger.LogInfo(ex.Message + Environment.NewLine + ex.StackTrace);
		}
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(ActPlan), nameof(ActPlan.ShowContextMenu))]
	public static bool Postfix(ref ActPlan __instance) {
		if (!__instance.pos.Equals(EClass.pc.pos) || EClass.game.activeZone.IsRegion || !EMono.game.activeZone.IsPCFaction) {
			return true;
		}

		var act = new DynamicAct("Show tourism items", () => {
			ShowZoneTourismItems();
			return false;
		}, false) {
			id = "Show tourism items",
			dist = 1,
			isHostileAct = false,
			localAct = true,
			cursor = ((CursorSystem.Arrow == null) ? null : null),
			canRepeat = () => true
		};
		__instance.list.Add(new Item {
			act = act,
			tc = null,
			pos = EClass.pc.pos.Copy()
		});
		return true;
	}

	//[HarmonyPostfix]
	//[HarmonyPatch(typeof(TraitHomeBoard), nameof(TraitHomeBoard.TrySetAct))]
	//public static void Postfix(ref ActPlan p, ref TraitHomeBoard __instance) {
	//	Card owner = __instance.owner;
	//	p.TrySetAct("Show tourism items", () => {
	//		ShowZoneTourismItems();
	//		return true;
	//	}, null, 1);
	//}
}
