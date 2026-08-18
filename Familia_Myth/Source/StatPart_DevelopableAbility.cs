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
            try
            {
                return GetExplanation(req);
            }
            catch
            {
                return "";
            }

            string GetExplanation(StatRequest req)
            {
                if (!req.HasThing || !req.Thing.Spawned || req.Thing is not Pawn pawn)
                    return "";

                var pharna = pawn.GetPharna();
                if (pharna?.developableAbilities == null) return "";

                var sb = new StringBuilder();
                foreach (var ability in pharna.developableAbilities)
                {
                    var line = ability.GetExplanation(parentStat);
                    if (line.NullOrEmpty()) continue;
                    sb.AppendLine(line);
                }
                return sb.ToString().TrimEnd();
            }
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            try
            {
                Transform(req, ref val);
            }
            catch { }

            void Transform(StatRequest req, ref float val)
            {
                if (!req.HasThing || !req.Thing.Spawned || req.Thing is not Pawn pawn)
                    return;

                var pharna = pawn.GetPharna();
                if (pharna?.developableAbilities == null) return;

                foreach (var ability in pharna.developableAbilities)
                    ability.ApplyModifiers(parentStat, ref val);
                return;
            }
        }
    }
}