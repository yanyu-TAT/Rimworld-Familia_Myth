using Verse;

namespace FamiliaMyth.Core
{
    /// <summary>
    /// 模组设置数据，由原版 Mod.GetSettings 读写到 Config 目录
    /// </summary>
    public class FamiliaMythSettingsData : ModSettings
    {
        public bool guildContent;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref guildContent, "guildContent", false);
        }
    }
}
