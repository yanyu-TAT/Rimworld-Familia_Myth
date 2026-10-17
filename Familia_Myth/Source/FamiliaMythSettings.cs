namespace FamiliaMyth.Core
{
    /// <summary>
    /// 模组设置项：实验性内容开关
    /// </summary>
    public static class FamiliaMythSettings
    {
        private static FamiliaMythMod _mod;

        /// <summary>
        /// 公会任务系统（实验性内容）是否启用；设置尚未就绪时按关闭处理
        /// </summary>
        public static bool GuildContentEnabled => _mod?.SettingsData?.guildContent ?? false;

        /// <summary>
        /// 由模组主类构造时注入自身
        /// </summary>
        public static void Register(FamiliaMythMod owner) => _mod = owner;
    }
}
