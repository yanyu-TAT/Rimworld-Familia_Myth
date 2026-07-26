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
        public bool displayAsPercent = true;

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (!req.HasThing || !req.Thing.Spawned || !(req.Thing is Pawn pawn))
                return;

            var pharna = Hediff_Pharna.GetPharna(pawn);
            if (pharna == null) return;

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

            var pharna = Hediff_Pharna.GetPharna(pawn);
            if (pharna == null) return "";

            int fullStatus = (int) pharna.GetFullStatus(statusType);
            int curStatus = (int) pharna.GetCurrentStatus(statusType);
            string rank = Hediff_Pharna.GetRank(curStatus);
            int level = pharna.level;
            //说明文本，例：Lv.3 敏捷F399: +35.5%
            string s = "Lv." + level.ToString() + " " + Hediff_Pharna.GetStatusName(statusType) + rank + curStatus.ToString() + ": ";

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
                    s += displayAsPercent
                        ? (a * fullStatus).ToStringPercent()
                        : (a * fullStatus).ToString("G3");
                    break;
                case StatPartMode.Factor:
                    s += displayAsPercent
                        ? (1 + a * fullStatus).ToStringPercent()
                        : (1 + a * fullStatus).ToString("G3");
                    break;
                case StatPartMode.ExpDecay:
                    s += displayAsPercent
                        ? ((float)Math.Pow(a, fullStatus)).ToStringPercent()
                        : ((float)Math.Pow(a, fullStatus)).ToString("G3");
                    break;
            }

            return s;
        }
    }
}