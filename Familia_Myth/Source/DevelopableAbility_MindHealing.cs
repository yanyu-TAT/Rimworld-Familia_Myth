using FamiliaMyth.Pharna;
using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    /// <summary>
    /// 精愈发展能力：持续自动恢复精神力，恢复速度随当前精神力线性下降、与熟练度成正比。
    /// </summary>
    public class DevelopableAbility_MindHealing : DevelopableAbility
    {
        //标定熟练度：500 熟练度时取转化消耗的 50%
        private const float ReferenceProficiency = 500f;
        private const float ReferenceRateFactor = 0.5f;
        //每 1% 精神力消耗 -> 熟练度
        public const float ProfGainPerPsyfocusPercent = 0.005f;

        //最大恢复速度（当前精神力为 0 时），单位：每秒
        public float MaxRestorePerSecond
            => Hediff_Pharna.PsyfocusPerSecond * ReferenceRateFactor * proficiency / ReferenceProficiency;

        //精愈不通过 StatPart 注入任何 StatDef
        public override bool ApplyModifiers(StatDef stat, ref float value) => false;

        public override string GetExplanation(StatDef stat) => "";

        public override void Tick()
        {
            Pawn pawn = pharna?.pawn;
            if (pawn == null || !pawn.Spawned) return;
            if (!pawn.IsHashIntervalTick(60)) return;

            Pawn_PsychicEntropyTracker tracker = pawn.psychicEntropy;
            if (tracker == null) return;
            if (!pawn.HasPsylink || !tracker.NeedsPsyfocus) return;

            float current = tracker.CurrentPsyfocus;
            if (current < 0f || current >= 1f) return;

            float restore = MaxRestorePerSecond * (1f - current);
            if (restore <= 0f) return;
            tracker.OffsetPsyfocusDirectly(restore);
        }
    }

    public static class MindHealingUtility
    {
        public static DevelopableAbility_MindHealing GetMindHealing(this Pawn pawn)
        {
            Hediff_Pharna pharna = pawn?.GetPharna();
            if (pharna?.developableAbilities == null) return null;
            foreach (DevelopableAbility ability in pharna.developableAbilities)
                if (ability is DevelopableAbility_MindHealing mindHealing)
                    return mindHealing;
            return null;
        }
    }
}
