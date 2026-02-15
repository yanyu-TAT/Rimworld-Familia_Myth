using FamiliaMyth.Core;
using RimWorld;
using Verse;

namespace FamiliaMyth.Pharna
{
    public enum StatPartMode{
        Offset,     //加算
        Factor,     //乘算
        ExpDecay    //指数衰减
    }
    public class StatPart_Pharna : StatPart
    {
        public PharnaStatusType statusType;
        public StatPartMode mode;
        public float a;

        private static HediffDef cachedPharnaDef;
        private static HediffDef PharnaDef => cachedPharnaDef ??= DefDatabase<HediffDef>.GetNamed("Hediff_Pharna");

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (!req.HasThing || !req.Thing.Spawned || !(req.Thing is Pawn pawn))
                return;
            if (pawn.health == null || pawn.health.hediffSet == null)
                return;

            if (pawn.health.hediffSet.GetFirstHediffOfDef(PharnaDef) is not Hediff_Pharna pharna) return;

            float status = pharna.GetFullStatus(statusType);
            switch (mode)
            {
                case StatPartMode.Offset:
                    val += a * status;break;
                case StatPartMode.Factor:
                    val *= 1 + a * status;break;
                case StatPartMode.ExpDecay:
                    val *= (float)Math.Pow(a, status);break;
            }
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (!req.HasThing || !req.Thing.Spawned || !(req.Thing is Pawn pawn))
                return null;
            if (pawn.health == null || pawn.health.hediffSet == null)
                return null;

            if (pawn.health.hediffSet.GetFirstHediffOfDef(PharnaDef) is not Hediff_Pharna pharna) return null;

            int fullStatus = (int) pharna.GetFullStatus(statusType);
            int curStatus = (int) pharna.GetCurrentStatus(statusType);
            string rank = Hediff_Pharna.GetRank(curStatus);
            int level = pharna.level;
            //说明文本，例：Lv.3 敏捷F399: +35.5%
            string s = "FamiliaMyth.Pharna".Translate() + "(Lv." + level.ToString() + " " + Hediff_Pharna.GetStatusName(statusType) + rank + curStatus.ToString() + "): ";

            //添加符号
            if (mode == StatPartMode.ExpDecay || mode == StatPartMode.Factor)
            {
                s += "x";
            }
            else if (a >= 0)
            {
                s += "+";
            } else
            {
                s += "-";
            }

            //计算数值
            switch (mode)
            {
                case StatPartMode.Offset:
                    s += (a * fullStatus).ToString(); break;
                case StatPartMode.Factor:
                    s += (1 + a * fullStatus).ToStringPercent(); break;
                case StatPartMode.ExpDecay:
                    s += ((float)Math.Pow(a, fullStatus)).ToStringPercent(); break;
            }

            return s;
        }
    }
}