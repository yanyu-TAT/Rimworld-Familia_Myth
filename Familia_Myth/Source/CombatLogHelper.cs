using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    /// <summary>
    /// 战斗日志的 Def 侧配置：图标、施法行规则包、伤害行规则包与日志显示名。
    /// 由 SpellDef 与 SkillDef 实现。
    /// </summary>
    public interface ICombatLogSource
    {
        /// <summary>日志行左侧图标。</summary>
        Texture2D LogIcon { get; }

        /// <summary>施法行规则包。</summary>
        RulePackDef CastRule { get; }

        /// <summary>伤害行规则包，未配置时返回默认包。</summary>
        RulePackDef HitRule { get; }

        /// <summary>日志里显示的魔法 / 技能名，未配置时回落 label。</summary>
        string LogLabel { get; }
    }

    /// <summary>
    /// 战斗日志条目的统一构造入口。
    /// </summary>
    public static class CombatLogHelper
    {
        /// <summary>施法行：只记施法者与所用的魔法 / 技能。</summary>
        public static void WriteCastLog(Pawn caster, Def usedDef)
        {
            if (caster == null || usedDef is not ICombatLogSource source) return;
            Find.BattleLog.Add(new BattleLogEntry_AbilityCast(caster, usedDef, source.CastRule));
        }

        /// <summary>伤害行：按 Def 的配置生成，返回条目供 AssociateWithLog 回填。</summary>
        public static BattleLogEntry_MeleeCombat MakeHitLog(Pawn caster, Thing target, ICombatLogSource source)
        {
            if (caster == null || target == null || source == null) return null;
            BattleLogEntry_MeleeCombat log = new(
                source.HitRule, true, caster, target, null, source.LogLabel,
                null, null, LogEntryDefOf.MeleeAttack);
            Find.BattleLog.Add(log);
            return log;
        }

        /// <summary>伤害行：无 Def 上下文的伤害点用固定规则包与固定文案。</summary>
        public static BattleLogEntry_MeleeCombat MakeHitLog(Pawn caster, Thing target, RulePackDef rule, string label)
        {
            if (caster == null || target == null || rule == null || label.NullOrEmpty()) return null;
            BattleLogEntry_MeleeCombat log = new(
                rule, true, caster, target, null, label,
                null, null, LogEntryDefOf.MeleeAttack);
            Find.BattleLog.Add(log);
            return log;
        }
    }
}
