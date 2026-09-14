using FamiliaMyth.Pharna;
using RimWorld;
using System.Text;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class StatPart_ManaControl : StatPart
    {
        public SpellType type = SpellType.None;

        public override void TransformValue(StatRequest req, ref float val)
        {
            try { Transform(req, ref val); }
            catch { }

            void Transform(StatRequest req, ref float val)
            {
                if (req.Thing is not Pawn pawn) return;
                var pharna = pawn.GetPharna();
                if (pharna == null) return;

                ApplyModifiers(pharna, ref val);
            }
        }

        private void ApplyModifiers(Hediff_Pharna pharna, ref float power)
        {
            if (pharna.skills != null)
                foreach (var s in pharna.skills)
                    if (s is IManaControlModifier m && m.ShouldApplyManaControl(type)) 
                        m.ApplyManaControlModifier(ref power);

            if (pharna.developableAbilities != null)
                foreach (var a in pharna.developableAbilities)
                    if (a is IManaControlModifier m && m.ShouldApplyManaControl(type)) 
                        m.ApplyManaControlModifier(ref power);

            if (pharna.pawn?.health?.hediffSet?.hediffs != null)
                foreach (var h in pharna.pawn.health.hediffSet.hediffs)
                    if (h is HediffWithComps hc)
                        foreach(var c in hc.comps)
                            if (c is IManaControlModifier m && m.ShouldApplyManaControl(type))
                                m.ApplyManaControlModifier(ref power);
        }

        public override string ExplanationPart(StatRequest req)
        {
            try { return Explain(req); }
            catch { return ""; }

            string Explain(StatRequest req)
            {
                if (req.Thing is not Pawn pawn) return "";
                var pharna = pawn.GetPharna();
                if (pharna == null) return "";

                var sb = new StringBuilder();

                //法强来源
                foreach (var line in GetModifierExplanationLines(pharna))
                    sb.AppendLine(line);

                return sb.ToString().TrimEnd();
            }
        }

        private IEnumerable<string> GetModifierExplanationLines(Hediff_Pharna pharna)
        {
            float power = 1f;
            var sources = new List<(string label, IManaControlModifier mod)>();

            if (pharna.skills != null)
                foreach (var s in pharna.skills)
                    if (s is IManaControlModifier m && m.ShouldApplyManaControl(type)) 
                        sources.Add((s.Label, m));

            if (pharna.developableAbilities != null)
                foreach (var a in pharna.developableAbilities)
                    if (a is IManaControlModifier m && m.ShouldApplyManaControl(type)) 
                        sources.Add((a.Label, m));

            if (pharna.pawn?.health?.hediffSet?.hediffs != null)
                foreach (var h in pharna.pawn.health.hediffSet.hediffs)
                    if (h is HediffWithComps hc)
                        foreach (var c in hc.comps)
                            if (c is IManaControlModifier m && m.ShouldApplyManaControl(type))
                                sources.Add((h.LabelBase, m));

            foreach (var (label, mod) in sources)
            {
                float before = power;
                mod.ApplyManaControlModifier(ref power);
                float delta = power - before;
                yield return $"\t{label}: +{delta.ToStringPercent()}";
            }
        }
    }
}
