using FamiliaMyth.Pharna;
using RimWorld;
using System.Text;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class StatPart_SpellPower : StatPart
    {
        private const float ManaFactor = 0.001f;

        public override void TransformValue(StatRequest req, ref float val)
        {
            try { Transform(req, ref val); }
            catch { }

            static void Transform(StatRequest req, ref float val)
            {
                if (req.Thing is not Pawn pawn) return;
                var pharna = Hediff_Pharna.GetPharna(pawn);
                if (pharna == null) return;

                val += pharna.GetFullStatus(PharnaStatusType.Mana) * ManaFactor;

                ApplyModifiers(pharna, ref val);
            }
        }

        private static void ApplyModifiers(Hediff_Pharna pharna, ref float power)
        {
            if (pharna.skills != null)
                foreach (var s in pharna.skills)
                    if (s is ISpellPowerModifier m) 
                        m.ApplySpellPowerModifier(ref power);

            if (pharna.developableAbilities != null)
                foreach (var a in pharna.developableAbilities)
                    if (a is ISpellPowerModifier m) 
                        m.ApplySpellPowerModifier(ref power);

            if (pharna.pawn?.health?.hediffSet != null)
                foreach (var h in pharna.pawn.health.hediffSet.hediffs)
                    if (h is ISpellPowerModifier m) 
                        m.ApplySpellPowerModifier(ref power);
        }

        public override string ExplanationPart(StatRequest req)
        {
            try { return Explain(req); }
            catch { return ""; }

            static string Explain(StatRequest req)
            {
                if (req.Thing is not Pawn pawn) return "";
                var pharna = Hediff_Pharna.GetPharna(pawn);
                if (pharna == null) return "";

                var sb = new StringBuilder();

                // 魔力行（风格对齐 StatPart_Pharna：名称+评级+数值: +XX%）
                float mana = pharna.GetFullStatus(PharnaStatusType.Mana);
                string rank = Hediff_Pharna.GetRank(mana);
                sb.AppendLine(
                    $"Lv.{pharna.level} {Hediff_Pharna.GetStatusName(PharnaStatusType.Mana)}{rank}{(int)mana}: " +
                    $"+{(mana * ManaFactor).ToStringPercent()}");

                // 其余 ISpellPowerModifier 来源（按前后差值求单项加成）
                foreach (var line in GetModifierExplanationLines(pharna))
                    sb.AppendLine(line);

                return sb.ToString().TrimEnd();
            }
        }

        private static IEnumerable<string> GetModifierExplanationLines(Hediff_Pharna pharna)
        {
            float power = 0f;
            var sources = new List<(string label, ISpellPowerModifier mod)>();

            if (pharna.skills != null)
                foreach (var s in pharna.skills)
                    if (s is ISpellPowerModifier m) 
                        sources.Add((s.Label, m));

            if (pharna.developableAbilities != null)
                foreach (var a in pharna.developableAbilities)
                    if (a is ISpellPowerModifier m) 
                        sources.Add((a.Label, m));

            if (pharna.pawn?.health?.hediffSet != null)
                foreach (var h in pharna.pawn.health.hediffSet.hediffs)
                    if (h is ISpellPowerModifier m) 
                        sources.Add((h.LabelBase, m));

            foreach (var (label, mod) in sources)
            {
                float before = power;
                mod.ApplySpellPowerModifier(ref power);
                float delta = power - before;
                yield return $"{label}: +{delta.ToStringPercent()}";
            }
        }
    }
}
