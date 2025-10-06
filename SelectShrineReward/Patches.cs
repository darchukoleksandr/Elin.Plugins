using System;
using System.Linq;
using HarmonyLib;

namespace SelectShrineReward;

public class Patches
{
	public static void DeactivateShrine(TraitShrine __instance) {
		SE.Play("shrine");
		__instance.owner.PlayEffect("buff");
		__instance.owner.isOn = false;
		__instance.owner.rarity = Rarity.Normal;
		__instance.owner.renderer.RefreshExtra();
	}

	public static bool GetSkillBook(TraitShrine instance) {
		try {
			if (!instance.IsOn) {
				Msg.SayNothingHappen();
				return true;
			}
			var skills = EClass.sources.elements.rows
				.Where((SourceElement.Row row) => row.category == "skill")
				.OrderBy(item => item.name).ToList();
			EClass.ui.AddLayer<LayerList>().SetList2(skills,
				getText: (SourceElement.Row a) => a.name,
				onClick: (SourceElement.Row a, ItemGeneral b) => {
					try {
						Point point = (instance.owner.ExistsOnMap ? instance.owner.pos : EClass.pc.pos);
						Thing book = ThingGen.CreateSkillbook(a.id, 1);
						EClass._zone.AddCard(book, point);
						DeactivateShrine(instance);
					} catch (Exception ex) {
						Plugin.Logger.LogError(ex.Message + Environment.NewLine + ex.StackTrace);
					}
				},
				onInstantiate: (SourceElement.Row a, ItemGeneral b) => {
					b.button1.SetTooltip((UITooltip tooltip) => {
						ElementContainer container = new ElementContainer();
						tooltip.note.Clear();
						tooltip.note.AddHeader("Party");
						foreach (Chara member in EClass.pc.party.members) {
							bool hasSkill = member.elements.dict.ContainsKey(a.id);
							tooltip.note.AddText(member.Name, hasSkill ? FontColor.Good : FontColor.Bad).Hyphenate();
							tooltip.note.Space(8);
						}
						tooltip.note.Build();
					});
					b.Build();
				}
			).SetSize(500f)
				.SetOnKill(delegate { });
		} catch (Exception ex) {
			Plugin.Logger.LogError(ex.Message + Environment.NewLine + ex.StackTrace);
			throw;
		}
		return true;
	}

	public static bool GetAntientBook(TraitShrine instance) {
		if (!instance.IsOn) {
			Msg.SayNothingHappen();
			return true;
		}
		try {
			Point point = (instance.owner.ExistsOnMap ? instance.owner.pos : EClass.pc.pos);
			Thing book = ThingGen.Create("book_ancient", -1, instance.owner.LV);
			EClass._zone.AddCard(book, point);
			DeactivateShrine(instance);
		} catch (Exception ex) {
			Plugin.Logger.LogError(ex.Message + Environment.NewLine + ex.StackTrace);
			throw;
		}
		return true;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(TraitPowerStatue), nameof(TraitPowerStatue.OnUse))]
	public static bool TraitShrineOnUse(TraitPowerStatue __instance, Chara c, ref bool __result) {
		try {
			if (__instance is not TraitShrine instance || instance.Shrine.id != "knowledge") {
				return true;
			}
			string[] items = { "Skill book", "Antient book" };
			Dialog dialog = null;
			dialog = Dialog.List("Select reward", items,
				getString: caption => caption,
				onSelect: (int selected, string caption) => {
					dialog.Close();
					if (selected == 0) {
						GetSkillBook(instance);
					} else if (selected == 1) {
						GetAntientBook(instance);
					}
					return true;
				}, true);
			__result = true;
			return false;
		} catch (Exception ex) {
			Plugin.Logger.LogError(ex.Message + Environment.NewLine + ex.StackTrace);
			throw;
		}
	}
}
