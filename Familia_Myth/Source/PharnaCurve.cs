using UnityEngine;
using Verse;

namespace FamiliaMyth.Pharna
{
    /// <summary>
    /// 六维能力值的加成曲线：把原始能力值折算为系数计算用的有效值。
    /// 曲线只决定「能力值如何折算成效果强度」，效果本身仍是线性系数（val += a * s 或 val *= 1 + a * s）。
    /// </summary>
    public static class PharnaCurve
    {
        /// <summary>定义域上限，对应能力值满值。</summary>
        public const float CurveMax = 10000f;

        /// <summary>默认曲线：满值 10000 锚定，轻微前缓后急。</summary>
        private static readonly SimpleCurve Default = new(new[]
        {
            new CurvePoint(0f, 0f),
            new CurvePoint(3333f, 2990f),
            new CurvePoint(6667f, 6400f),
            new CurvePoint(CurveMax, CurveMax),
        });

        /// <summary>恒等直线：即 y = x</summary>
        private static readonly SimpleCurve Identity = new(new[]
        {
            new CurvePoint(0f, 0f),
            new CurvePoint(CurveMax, CurveMax),
        });

        public static SimpleCurve GetCurve(PharnaStatusType statusType)
        {
            //目前全部使用默认值（魔力的默认曲线为 y = x）
            return statusType == PharnaStatusType.Mana ? Identity : Default;
        }

        /// <summary>把原始能力值折算为曲线有效值；定义域外取端点值。</summary>
        public static float Sample(SimpleCurve curve, float rawStatus)
        {
            float y = curve.Evaluate(Mathf.Clamp(rawStatus, 0f, CurveMax));
            return y < 0f ? 0f : y;
        }
    }
}
