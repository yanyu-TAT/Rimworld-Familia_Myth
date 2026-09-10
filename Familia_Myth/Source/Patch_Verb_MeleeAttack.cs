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
            try
            {
                var battles = Find.BattleLog?.Battles;
                __state = battles != null && battles.Count > 0 ? battles[0].Entries.Count : 0;
            }
            catch (Exception ex)
            {
                //置无效值，令 Postfix 直接跳过本次结算
                __state = -1;
                Log.Error($"[FamiliaMyth] {nameof(Patch_Verb_MeleeAttack)} prefix error: {ex}");
            }
        }

        public static void Postfix(Verb_MeleeAttack __instance, bool __result, int __state)
        {
            try
            {
                if (__state < 0) return;
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
            catch (Exception ex)
            {
                Log.Error($"[FamiliaMyth] {nameof(Patch_Verb_MeleeAttack)} postfix error: {ex}");
            }
        }
    }
}