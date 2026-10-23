using FamiliaMyth.Abilities;
using FamiliaMyth.Pharna;
using RimWorld;
using Verse;

namespace FamiliaMyth.Feedback
{
    //成长反馈的统一出口：档位提升消息集中在此，音效由 MessageTypeDef 自带
    public static class GrowthFeedback
    {
        //能力值档位提升：一次调用只发一条，报最终档
        public static void NotifyStatusRankUp(Hediff_Pharna pharna, PharnaStatusType type, int prevRank, int newRank)
        {
            if (newRank <= prevRank) return;

            Pawn pawn = pharna?.pawn;
            if (pawn == null) return;

            Messages.Message(
                "FamiliaMyth.Growth.StatusRankUp".Translate(
                    pawn.Name?.ToStringShort ?? pawn.LabelShort,
                    Hediff_Pharna.GetStatusName(type),
                    Hediff_Pharna.RankNameAt(newRank)),
                pawn, MessageTypeDefOf.PositiveEvent, historical: false);
        }

        //发展能力档位提升：一次调用只发一条，报最终档
        public static void NotifyDegreeRankUp(Hediff_Pharna pharna, DevelopableAbility ability, int prevDegree, int newDegree)
        {
            if (newDegree <= prevDegree || ability == null) return;

            Pawn pawn = pharna?.pawn;
            if (pawn == null) return;

            Messages.Message(
                "FamiliaMyth.Growth.DegreeRankUp".Translate(
                    pawn.Name?.ToStringShort ?? pawn.LabelShort,
                    ability.Label,
                    Hediff_Pharna.RankNameAt(newDegree)),
                pawn, MessageTypeDefOf.PositiveEvent, historical: false);
        }
    }
}
