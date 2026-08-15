using FamiliaMyth.Abilities;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Patch
{
    [HarmonyPatch(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.PreApplyDamage))]
    public static class Patch_IaiSpiritSlash
    {
        private static FleckDef blockFleckDef;
        private static FleckDef BlockFleckDef =>
            blockFleckDef ??= DefDatabase<FleckDef>.GetNamed("BlockSpark");

        public static bool Prefix(Pawn ___pawn, DamageInfo dinfo, ref bool absorbed)
        {
            var skill = Skill_IaiSpiritSlash.GetActiveSkill(___pawn);
            if (skill == null) return true; //非居合状态：正常结算伤害

            //吸收伤害并记录
            skill.absorbedDamage += dinfo.Amount;
            absorbed = true;

            //格挡特效
            skill.moteSneathe?.Hit(dinfo.Amount);
            var loc = ___pawn.DrawPos + Rand.InsideUnitCircleVec3;
            var cnt = Rand.Range(1, 10);
            for (int i = 0; i < cnt; i++)
            {
                var data = FleckMaker.GetDataStatic(loc, ___pawn.Map, BlockFleckDef);
                data.velocity = Rand.InsideUnitCircleVec3 * Rand.Range(1f, 1.2f);
                data.scale = Rand.Range(1f, 1.2f);
                data.rotation = Rand.Range(0f, 360f);
                data.instanceColor = Color.red.WithAlpha(0.5f);
                ___pawn.Map.flecks.CreateFleck(data);
            }

            //跳过原逻辑
            return false; 
        }
    }
}
