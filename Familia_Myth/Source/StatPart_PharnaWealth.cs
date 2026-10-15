using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Pharna
{
    /// <summary>
    /// 按恩惠等级与经验值放大 Pawn 的市场价值，经原版财富链路抬升袭击点数。
    /// 等级归一化线性、经验归一化 smoothstep，两项各域 ×1..×5，乘积顶点 ×25。
    /// </summary>
    public class StatPart_PharnaWealth : StatPart
    {
        private const float LevelMaxBonus = 4f;
        private const float ExpMaxBonus = 4f;
        private const float ExpCap = 30000f;

        public override void TransformValue(StatRequest req, ref float val)
        {
            try
            {
                if (req.Thing is not Pawn pawn)
                    return;

                var pharna = pawn.GetPharna();
                if (pharna is null)
                    return;

                val *= GetFactor(pharna);
            }
            catch (Exception ex)
            {
                Log.Error($"[FamiliaMyth] StatPart_PharnaWealth.TransformValue 失败：{ex}");
            }
        }

        public override string ExplanationPart(StatRequest req)
        {
            try
            {
                if (req.Thing is not Pawn pawn)
                    return null;

                var pharna = pawn.GetPharna();
                if (pharna is null)
                    return null;

                string levelLine = "FamiliaMyth.Stat.PharnaWealthLevel".Translate(
                    pharna.level);
                string experienceLine = "FamiliaMyth.Stat.PharnaWealthExperience".Translate(
                    (int)pharna.experience);

                return levelLine + $": x{GetLevelFactor(pharna).ToStringPercent()}" + "\n" + 
                       experienceLine + $": x{GetExperienceFactor(pharna).ToStringPercent()}";
            }
            catch (Exception ex)
            {
                Log.Error($"[FamiliaMyth] StatPart_PharnaWealth.ExplanationPart 失败：{ex}");
                return null;
            }
        }

        //等级归一化 level/10 线性，经验归一化做 smoothstep
        private static float GetLevelFactor(Hediff_Pharna pharna)
        {
            float tLevel = Mathf.Clamp01(pharna.level / 10f);
            return 1f + LevelMaxBonus * tLevel;
        }

        private static float GetExperienceFactor(Hediff_Pharna pharna)
        {
            float t = Mathf.Clamp01(pharna.experience / ExpCap);
            float e = t * t * (3f - 2f * t);
            return 1f + ExpMaxBonus * e;
        }

        private static float GetFactor(Hediff_Pharna pharna)
        {
            return GetLevelFactor(pharna) * GetExperienceFactor(pharna);
        }
    }
}
