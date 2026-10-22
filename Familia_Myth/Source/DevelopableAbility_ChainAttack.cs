using FamiliaMyth.Pharna;
using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    /// <summary>
    /// 连攻发展能力：按最近 10 秒的近战出手频率为力量与灵巧提供补正。
    /// </summary>
    public class DevelopableAbility_ChainAttack : DevelopableAbility, IStatusModifier
    {
        //攻击统计窗口（10 秒）
        private const int WindowTicks = 600;
        //力量/灵巧补正封顶
        private const float MaxBonus = 750f;
        //标定攻速（次/秒）
        private const float ReferenceRate = 2.5f;
        //近战技能经验 -> 发展能力熟练度
        public const float ProfGainPerXp = 0.005f;

        private readonly List<int> attackTicks = new();

        //连攻不通过 StatPart 注入任何 StatDef
        public override bool ApplyModifiers(StatDef stat, ref float value) => false;

        public override string GetExplanation(StatDef stat) => "";

        //记录一次近战出手
        public void Notify_MeleeAttack()
        {
            attackTicks.Add(Find.TickManager.TicksGame);
        }

        public float GetAttackRate()
        {
            return attackTicks.Count / (WindowTicks / 60f);
        }

        //补正与攻速、熟练度均成正比，封顶 MaxBonus
        public float GetBonus()
        {
            return Mathf.Min(MaxBonus, GetAttackRate() * proficiency / ReferenceRate);
        }

        public override void Tick()
        {
            if (attackTicks.Count == 0) return;
            int oldest = Find.TickManager.TicksGame - WindowTicks;
            while (attackTicks.Count > 0 && attackTicks[0] <= oldest)
                attackTicks.RemoveAt(0);
        }

        public void ApplyStatusModifier(PharnaStatusType type, ref float val)
        {
            if (type != PharnaStatusType.Strength && type != PharnaStatusType.Dexterity) return;
            val += GetBonus();
        }

        public string GetExplanation(PharnaStatusType type)
        {
            if (type != PharnaStatusType.Strength && type != PharnaStatusType.Dexterity)
                return "";
            float bonus = GetBonus();
            if (bonus <= 0f) return "";
            return "FamiliaMyth.Ability.ChainAttack".Translate(
                GetAttackRate().ToString("F1"), bonus.ToString("F0"));
        }
    }

    public static class ChainAttackUtility
    {
        public static DevelopableAbility_ChainAttack GetChainAttack(this Pawn pawn)
        {
            Hediff_Pharna pharna = pawn?.GetPharna();
            if (pharna?.developableAbilities == null) return null;
            foreach (DevelopableAbility ability in pharna.developableAbilities)
                if (ability is DevelopableAbility_ChainAttack chainAttack)
                    return chainAttack;
            return null;
        }
    }
}
