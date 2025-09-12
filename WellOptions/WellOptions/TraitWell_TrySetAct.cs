using HarmonyLib;

namespace WellOptions
{
	[HarmonyPatch(typeof(TraitWell), nameof(TraitWell.TrySetAct))]
	internal class TraitWell_TrySetAct
	{
		private static bool Prefix(ref TraitWell __instance, ActPlan p) {
			if (!ModConfig.IsModEnabled.Value) {
				return true;
			}

			TraitWell instance = __instance;
			p.TrySetAct("actDrink", delegate {
				if (instance.Charges <= 0) {
					EClass.pc.Say("drinkWell_empty", EClass.pc, instance.owner);
					return false;
				}

				EClass.pc.Say("drinkWell", EClass.pc, instance.owner);
				EClass.pc.PlaySound("drink");
				EClass.pc.PlayAnime(AnimeID.Shiver);
				if (EClass.rnd(EClass.debug.enable ? 2 : 10) == 0 && !instance.polluted && !EClass.player.wellWished) {
					if (EClass.player.CountKeyItem("well_wish") > 0) {
						EClass.player.ModKeyItem("well_wish", -1);
						ActEffect.Proc(EffectId.Wish, EClass.pc, null, 50 + EClass.player.CountKeyItem("well_enhance") * 50);
						EClass.player.wellWished = true;
					} else {
						Msg.SayNothingHappen();
					}
				} else if (ModConfig.IsAlwaysPositiveEffectEnabled.Value) {
					ActEffect.Proc(EffectId.ModPotential, EClass.pc, null, 100);
				} else if (instance.IsHoly || EClass.rnd(5) == 0) {
					var isPositive = (!instance.polluted && (instance.IsHoly || EClass.rnd(2) == 0));
					if (isPositive) {
						ActEffect.Proc(EffectId.ModPotential, EClass.pc, null, 100);
					} else if (ModConfig.IsNegativeEffectsDisabled.Value) {
						EClass.pc.Say("drinkWater_clear");
					} else {
						ActEffect.Proc(EffectId.ModPotential, EClass.pc, null, -100);
					}
				} else if (EClass.rnd(5) == 0) {
					if (ModConfig.IsNegativeEffectsDisabled.Value) {
						EClass.pc.Say("drinkWater_clear");
					} else {
						TraitWell.BadEffect(EClass.pc);
					}
				} else if (EClass.rnd(4) == 0) {
					if (!ModConfig.IsMutationsDisabled.Value) {
						ActEffect.Proc(EffectId.Mutation, EClass.pc);
						EClass.pc.Say("resistMutation");
					}
				} else if (instance.polluted) {
					EClass.pc.Say("drinkWater_dirty");
					if (!ModConfig.IsNegativeEffectsDisabled.Value) {
						TraitWell.BadEffect(EClass.pc);
					}
				} else {
					EClass.pc.Say("drinkWater_clear");
				}

				instance.ModCharges(-1);
				return true;
			}, __instance.owner);
            return false;
        }
	}
}
