using FamiliaMyth.Pharna;
using HarmonyLib;
using RimWorld;
using Verse;

namespace FamiliaMyth.Patch
{
    [HarmonyPatch(typeof(Verb_MeleeAttack), "TryCastShot")]
    public static class Patch_Verb_MeleeAttack
    {
        private static HashSet<RulePackDef> dodgeRules;
        private static HashSet<RulePackDef> DodgeRules
        {
            get
            {
                if (dodgeRules == null)
                {
                    dodgeRules = new HashSet<RulePackDef>();
                    foreach(var rule in DefDatabase<ManeuverDef>.AllDefs)
                        if (rule.combatLogRulesDodge != null)
                            dodgeRules.Add(rule.combatLogRulesDodge);
                }
                return dodgeRules; 
            }
        }

        public static void Prefix(ref int __state)
        {
            var battles = Find.BattleLog.Battles;
            __state = battles.Count > 0 ? battles[0].Entries.Count : 0;
        }

        public static void Postfix(Verb_MeleeAttack __instance, bool __result, int __state)
        {
            if (__result) return;

            var battles = Find.BattleLog.Battles;
            if (battles.Count <= 0) return;

            var entries = battles[0].Entries;
            if (entries.Count <= __state) return;

            if (entries[entries.Count - 1] is BattleLogEntry_MeleeCombat log && DodgeRules.Contains(log.RuleDef))
            {
                if (__instance.CurrentTarget.Thing is Pawn pawn)
                {
                    //闪避成功每次5点
                    pawn.GetPharna()?.GainStatus(PharnaStatusType.Agility, 5f);
                }
            }
        }
    }
}