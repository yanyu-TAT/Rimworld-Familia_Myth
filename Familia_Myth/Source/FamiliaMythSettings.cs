using HugsLib.Settings;
using Verse;

namespace FamiliaMyth.Core
{
    /// <summary>
    /// 模组设置项：实验性内容开关（HugsLib 设置体系，入口为「选项 → 模组选项」）
    /// </summary>
    public static class FamiliaMythSettings
    {
        private const string GuildContentName = "EnableGuildContent";

        private static SettingHandle<bool> guildContent;

        /// <summary>
        /// 公会任务系统（实验性内容）是否启用；设置项尚未注册时按关闭处理
        /// </summary>
        public static bool GuildContentEnabled => guildContent?.Value ?? false;

        //由 FamiliaMythMod.Initialize 调用
        public static void Register(ModSettingsPack pack)
        {
            pack.EntryName = "FamiliaMyth.Settings.Category".Translate();

            guildContent = pack.GetHandle(GuildContentName,
                "FamiliaMyth.Settings.GuildContentLabel".Translate(),
                "FamiliaMyth.Settings.GuildContentDesc".Translate(),
                false);
        }
    }
}
