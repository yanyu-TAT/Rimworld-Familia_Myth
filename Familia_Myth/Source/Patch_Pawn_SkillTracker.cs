using HarmonyLib;
using RimWorld;
using FamiliaMyth.Pharna;
using Verse;
using FamiliaMyth.Abilities;

namespace FamiliaMyth.Patch
{
    [HarmonyPatch(typeof(Pawn_SkillTracker), nameof(Pawn_SkillTracker.Learn))]
    public static class Patch_Pawn_SkillTracker
    {
        public static void Postfix(Pawn ___pawn, RimWorld.SkillDef sDef, float xp)
        {
            try
            {
                if (sDef == null || ___pawn == null) return;

                Hediff_Pharna pharna = ___pawn.GetPharna();
                if (pharna == null) return;

                //每次攻击1点
                if (sDef == SkillDefOf.Melee)
                    pharna.GainStatus(PharnaStatusType.Strength, xp / 300f);

                //每次射击1点
                if (sDef == SkillDefOf.Shooting)
                    pharna.GainStatus(PharnaStatusType.Dexterity, xp / 300f);

                //建筑工作时0.3点/秒
                if (sDef == SkillDefOf.Construction)
                    pharna.GainStatus(PharnaStatusType.Dexterity, xp / 50f);

                //采矿工作室0.3点/秒
                if (sDef == SkillDefOf.Mining)
                    pharna.GainStatus(PharnaStatusType.Strength, xp / 12f);

                //烹饪工作每800点工作量（奢侈食物）1点
                if (sDef == SkillDefOf.Cooking)
                    pharna.GainStatus(PharnaStatusType.Dexterity, xp / 800f);

                //种植工作0.3点/秒
                if (sDef == SkillDefOf.Plants)
                    pharna.GainStatus(PharnaStatusType.Dexterity, xp / 15f);

                //艺术工作0.5点/秒
                if (sDef == SkillDefOf.Artistic)
                    pharna.GainStatus(PharnaStatusType.Dexterity, xp / 12f);

                //普通制作工作0.3点/秒
                if (sDef == SkillDefOf.Crafting)
                    pharna.GainStatus(PharnaStatusType.Dexterity, xp / 20f);

                if (sDef == SkillDefOf.Medicine)
                    pharna.developableAbilities.FirstOrDefault(d => d is DevelopableAbility_Healing)?.GainProficiency(xp * DevelopableAbility_Healing.ProfGainPerXp);

                if (sDef == SkillDefOf.Plants)
                    pharna.developableAbilities.FirstOrDefault(d => d is DevelopableAbility_Botany)?.GainProficiency(xp * DevelopableAbility_Botany.ProfGainPerXp);

                if (sDef == SkillDefOf.Mining)
                    pharna.developableAbilities.FirstOrDefault(d => d is DevelopableAbility_Mineralogy)?.GainProficiency(xp * DevelopableAbility_Mineralogy.ProfGainPerXp);
            }
            catch (Exception ex)
            {
                Log.Error($"[FamiliaMyth] {nameof(Patch_Pawn_SkillTracker)} error: {ex}");
            }
        }
    }
}
