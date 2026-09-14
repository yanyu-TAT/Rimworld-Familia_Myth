using FamiliaMyth.Pharna;
using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.UI
{
    /// <summary>
    /// 魔力池状态条：显示当前魔力与储量上限，右上角角标用于开关自动转化
    /// </summary>
    [StaticConstructorOnStartup]
    public class Gizmo_ManaPool : Gizmo
    {
        private const float PoolHeight = 75f;
        private const float ToggleSize = 26f;
        private const float MarkSize = 14f;

        private readonly Hediff_Pharna pharna;

        private static readonly Texture2D BarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.1f, 0.75f, 0.86f));
        private static readonly Texture2D EmptyBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.03f, 0.3f, 0.4f));
        private static readonly Texture2D ToggleTex = ContentFinder<Texture2D>.Get("Familia_Myth/UI/Commands/ManaAutoConvert", true);

        public Gizmo_ManaPool(Hediff_Pharna pharna)
        {
            this.pharna = pharna;
            Order = -90;
        }

        public override float GetWidth(float maxWidth) => 212f;

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect rect = new(topLeft.x, topLeft.y, GetWidth(maxWidth), PoolHeight);
            Rect inner = rect.ContractedBy(6f);
            Widgets.DrawWindowBackground(rect);

            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(inner.x, inner.y + 6f, inner.width, Text.LineHeight), "FamiliaMyth.Mana.PoolTitle".Translate());

            //魔力进度条
            float max = pharna.MaxMana;
            Rect bar = new(inner.x, inner.y + 32f, inner.width - 10f, 22f);
            Widgets.FillableBar(bar, max > 0f ? Mathf.Clamp01(pharna.manaPool / max) : 0f, BarTex, EmptyBarTex, doBorder: true);
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(bar, pharna.manaPool.ToString("F0") + " / " + max.ToString("F0"));
            Text.Anchor = TextAnchor.UpperLeft;

            TooltipHandler.TipRegion(bar, () => "FamiliaMyth.Mana.PoolTooltip".Translate(pharna.manaPool.ToString("F0"), max.ToString("F0")),
                Gen.HashCombineInt(pharna.GetHashCode(), 197305));

            //右上角角标：自动转化开关，右下角以勾/叉指示当前状态
            Rect toggle = new(rect.xMax - ToggleSize - 4f, rect.y + 4f, ToggleSize, ToggleSize);
            Color prevColor = GUI.color;
            GUI.color = pharna.autoConvert ? Color.white : Color.gray;
            if (Widgets.ButtonImage(toggle, ToggleTex))
                pharna.autoConvert = !pharna.autoConvert;
            GUI.color = prevColor;

            Rect mark = new(toggle.xMax - MarkSize, toggle.yMax - MarkSize, MarkSize, MarkSize);
            GUI.DrawTexture(mark, pharna.autoConvert ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex);

            TooltipHandler.TipRegion(toggle, "FamiliaMyth.Mana.AutoConvertTooltip".Translate());

            return new GizmoResult(GizmoState.Clear);
        }
    }
}
