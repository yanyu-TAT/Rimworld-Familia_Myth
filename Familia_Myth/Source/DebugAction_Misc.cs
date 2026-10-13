using FamiliaMyth.Abilities;
using LudeonTK;
using Verse;

namespace FamiliaMyth.Debug
{
    public static class DebugAction_Misc
    {
        /// <summary>
        /// 调试指令：把全部咏唱词烘焙成 PNG 落到 mod 的 Textures/Incantation/ 下。
        /// 1.7 的字体后端不提供中文字形，需在 1.6 下导出后随 mod 分发。
        /// </summary>
        [DebugAction("Familia Myth", "导出咏唱词纹理",
            actionType = DebugActionType.Action,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void DumpIncantationTextures()
        {
            IncantationTextureCache.DumpAllIncantationTextures();
        }
    }
}
