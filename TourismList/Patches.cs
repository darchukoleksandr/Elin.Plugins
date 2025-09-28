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

		public string TraitName { get; set; }

		public int Price { get; set; }

		public int SoloBonus { get; set; }

		public int MassBonus { get; set; }

		public string SourceRefId { get; set; }

		public int UId { get; set; }
	}

	public static void ShowZoneTourismItems() {
		try {
			IEnumerable<IGrouping<string, TourismItemModel>> items = EMono.game.activeZone.map.things
				.Where((Thing thing) => thing.IsInstalled && thing.HasTag(CTAG.tourism))
				.Select((Thing thing) => GetTourismModel(thing))
				.GroupBy((TourismItemModel model) => model.SourceRefId);

			var sortRule = ClassExtension.ToEnum<UIList.SortMode>(PluginConfig.SortRule.Value, true);
			switch (sortRule) {
				case UIList.SortMode.ByNumber:
					items = items.OrderByDescending(item => item.Count());
					break;
				case UIList.SortMode.ByPrice:
					items = items.OrderByDescending(item => item.Max(group => group.Price));
					break;
				default: // ByName
					items = items.OrderBy(item => item.First().TraitName);
					break;
			}
			IGrouping<string, TourismItemModel>[] sortedList = items.ToArray();

			EClass.ui.AddLayer<LayerList>().SetList2(sortedList,
				getText: (IGrouping<string, TourismItemModel> group) => {
					return group.First().TraitName;
				},
				onClick: delegate (IGrouping<string, TourismItemModel> group, ItemGeneral b) {
					ShowZoneTourismItemsBytCategory(items, group.First());
				},
				onInstantiate: delegate (IGrouping<string, TourismItemModel> group, ItemGeneral b) {
					int maxTourismValue = group.Max(item => item.Price);
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

	public static void ShowZoneTourismItemsBytCategory(IEnumerable<IGrouping<string, TourismItemModel>> allItems, TourismItemModel source) {
		try {
			int mass = EClass.Branch.Evalue(POLICY.mass_exhibition);
			int solo = EClass.Branch.Evalue(POLICY.legendary_exhibition);

			TourismItemModel[] items = allItems
				.Where((IGrouping<string, TourismItemModel> group) => group.Key == source.SourceRefId)
				.SelectMany((IGrouping<string, TourismItemModel> group) => group.Select(item => item))
				.OrderByDescending((TourismItemModel model) => model.Price)
				.ToArray();

			EClass.ui.AddLayer<LayerList>().SetList2(items,
				getText: (TourismItemModel model) => {
					return model.Name;
				},
				onClick: delegate (TourismItemModel model, ItemGeneral b) { },
				onInstantiate: delegate (TourismItemModel model, ItemGeneral b) {
					int massOrSolo = 0;
					if (mass > 0) {
						massOrSolo = model.MassBonus;
					} else if (solo > 0 && items[0].UId == model.UId) { // only for first item
						massOrSolo = model.SoloBonus;
					}
					b.SetSubText($"({model.Price},{massOrSolo})", 200, FontColor.Default, TextAnchor.MiddleRight);
					b.Build();
					b.button1.mainText.rectTransform.sizeDelta = new Vector2(650f, 20f);
				}
			).SetSize(900f)
				.SetOnKill(delegate { });
		} catch (Exception ex) {
			Plugin.Logger.LogError(ex.Message + Environment.NewLine + ex.StackTrace);
			throw;
		}
	}

	public static string GetThingSourceRefId(Thing thing) {
		if (thing.category.id == "figure") {
			return $"{thing.category.id}_{thing.c_idRefCard}";
		} else {
			return $"{thing.id}_{thing.idSkin}";
		}
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

	public static TourismItemModel GetTourismModel(Thing item, FactionBranch faction = null) {
		if (faction == null) {
			faction = EClass.Branch;
		}
		int mass = faction.Evalue(POLICY.mass_exhibition);
		int solo = faction.Evalue(POLICY.legendary_exhibition);

		int price = GetPrice(item);
		int soloBonus = GetSoloTourismPrice(price, solo);
		int massBonus = GetMassTourismPrice(price, mass);
		string refId = GetThingSourceRefId(item);
		
		return new TourismItemModel {
			Name = $"{item.Name} ({item.material.name}, {price}, {soloBonus}, {massBonus})",
			TraitName = item.trait.Name,
			Price = price,
			SoloBonus = soloBonus,
			MassBonus = massBonus,
			SourceRefId = refId,
			UId = item.uid,
		};
	}

	public static int GetSoloTourismPrice(int price, int solo) {
		return (price * (110 + (int)Mathf.Sqrt(solo) * 4) / 100) - price;
	}

	public static int GetMassTourismPrice(int price, int mass) {
		return price / Mathf.Max(20, 30 - (int)Mathf.Sqrt(mass));
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(Zone), nameof(Zone.Activate))]
	public static void Postfix(ref Zone __instance) {
		try {
			if (!__instance.IsPCFaction) return;

			var items = EMono.game.activeZone.map.things.Where((Thing thing) => thing.IsInstalled && thing.HasTag(CTAG.tourism)).ToArray();
			IEnumerable<TourismItemModel> models = items.Select((Thing thing) => GetTourismModel(thing))
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
}
