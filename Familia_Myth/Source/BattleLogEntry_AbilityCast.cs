using UnityEngine;
using Verse;
using Verse.Grammar;

namespace FamiliaMyth.Abilities
{
    /// <summary>
    /// 施法日志条目：记录施法者与所用的魔法 / 技能，
    /// 图标取自 Def 的 ICombatLogSource 配置。
    /// </summary>
    public class BattleLogEntry_AbilityCast : LogEntry
    {
        private Pawn casterPawn;
        private Def usedDef;
        private RulePackDef ruleDef;

        public BattleLogEntry_AbilityCast() { }

        public BattleLogEntry_AbilityCast(Pawn caster, Def usedDef, RulePackDef ruleDef)
        {
            casterPawn = caster;
            this.usedDef = usedDef;
            this.ruleDef = ruleDef;
        }

        public override bool Concerns(Thing t) => casterPawn != null && t == casterPawn;

        public override IEnumerable<Thing> GetConcerns()
        {
            if (casterPawn != null) yield return casterPawn;
        }

        public override Texture2D IconFromPOV(Thing pov) => (usedDef as ICombatLogSource)?.LogIcon;

        public override bool ShowInCompactView() => true;

        protected override string ToGameStringFromPOV_Worker(Thing pov, bool forceLog)
        {
            if (casterPawn == null || usedDef == null || ruleDef == null) return string.Empty;
            return base.ToGameStringFromPOV_Worker(pov, forceLog);
        }

        protected override GrammarRequest GenerateGrammarRequest()
        {
            GrammarRequest request = base.GenerateGrammarRequest();
            request.Includes.Add(ruleDef);
            request.Rules.AddRange(GrammarUtility.RulesForPawn("INITIATOR", casterPawn, request.Constants));
            //规则包只用 ABILITY_label，取带色的日志名
            string abilityLabel = (usedDef as ICombatLogSource)?.LogLabel ?? usedDef.label;
            request.Rules.Add(new Rule_String("ABILITY_label", abilityLabel));
            return request;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref casterPawn, "casterPawn", saveDestroyedThings: true);
            Scribe_Defs.Look(ref usedDef, "usedDef");
            Scribe_Defs.Look(ref ruleDef, "ruleDef");
        }

        public override string ToString()
        {
            return $"{ruleDef?.defName}: {casterPawn} ({usedDef?.defName})";
        }
    }
}
