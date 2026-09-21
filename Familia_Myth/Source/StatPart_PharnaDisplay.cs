using FamiliaMyth.Pharna;
using RimWorld;
using System.Text;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class StatPart_PharnaDisplay : StatPart
    {
        private static StatDef cachedManaConversionDef;
        private static StatDef ManaConversionDef => cachedManaConversionDef ??= DefDatabase<StatDef>.GetNamed("ManaConversion");

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
                PharnaDisplayMode.ManaConversionSpeed => GetManaConversionSpeedExplanation(pharna),
                _ => ""
            };
        }

        private string GetManaConversionSpeedExplanation(Hediff_Pharna pharna)
        {
            var pawn = pharna.pawn;
            float efficiency = pawn.GetStatValue(ManaConversionDef);
            float consciousness = pawn.health?.capacities?.GetLevel(PawnCapacityDefOf.Consciousness) ?? 1f;
            return "FamiliaMyth.Stat.ManaConversionSpeed".Translate(
                efficiency.ToStringPercent(), 
                consciousness.ToStringPercent(), 
                pharna.ManaConversionSpeed.ToString("F1"));
        }

        private string GetExploitsExplanation(Hediff_Pharna pharna)
        {
            return "FamiliaMyth.Stat.PharnaExploits".Translate((int)pharna.exploits, pharna.level);
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

            return "FamiliaMyth.Stat.PharnaExperience".Translate(sb.ToString().TrimEnd());
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
            string status = "FamiliaMyth.Stat.PharnaStatus".Translate((int)cur, (int)pot, (int)full);
            s += status;

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
                PharnaDisplayMode.ManaConversionSpeed => pharna.ManaConversionSpeed,
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
        Exploits,
        ManaConversionSpeed
    }
}
