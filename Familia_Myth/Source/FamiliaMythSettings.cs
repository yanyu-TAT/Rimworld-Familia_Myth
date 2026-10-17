using FamiliaMyth.Pharna;
using Verse;

namespace FamiliaMyth.Core
{
    /// <summary>
    /// 模组设置项：实验性内容开关与六维加成曲线
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
        /// 由模组主类构造时注入自身
        /// </summary>
        public static void Register(FamiliaMythMod owner) => _mod = owner;
    }
}
