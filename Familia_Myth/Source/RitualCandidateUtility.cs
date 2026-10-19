using RimWorld;
using Verse;

namespace FamiliaMyth.Ritual
{
    /// <summary>
    /// 恩惠仪式角色的候选校验：只有文化结构为眷族神话的成员才能担任接受者。
    /// </summary>
    public static class RitualCandidateUtility
    {
        private static MemeDef meme;
        private static MemeDef FamiliaMeme =>
            meme ??= DefDatabase<MemeDef>.GetNamed("Structure_FamiliaMyth");

        public static bool IsFamiliaIdeoMember(Pawn pawn, out string reason, bool skipReason)
        {
            reason = null;
            if (pawn?.Ideo?.StructureMeme == FamiliaMeme)
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
