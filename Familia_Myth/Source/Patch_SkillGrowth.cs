using HarmonyLib;
using RimWorld;
using FamiliaMyth.Pharna;
using Verse;
using FamiliaMyth.Abilities;

namespace FamiliaMyth.Patch
{
    [HarmonyPatch(typeof(Pawn_SkillTracker), nameof(Pawn_SkillTracker.Learn))]
    public static class Patch_SkillGrowth
    {
        //技能经验 -> 能力值 转化率（每点 XP）
        private const float StrengthPerXpMelee = 0.02f;
        private const float StrengthPerXpMining = 0.05f;
        private const float DexterityPerXp = 0.02f;

        public static void Postfix(Pawn ___pawn, RimWorld.SkillDef sDef, float xp)
        {
            try
            {
                if (sDef == null || ___pawn == null) return;

                Hediff_Pharna pharna = ___pawn.GetPharna();
                if (pharna == null) return;

                if (sDef == SkillDefOf.Melee)
                    pharna.GainStatus(PharnaStatusType.Strength, xp * StrengthPerXpMelee);

                if (sDef == SkillDefOf.Mining)
                    pharna.GainStatus(PharnaStatusType.Strength, xp * StrengthPerXpMining);

                if (sDef == SkillDefOf.Shooting)
                    pharna.GainStatus(PharnaStatusType.Dexterity, xp * DexterityPerXp);

                if (sDef == SkillDefOf.Construction)
                    pharna.GainStatus(PharnaStatusType.Dexterity, xp * DexterityPerXp);

                if (sDef == SkillDefOf.Cooking)
                    pharna.GainStatus(PharnaStatusType.Dexterity, xp * DexterityPerXp);

                if (sDef == SkillDefOf.Plants)
                    pharna.GainStatus(PharnaStatusType.Dexterity, xp * DexterityPerXp);

                if (sDef == SkillDefOf.Artistic)
                    pharna.GainStatus(PharnaStatusType.Dexterity, xp * DexterityPerXp);

                if (sDef == SkillDefOf.Crafting)
                    pharna.GainStatus(PharnaStatusType.Dexterity, xp * DexterityPerXp);

                if (sDef == SkillDefOf.Medicine)
                    pharna.developableAbilities.FirstOrDefault(d => d is DevelopableAbility_Healing)?.GainProficiency(xp * DevelopableAbility_Healing.ProfGainPerXp);

                if (sDef == SkillDefOf.Plants)
                    pharna.developableAbilities.FirstOrDefault(d => d is DevelopableAbility_Botany)?.GainProficiency(xp * DevelopableAbility_Botany.ProfGainPerXp);

                if (sDef == SkillDefOf.Mining)
                    pharna.developableAbilities.FirstOrDefault(d => d is DevelopableAbility_Mineralogy)?.GainProficiency(xp * DevelopableAbility_Mineralogy.ProfGainPerXp);
            }
            catch (Exception ex)
            {
                Log.Error($"[FamiliaMyth] {nameof(Patch_SkillGrowth)} error: {ex}");
            }
        }
    }
}
