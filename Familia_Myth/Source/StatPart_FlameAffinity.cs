using FamiliaMyth.Core;
using FamiliaMyth.Pharna;
using RimWorld;
using Verse;

namespace FamiliaMyth.Abilities
{
    public enum FlameAffinityStatType
    {
        ArmorHeat,
        ComfyTemp,
        Flammability
    }

    public class StatPart_FlameAffinity : StatPart
    {
        public FlameAffinityStatType statType;

        public override string ExplanationPart(StatRequest req)
        {
            try { return Explain(req); }
            catch { return ""; }
            string Explain(StatRequest req)
            {
                if (req.Thing is not Pawn pawn) return "";
                var pharna = Hediff_Pharna.GetPharna(pawn);
                if (pharna?.skills?.OfType<Skill_FlameAffinity>().Any() != true) return "";
                string s = "技能 火焰亲和: ";
                switch (statType)
                {
                    case FlameAffinityStatType.ArmorHeat:
                        s += "+200%";
                        break;
                    case FlameAffinityStatType.ComfyTemp:
                        s += "1000℃";
                        break;
                    case FlameAffinityStatType.Flammability:
                        s += "+100%";
                        break;
                }
                return s;
            }
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            try { Transform(req, ref val); }
            catch { }
            void Transform(StatRequest req, ref float val)
            {
                if (req.Thing is not Pawn pawn) return;
                var pharna = Hediff_Pharna.GetPharna(pawn);
                if (pharna?.skills?.OfType<Skill_FlameAffinity>().Any() != true) return;
                switch (statType)
                {
                    case FlameAffinityStatType.ArmorHeat:
                        val += 2f;      //热能护甲+200%
                        break;
                    case FlameAffinityStatType.ComfyTemp:
                        val += 1000f;   //最高舒适温度+1000℃
                        break;
                    case FlameAffinityStatType.Flammability:
                        val += 1f;      //易燃性+100%
                        break;
                }
            }
        }
    }
}
