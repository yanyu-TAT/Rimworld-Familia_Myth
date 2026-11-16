using FamiliaMyth.Pharna;
using Verse;

namespace FamiliaMyth.Core
{
    /// <summary>
    /// 模组设置项：实验性内容开关、六维加成曲线与成长衰减参数
    /// </summary>
    public static class FamiliaMythSettings
    {
        private static FamiliaMythMod _mod;

        /// <summary>
        /// 模组设置数据；设置尚未就绪时为 null
        /// </summary>
        public static FamiliaMythSettingsData Data => _mod?.SettingsData;

        /// <summary>
        /// 公会任务系统（实验性内容）是否启用；设置尚未就绪时按关闭处理
        /// </summary>
        public static bool GuildContentEnabled => Data?.guildContent ?? false;

        /// <summary>
        /// 教程信件是否启用；设置尚未就绪时按开启处理
        /// </summary>
        public static bool TutorialLettersEnabled => Data?.tutorialLetters ?? true;

        /// <summary>
        /// 六维之一当前生效的加成曲线；设置尚未就绪时返回 null
        /// </summary>
        public static SimpleCurve Curve(PharnaStatusType statusType) => Data?.GetCurve(statusType);

        /// <summary>
        /// 把六维之一恢复为内置默认曲线
        /// </summary>
        public static void ResetCurve(PharnaStatusType statusType) => Data?.ResetCurve(statusType);

        /// <summary>
        /// 把六维曲线全部恢复为内置默认值
        /// </summary>
        public static void ResetAllCurves() => Data?.ResetAllCurves();

        /// <summary>
        /// 当前生效的成长衰减预设；设置尚未就绪时按默认预设处理
        /// </summary>
        public static GrowthDecayPreset DecayPreset => Data?.decayPreset ?? GrowthDecayPreset.Default;

        /// <summary>
        /// 单项衰减的起始阈值；设置尚未就绪时用默认预设
        /// </summary>
        public static float StatDecayThreshold => Data?.statDecayThreshold ?? GrowthDecay.DefaultStatThreshold;

        /// <summary>
        /// 单项达单级上限时的衰减率；设置尚未就绪时用默认预设
        /// </summary>
        public static float StatDecayPeakRate => Data?.statDecayPeakRate ?? GrowthDecay.DefaultStatPeakRate;

        /// <summary>
        /// 总和衰减的起始阈值；设置尚未就绪时用默认预设
        /// </summary>
        public static float TotalDecayThreshold => Data?.totalDecayThreshold ?? GrowthDecay.DefaultTotalThreshold;

        /// <summary>
        /// 总和达单级上限时的衰减率；设置尚未就绪时用默认预设
        /// </summary>
        public static float TotalDecayPeakRate => Data?.totalDecayPeakRate ?? GrowthDecay.DefaultTotalPeakRate;

        /// <summary>
        /// 套用成长衰减预设
        /// </summary>
        public static void ApplyDecayPreset(GrowthDecayPreset preset) => Data?.ApplyDecayPreset(preset);

        /// <summary>
        /// 由模组主类构造时注入自身
        /// </summary>
        public static void Register(FamiliaMythMod owner) => _mod = owner;
    }
}
