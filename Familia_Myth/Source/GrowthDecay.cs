using UnityEngine;

namespace FamiliaMyth.Pharna
{
    /// <summary>
    /// 成长衰减预设：默认 / 低衰减由常量给出，自定义沿用玩家当前参数
    /// </summary>
    public enum GrowthDecayPreset
    {
        Default,
        Low,
        Custom
    }

    /// <summary>
    /// 成长衰减参数：起算阈值以上按反比衰减，比例由「达单级上限时的衰减率」反解。
    /// </summary>
    public static class GrowthDecay
    {
        /// <summary>单项能力值的单级上限</summary>
        public const float StatusCap = 999f;

        /// <summary>能力值项目数，单项上限乘以它即总和上限</summary>
        public const int StatusCount = 6;

        //默认预设：单项阈值 400、总和阈值 2000，达单级上限时衰减 50% / 60%
        public const float DefaultStatThreshold = 400f;
        public const float DefaultStatPeakRate = 0.5f;
        public const float DefaultTotalThreshold = 2000f;
        public const float DefaultTotalPeakRate = 0.6f;

        //低衰减预设：单项阈值 500、达单级上限时衰减 30%；总和高于上限且峰值率为 0，即不衰减
        public const float LowStatThreshold = 500f;
        public const float LowStatPeakRate = 0.3f;
        public const float LowTotalThreshold = 6000f;
        public const float LowTotalPeakRate = 0f;

        /// <summary>总和上限：六项各自的单级上限之和</summary>
        public static float TotalCap(float statusCap) => statusCap * StatusCount;

        /// <summary>
        /// 反解衰减比例：使取值达到 cap 时的衰减率恰为 peakRate。
        /// peakRate 为 0 时返回正无穷，代表整段不衰减。
        /// </summary>
        public static float Scale(float threshold, float peakRate, float cap)
        {
            float range = cap - threshold;
            if (range <= 0f || peakRate <= 0f)
            {
                return float.PositiveInfinity;
            }
            float rate = Mathf.Min(peakRate, 1f);
            return range * (1f - rate) / rate;
        }

        /// <summary>衰减系数：1 为不衰减，0 为完全不增长</summary>
        public static float Factor(float value, float threshold, float scale)
        {
            float excess = value - threshold;
            if (excess <= 0f)
            {
                return 1f;
            }
            if (scale <= 0f)
            {
                return 0f;
            }
            return 1f / (1f + excess / scale);
        }
    }
}
