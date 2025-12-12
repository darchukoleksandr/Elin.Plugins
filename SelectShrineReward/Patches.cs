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
					if (PluginConfig.CheckPartyMembers.Value) {
						b.AddSubButton(EClass.core.refs.icons.fav, () => { }, null,
							(UITooltip tooltip) => {
								ElementContainer container = new ElementContainer();
								tooltip.note.Clear();
								tooltip.note.AddHeader("Party");
								foreach (Chara member in EClass.pc.party.members) {
									var skill = member.elements.dict.FirstOrDefault(item => item.Key == a.id);
									//Plugin.Logger.LogInfo($"skill {skill.Key} member {member.Name} base {skill.Value?.vBase}  total {skill.Value?.Value}");
									bool hasSkill = skill.Value != null && skill.Value.vBase > 0;
									tooltip.note.AddText(member.Name, hasSkill ? FontColor.Good : FontColor.Bad).Hyphenate();
									tooltip.note.Space(4);
								}
								tooltip.note.Build();
							}).icon.SetAlpha(0.4f);
					}
					if (PluginConfig.CheckLandsMembers.Value) {
						foreach (FactionBranch faction in EClass.pc.faction.GetChildren()) {
							b.AddSubButton(EClass.core.refs.icons.home, () => { }, null,
								(UITooltip tooltip) => {
									ElementContainer container = new ElementContainer();
									tooltip.note.Clear();
									tooltip.note.AddHeader(faction.owner.Name);
									foreach (Chara member in faction.members) {
										if (member.memberType == FactionMemberType.Livestock) continue;

										var skill = member.elements.dict.FirstOrDefault(item => item.Key == a.id);
										//Plugin.Logger.LogInfo($"skill {skill.Key} member {member.Name} base {skill.Value?.vBase}  total {skill.Value?.Value}");
										bool hasSkill = skill.Value != null && skill.Value.vBase > 0;
										tooltip.note.AddText(member.Name, hasSkill ? FontColor.Good : FontColor.Bad).Hyphenate();
										tooltip.note.Space(4);
									}
									tooltip.note.Build();
								}).icon.SetAlpha(0.4f);
						}
					}
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
