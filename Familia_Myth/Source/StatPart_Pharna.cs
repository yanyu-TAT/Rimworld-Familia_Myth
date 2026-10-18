using RimWorld;
using Verse;

namespace FamiliaMyth.Pharna
{
    public enum StatPartMode
    {
        Offset,     //加算
        Factor      //乘算
    }

    public class StatPart_Pharna : StatPart
    {
        public PharnaStatusType statusType;
        public StatPartMode mode;
        public float a;
        public bool displayAsPercent = true;

        public override void TransformValue(StatRequest req, ref float val)
        {
            try
            {
                Transform(req, ref val);
            }
            catch { }

            void Transform(StatRequest req, ref float val)
            {
                if (!req.HasThing || req.Thing is not Pawn pawn)
                    return;

                var pharna = pawn.GetPharna();
                if (pharna == null) return;

                float processedStatus = PharnaCurve.Sample(PharnaCurve.GetCurve(statusType), pharna.GetFinalStatus(statusType));
                switch (mode)
                {
                    case StatPartMode.Offset:
                        val += a * processedStatus; break;
                    case StatPartMode.Factor:
                        val *= 1 + a * processedStatus; break;
                }
            }
        }

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
                if (!req.HasThing || req.Thing is not Pawn pawn)
                    return null;

                var pharna = pawn.GetPharna();
                if (pharna == null) return "";

                int finalStatus = (int)pharna.GetFinalStatus(statusType);
                int curStatus = (int)pharna.GetCurrentStatus(statusType);
                int processedStatus = (int)PharnaCurve.Sample(PharnaCurve.GetCurve(statusType), finalStatus);
                string rank = Hediff_Pharna.GetRank(curStatus);
                int level = pharna.level;
                //说明文本，例：Lv.3 敏捷F399: +35.5%    （仅使用当前值）
                string s = "  Lv." + level.ToString() + " " + Hediff_Pharna.GetStatusName(statusType) + rank + curStatus.ToString() + ": ";

                //添加符号
                if (mode == StatPartMode.Factor)
                {
                    s += "x";
                }
                else if (a >= 0)
                {
                    s += "+";
                }
                else
                {
                    s += "-";
                }

                //计算数值（使用处理后的值）
                switch (mode)
                {
                    case StatPartMode.Offset:
                        s += displayAsPercent
                            ? (a * processedStatus).ToStringPercent()
                            : (a * processedStatus).ToString("G3");
                        break;
                    case StatPartMode.Factor:
                        s += displayAsPercent
                            ? (1 + a * processedStatus).ToStringPercent()
                            : (1 + a * processedStatus).ToString("G3");
                        break;
                }

                return s;
            }
        }
    }
}
