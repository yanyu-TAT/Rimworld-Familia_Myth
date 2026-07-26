using FamiliaMyth.Pharna;
using RimWorld;
using System.Text;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class StatPart_DevelopableAbility : StatPart
    {
        public override string ExplanationPart(StatRequest req)
        {
            if (!req.HasThing || !req.Thing.Spawned || !(req.Thing is Pawn pawn))
                return "";

            var pharna = Hediff_Pharna.GetPharna(pawn);
            if (pharna?.developableAbilities == null) return "";

            var sb = new StringBuilder();
            foreach (var ability in pharna.developableAbilities)
            {
                var line = ability.GetExplanation(pharna, parentStat);
                if (line.NullOrEmpty()) continue;
                sb.AppendLine(line); 
            }
            return sb.ToString().TrimEnd();
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (!req.HasThing || !req.Thing.Spawned || !(req.Thing is Pawn pawn))
                return;

            var pharna = Hediff_Pharna.GetPharna(pawn);
            if (pharna?.developableAbilities == null) return;

            foreach (var ability in pharna.developableAbilities)
                ability.ApplyModifiers(pharna, parentStat, ref val);
        }
    }
}