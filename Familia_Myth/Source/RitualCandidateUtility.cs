using RimWorld;
using Verse;

namespace FamiliaMyth.Ritual
{
    /// <summary>
    /// 恩惠仪式角色的候选校验：只有玩家当前文化的成员才能担任接受者。
    /// </summary>
    internal static class RitualCandidateUtility
    {
        public static bool IsPlayerIdeoMember(Pawn pawn, out string reason, bool skipReason)
        {
            reason = null;
            if (pawn?.Ideo != null && pawn.Ideo == Find.FactionManager?.OfPlayer?.ideos?.PrimaryIdeo)
            {
                return true;
            }
            if (!skipReason)
            {
                reason = "FamiliaMyth.NotFamiliaMemberRefusion".Translate();
            }
            return false;
        }
    }
}
