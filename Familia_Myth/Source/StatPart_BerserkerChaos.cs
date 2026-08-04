using FamiliaMyth.Core;
using FamiliaMyth.Pharna;
using RimWorld;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class StatPart_BerserkerChaos : StatPart
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

            static string GetExplanation(StatRequest req)
            {
                if (req.Thing is Pawn pawn)
                {
                    var pharna = Hediff_Pharna.GetPharna(pawn);
                    if (pharna?.skills == null)
                        return "";
                    foreach (var skill in pharna.skills)
                        if (skill is Skill_BerserkerChaos berserkerChaos)
                        {
                            var x = berserkerChaos.GetDamageFactor(pawn);
                            return TranslateHelper.Translate("FamiliaMyth.BerserkerChaosLabel") + ": +" + (1.5f * x * x).ToStringPercent();
                        }
                }
                return "";
            }
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            try
            {
                Transform(req, ref val);
            }
            catch { }

            static void Transform(StatRequest req, ref float val)
            {
                if (req.Thing is Pawn pawn)
                {
                    var pharna = Hediff_Pharna.GetPharna(pawn);
                    if (pharna?.skills == null)
                        return;
                    foreach (var skill in pharna.skills)
                        if (skill is Skill_BerserkerChaos berserkerChaos)
                        {
                            var x = berserkerChaos.GetDamageFactor(pawn);
                            val *= 1f + 1.5f * x * x;
                            break;
                        }
                }

                return;
            }
        }
    }
}