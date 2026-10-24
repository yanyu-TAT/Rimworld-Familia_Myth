using RimWorld;
using Verse;

namespace FamiliaMyth.Abilities
{
    [DefOf]
    public static class FamiliaMythRulePackDefOf
    {
        public static RulePackDef FamiliaMyth_CastSpell;
        public static RulePackDef FamiliaMyth_CastSkill;
        public static RulePackDef FamiliaMyth_HitBySpell;
        public static RulePackDef FamiliaMyth_HitBySkill;

        static FamiliaMythRulePackDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(FamiliaMythRulePackDefOf));
        }
    }
}
