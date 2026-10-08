using UnityEngine;

namespace FamiliaMyth.UI
{
    //全局配色来源：容器三层底色、暖金强调、状态色、成长衰减色与稀有度四色
    public static class UI_Palette
    {
        //容器
        public static readonly Color ContentBg = new(47f / 255f, 48f / 255f, 55f / 255f, 0.98f);
        public static readonly Color TitleBarBg = new(14f / 255f, 16f / 255f, 39f / 255f);
        public static readonly Color CardBg = new(20f / 255f, 22f / 255f, 30f / 255f, 0.95f);
        public static readonly Color BarTrack = new(0.12f, 0.12f, 0.15f, 0.9f);

        //强调色：升华门槛刻度、评级文本、可升级提示
        public static readonly Color Accent = new(196f / 255f, 149f / 255f, 82f / 255f);

        //分区卡描边与顶部高光（比强调色淡）
        public static readonly Color SectionBorder = new(196f / 255f, 149f / 255f, 82f / 255f, 0.55f);

        //铭刻：已铭刻项的左侧色条、进度条与行底（比强调色更亮，同暖金族）
        public static readonly Color Inscribed = new(1f, 0.86f, 0.52f);

        //文本
        public static readonly Color TextNormal = new(0.8f, 0.8f, 0.9f);
        public static readonly Color TextSubtle = new(0.82f, 0.82f, 0.9f);
        //能力值等数值文本
        public static readonly Color TextValue = new(0.72f, 0.74f, 0.80f);

        //成长增速衰减终点色
        public static readonly Color Decay = new(1f, 0.72f, 0.45f);

        //状态
        public static readonly Color Ready = new(0.35f, 0.85f, 0.85f);
        public static readonly Color Warning = new(1f, 0.75f, 0.2f);
        public static readonly Color Disabled = new(0.45f, 0.45f, 0.5f);

        //稀有度
        public static readonly Color RarityCommon = new(0.62f, 0.72f, 0.82f);
        public static readonly Color RarityUncommon = new(0.35f, 0.95f, 0.55f);
        public static readonly Color RarityRare = new(1f, 0.75f, 0.2f);
        public static readonly Color RarityUnique = new(1f, 0.25f, 0.65f);

        //伟业档位（1~9 档）
        public static readonly Color[] TierColors =
        {
            new Color(0.78f, 0.80f, 0.86f), new Color(0.55f, 0.80f, 0.90f), new Color(0.45f, 0.87f, 0.67f),
            new Color(0.60f, 0.90f, 0.42f), new Color(0.92f, 0.88f, 0.40f), new Color(1.00f, 0.75f, 0.20f),
            new Color(1.00f, 0.55f, 0.15f), new Color(1.00f, 0.32f, 0.28f), new Color(1.00f, 0.15f, 0.55f)
        };

        //表格行交错底色
        public static readonly Color RowStripe = new(1f, 1f, 1f, 0.035f);

        public static Color TierColorOf(int tier)
            => TierColors[Mathf.Clamp(tier - 1, 0, TierColors.Length - 1)];
    }
}
