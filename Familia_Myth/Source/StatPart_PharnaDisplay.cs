using FamiliaMyth.Pharna;
using RimWorld;
using System.Text;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class StatPart_PharnaDisplay : StatPart
    {
        public PharnaDisplayMode mode;
        public PharnaStatusType statusType;

        public override string ExplanationPart(StatRequest req)
        {
            if (req.Thing is not Pawn pawn) return null;
            var pharna = pawn.GetPharna();
            if (pharna == null) return "";

            return mode switch
            {
                PharnaDisplayMode.Status => GetStatusExplanation(pharna),
                PharnaDisplayMode.Experience => GetExperienceExplanation(pharna),
                PharnaDisplayMode.Exploits => GetExploitsExplanation(pharna),
                _ => ""
            };
        }

        private string GetExploitsExplanation(Hediff_Pharna pharna)
        {
            return $"当前伟业值: {(int)pharna.exploits} / 999\n"
                 + $"恩惠等级: Lv.{pharna.level}";
        }

        private string GetExperienceExplanation(Hediff_Pharna pharna)
        {
            bool first = true;
            var sb = new StringBuilder();
            foreach (var a in Enum.GetValues(typeof(PharnaStatusType)))
            {
                var type = (PharnaStatusType)a;
                string s = "  " + (first ? " " : "+");
                if (first)
                    first = false;
                s += $"{(int)pharna.GetCurrentStatus(type)}({Hediff_Pharna.GetStatusName(type)})";
                sb.AppendLine(s);
            }

            return $"当前经验值 =\n" + sb.ToString().TrimEnd();
        }

        private string GetStatusExplanation(Hediff_Pharna pharna)
        {
            float cur = pharna.GetCurrentStatus(statusType);
            float full = pharna.GetFullStatus(statusType);
            _ = pharna.GetFinalStatus(statusType, out var explanation);
            float pot = full - cur;
            string rank = Hediff_Pharna.GetRank(cur);
            string typeName = Hediff_Pharna.GetStatusName(statusType);
            int level = pharna.level;

            string s = $"Lv.{level} {typeName}{rank}{(int)cur}\n\n";
            s += $"当前: {(int)cur} | 潜在: {(int)pot}\n" +
                 $"合计: {(int)full}\n";

            s += explanation;

            return s;
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (req.Thing is not Pawn pawn) return;
            var pharna = pawn.GetPharna();
            if (pharna == null) return;

            val = mode switch
            {
                PharnaDisplayMode.Status => pharna.GetFinalStatus(statusType),
                PharnaDisplayMode.Experience => pharna.experience,
                PharnaDisplayMode.Exploits => pharna.exploits,
                _ => -1f
            };
        }

        public override bool ForceShow(StatRequest req)
        {
            return req.Thing is Pawn pawn && pawn.GetPharna() != null;
        }
    }

    public enum PharnaDisplayMode
    {
        Status,
        Experience,
        Exploits
    }
}
