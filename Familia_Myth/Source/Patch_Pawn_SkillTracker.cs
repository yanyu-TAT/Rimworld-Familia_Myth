using HarmonyLib;
using RimWorld;
using FamiliaMyth.Pharna;
using Verse;

namespace FamiliaMyth.Patch
{
    [HarmonyPatch(typeof(Pawn_SkillTracker), nameof(Pawn_SkillTracker.Learn))]
    public static class Patch_Pawn_SkillTracker
    {
        public static void Postfix(Pawn ___pawn, SkillDef sDef, float xp)
        {
            if (sDef == null) return;

            Hediff_Pharna pharna = Hediff_Pharna.GetPharna(___pawn);
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

            return;
        }
    }
}