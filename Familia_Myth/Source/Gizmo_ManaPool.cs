using FamiliaMyth.Abilities;
using FamiliaMyth.Pharna;
using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.UI
{
    /// <summary>
    /// 魔力池状态条：显示当前魔力与储量上限，右上角角标用于开关自动转化；
    /// 悬停魔法图标时预览本次施法的魔力消耗
    /// </summary>
    [StaticConstructorOnStartup]
    public class Gizmo_ManaPool : Gizmo
    {
        private const float PoolHeight = 75f;
        private const float ToggleSize = 26f;
        private const float MarkSize = 14f;

        //施法预览段的闪烁节奏：周期 0.85s，其中 0.1s 淡入、0.25s 起淡出 0.6s
        private const float PreviewCycle = 0.85f;
        private const float PreviewFadeIn = 0.1f;
        private const float PreviewSolidUntil = 0.25f;
        private const float PreviewFadeOut = 0.6f;
        private const float PreviewAlphaScale = 0.7f;

        //预览段取条主色向白色插值的程度
        private const float PreviewTint = 0.9f;

        private readonly Hediff_Pharna pharna;

        private static readonly Color BarColor = new(0.1f, 0.75f, 0.86f);

        private static readonly Texture2D BarTex = SolidColorMaterials.NewSolidColorTexture(BarColor);
        private static readonly Texture2D EmptyBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.03f, 0.3f, 0.4f));
        private static readonly Texture2D ToggleTex = ContentFinder<Texture2D>.Get("Familia_Myth/UI/Commands/ManaAutoConvert", true);

        //预览段用条主色调亮，表示本次施法将消耗的部分
        private static readonly Texture2D CostPreviewTex = SolidColorMaterials.NewSolidColorTexture(Color.Lerp(BarColor, Color.white, PreviewTint));

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
            DrawCastPreview(bar, max);
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

        /// <summary>
        /// 在魔力条上标出鼠标下魔法将消耗的魔力；魔力不足时改用警示色
        /// </summary>
        private void DrawCastPreview(Rect bar, float max)
        {
            if (max <= 0f) return;
            if (MapGizmoUtility.LastMouseOverGizmo is not Command_Ability command) return;
            if (command.Pawn != pharna.pawn) return;
            if (command.Ability.def is not SpellDef spellDef || spellDef.manaCost <= float.Epsilon) return;

            float current = Mathf.Clamp01(pharna.manaPool / max);
            float cost = Mathf.Min(spellDef.manaCost / max, current);
            if (cost <= 0f) return;

            Rect preview = bar.ContractedBy(3f);
            float width = preview.width;
            float end = preview.xMin + width * current;
            preview.xMin = end - width * cost;
            preview.xMax = end;

            GUI.color = new Color(1f, 1f, 1f, PreviewAlpha() * PreviewAlphaScale);
            GenUI.DrawTextureWithMaterial(preview, CostPreviewTex, null);
            GUI.color = Color.white;
        }

        /// <summary>
        /// 预览段的闪烁透明度
        /// </summary>
        private static float PreviewAlpha()
        {
            float t = Mathf.Repeat(Time.time, PreviewCycle);
            if (t < PreviewFadeIn) return t / PreviewFadeIn;
            if (t >= PreviewSolidUntil) return 1f - (t - PreviewSolidUntil) / PreviewFadeOut;
            return 1f;
        }
    }
}
