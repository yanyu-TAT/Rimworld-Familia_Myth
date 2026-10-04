using FamiliaMyth.Abilities;
using FamiliaMyth.Feat;
using FamiliaMyth.Pharna;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace FamiliaMyth.UI
{
    //恩惠页签：能力 / 发展能力 / 技能·魔法 / 伟业 四个分区
    //分区卡统一 浅褐色描边 + 灰黑填充；条目小卡按稀有度（发展能力/技能/魔法）或伟业档位着色
    public class ITab_Pawn_Pharna : ITab
    {
        private const float TabWidth = 630f;
        private const float TabHeight = 510f;
        private const float SectionGap = 8f;
        private const float RowHeight = 22f;
        private const float SmallCardH = 20f;
        private const float SmallCardGap = 2f;
        private const float FeatCardH = 62f;
        private const float FeatCardGap = 4f;

        //能力行三段宽度：名称 / 数值 / 潜在
        private const float W_Name = 52f;
        private const float W_Num = 96f;
        private const float W_Pot = 92f;

        //分区卡统一描边色 浅棕
        private static readonly Color SectionBorder = new(196f / 255f, 149f / 255f, 82f / 255f);
        //增速衰减终点色
        private static readonly Color DecayPink = new(1f, 0.45f, 0.72f);
        private static readonly Color TextNormal = new(0.8f, 0.8f, 0.9f);
        private static readonly Color BarBg = new(0.12f, 0.12f, 0.15f, 0.9f);
        private static readonly Color CardBg = new(20f / 255f, 22f / 255f, 30f / 255f, 0.95f);
        private const float PotentialAlpha = 0.55f;

        //伟业档位配色（tier 1~9）
        private static readonly Color[] TierColors =
        {
            new Color(0.78f, 0.80f, 0.86f), new Color(0.55f, 0.80f, 0.90f), new Color(0.45f, 0.87f, 0.67f),
            new Color(0.60f, 0.90f, 0.42f), new Color(0.92f, 0.88f, 0.40f), new Color(1.00f, 0.75f, 0.20f),
            new Color(1.00f, 0.55f, 0.15f), new Color(1.00f, 0.32f, 0.28f), new Color(1.00f, 0.15f, 0.55f)
        };

        private Vector2 scroll;

        public ITab_Pawn_Pharna()
        {
            size = new Vector2(TabWidth, TabHeight);
            labelKey = "FamiliaMyth.TabPharna";
        }

        //仅持有恩惠者显示本页签
        public override bool IsVisible => SelPawn?.GetPharna() != null;

        protected override void FillTab()
        {
            Hediff_Pharna pharna = SelPawn?.GetPharna();
            if (pharna == null) return;

            Rect rect = new Rect(0f, 0f, TabWidth, TabHeight).ContractedBy(10f);
            Rect view = new(0f, 0f, rect.width - 16f, Mathf.Max(MeasureHeight(pharna), rect.height));

            Widgets.BeginScrollView(rect, ref scroll, view);
            float y = 0f;
            y = DrawAbilitySection(pharna, view.width, y);
            y = DrawDevelopableSection(pharna, view.width, y);
            y = DrawSkillSpellSection(pharna, view.width, y);
            DrawFeatSection(pharna, view.width, y);
            Widgets.EndScrollView();
        }

        //==================== 高度测算 ====================

        private static float SectionHeight(float innerHeight)
            => UI_GroupBox.TitleHeight + UI_GroupBox.BasicMargin * 2f + innerHeight;

        private static float AbilityInnerHeight()
            => RowHeight * 7f;                       //等级/经验行 + 六维

        private static float DevelopableInnerHeight(Hediff_Pharna pharna)
        {
            int n = pharna.developableAbilities?.Count ?? 0;
            return RowHeight * Mathf.Max(1, n);
        }

        private static float SkillSpellInnerHeight(Hediff_Pharna pharna)
        {
            int n = Mathf.Max(pharna.skills?.Count ?? 0, pharna.spells?.Count ?? 0);
            return Mathf.Max(1, n) * (SmallCardH + SmallCardGap);
        }

        private static float FeatInnerHeight(Hediff_Pharna pharna)
        {
            int n = pharna.feats?.Count ?? 0;
            return Mathf.Max(1, n) * (FeatCardH + FeatCardGap);
        }

        private static float MeasureHeight(Hediff_Pharna pharna)
            => AbilityInnerHeight() + SectionGap
             + DevelopableInnerHeight(pharna) + SectionGap
             + SkillSpellInnerHeight(pharna) + SectionGap
             + FeatInnerHeight(pharna);

        //==================== 能力 ====================

        private static float DrawAbilitySection(Hediff_Pharna pharna, float width, float y)
        {
            Rect card = new(0f, y, width, SectionHeight(AbilityInnerHeight()));
            UI_GroupBox.DrawOutlinedCardBg(card, "FamiliaMyth.Panel.SectionAbility".Translate(), SectionBorder);
            Rect inner = UI_GroupBox.ContentRect(card);

            GameFont prevFont = Text.Font;
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.MiddleLeft;

            //等级与经验
            GUI.color = TextNormal;
            Widgets.Label(new Rect(inner.x, inner.y, inner.width, RowHeight),
                "FamiliaMyth.Panel.Header".Translate(pharna.level, (int)pharna.experience));

            //六维
            float ry = inner.y + RowHeight;
            for (int i = 0; i < 6; i++)
            {
                DrawStatusRow(pharna, (PharnaStatusType)i, new Rect(inner.x, ry, inner.width, RowHeight));
                ry += RowHeight;
            }

            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = prevFont;

            return card.yMax + SectionGap;
        }

        private static void DrawStatusRow(Hediff_Pharna pharna, PharnaStatusType type, Rect r)
        {
            float cur = pharna.GetCurrentStatus(type);
            float pot = pharna.GetFullStatus(type) - cur;
            //数值与进度条同色：白 = 正常增速，渐变至粉 = 增速衰减
            Color color = Color.Lerp(Color.white, DecayPink, 1f - pharna.GetGrowthDecay(type));

            GUI.color = TextNormal;
            Widgets.Label(new Rect(r.x, r.y, W_Name, r.height), Hediff_Pharna.GetStatusName(type));

            GUI.color = color;
            Widgets.Label(new Rect(r.x + W_Name, r.y, W_Num, r.height),
                "FamiliaMyth.Panel.StatusNumber".Translate(
                    Hediff_Pharna.GetRank(cur), ((int)cur).ToString(), ((int)pharna.maxStatus).ToString()));

            GUI.color = color.WithAlpha(PotentialAlpha);
            Widgets.Label(new Rect(r.x + W_Name + W_Num, r.y, W_Pot, r.height),
                "FamiliaMyth.Panel.Potential".Translate($"+{(int)pot}"));

            float barX = r.x + W_Name + W_Num + W_Pot + 6f;
            float barW = r.xMax - barX;
            if (barW > 8f)
            {
                Rect bar = new(barX, r.y + 5f, barW, r.height - 10f);
                Widgets.DrawBoxSolid(bar, BarBg);
                GUI.color = color;
                Widgets.FillableBar(bar, Mathf.Clamp01(cur / pharna.maxStatus));
            }

            GUI.color = Color.white;
        }

        //==================== 发展能力 ====================

        private static float DrawDevelopableSection(Hediff_Pharna pharna, float width, float y)
        {
            float inner = DevelopableInnerHeight(pharna);
            Rect card = new(0f, y, width, SectionHeight(inner));
            UI_GroupBox.DrawOutlinedCardBg(card, "FamiliaMyth.Panel.SectionDevelopable".Translate(), SectionBorder);
            Rect content = UI_GroupBox.ContentRect(card);

            GameFont prevFont = Text.Font;
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.MiddleLeft;

            if (pharna.developableAbilities.NullOrEmpty())
            {
                GUI.color = TextNormal.WithAlpha(0.6f);
                Widgets.Label(new Rect(content.x, content.y, content.width, RowHeight),
                    "FamiliaMyth.Panel.Empty".Translate());
            }
            else
            {
                float ry = content.y;
                foreach (DevelopableAbility ability in pharna.developableAbilities)
                {
                    if (ability == null)
                        continue;
                    DrawDevelopableRow(new Rect(content.x, ry, content.width, RowHeight), ability);
                    ry += RowHeight;
                }
            }

            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = prevFont;

            return card.yMax + SectionGap;
        }

        //==================== 技能 / 魔法（两栏） ====================

        private static float DrawSkillSpellSection(Hediff_Pharna pharna, float width, float y)
        {
            float inner = SkillSpellInnerHeight(pharna);
            float halfW = (width - SectionGap) / 2f;
            Rect left = new(0f, y, halfW, SectionHeight(inner));
            Rect right = new(halfW + SectionGap, y, halfW, SectionHeight(inner));

            UI_GroupBox.DrawOutlinedCardBg(left, "FamiliaMyth.Panel.SectionSkills".Translate(), SectionBorder);
            UI_GroupBox.DrawOutlinedCardBg(right, "FamiliaMyth.Panel.SectionSpells".Translate(), SectionBorder);

            GameFont prevFont = Text.Font;
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.MiddleLeft;

            DrawSmallCardList(UI_GroupBox.ContentRect(left), "✦", pharna.skills);
            DrawSmallCardList(UI_GroupBox.ContentRect(right), "✧", pharna.spells);

            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = prevFont;

            return left.yMax + SectionGap;
        }

        private static void DrawSmallCardList(Rect content, string prefix, IEnumerable<IAscendReward> rewards)
        {
            if (rewards == null)
            {
                GUI.color = TextNormal.WithAlpha(0.6f);
                Widgets.Label(new Rect(content.x, content.y, content.width, SmallCardH),
                    "FamiliaMyth.Panel.Empty".Translate());
                return;
            }

            float ry = content.y;
            foreach (IAscendReward reward in rewards)
            {
                if (reward == null)
                    continue;
                DrawSmallCard(new Rect(content.x, ry, content.width, SmallCardH), prefix, reward.Label, reward.RarityColor);
                ry += SmallCardH + SmallCardGap;
            }
        }

        //发展能力行：★ 名称 + 品质 + 熟练度文本 + 熟练度条（稀有度着色）
        private static void DrawDevelopableRow(Rect r, DevelopableAbility ability)
        {
            Color color = ((IAscendReward)ability).RarityColor;
            Widgets.DrawBoxSolid(r, CardBg);
            Widgets.DrawBoxSolid(new Rect(r.x, r.y, 3f, r.height), color);

            GameFont prevFont = Text.Font;
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.MiddleLeft;

            float nameW = r.width * 0.45f;
            GUI.color = color;
            Widgets.Label(new Rect(r.x + 8f, r.y, nameW - 8f, r.height),
                $"★ {ability.Label}  {Hediff_Pharna.GetDegreeRank(ability.Degree)}");

            float maxProf = ability.def?.maxProficiency ?? 999f;
            Rect numR = new(r.x + nameW, r.y, 100f, r.height);
            GUI.color = TextNormal;
            Widgets.Label(numR, "FamiliaMyth.Panel.DevelopableProficiency".Translate(
                ((int)ability.Proficiency).ToString(), ((int)maxProf).ToString()));

            float barX = numR.xMax + 4f;
            float barW = r.xMax - barX - 6f;
            if (barW > 8f)
            {
                Rect bar = new(barX, r.y + 5f, barW, r.height - 10f);
                Widgets.DrawBoxSolid(bar, BarBg);
                GUI.color = color;
                Widgets.FillableBar(bar, Mathf.Clamp01(ability.Proficiency / maxProf));
            }

            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = prevFont;
        }

        //条目小卡：深底 + 左侧稀有度色条 + 稀有色文本
        private static void DrawSmallCard(Rect r, string prefix, string label, Color color)
        {
            Widgets.DrawBoxSolid(r, CardBg);
            Widgets.DrawBoxSolid(new Rect(r.x, r.y, 3f, r.height), color);

            GUI.color = color;
            Rect labelR = new(r.x + 8f, r.y, r.width - 10f, r.height);
            GUI.BeginGroup(labelR);
            Widgets.Label(GenUI.AtZero(labelR), $"{prefix} {label}");
            GUI.EndGroup();
            GUI.color = Color.white;
        }

        //==================== 伟业 ====================

        private static float DrawFeatSection(Hediff_Pharna pharna, float width, float y)
        {
            float inner = FeatInnerHeight(pharna);
            Rect card = new(0f, y, width, SectionHeight(inner));
            UI_GroupBox.DrawOutlinedCardBg(card, "FamiliaMyth.Panel.SectionFeats".Translate(), SectionBorder);
            Rect content = UI_GroupBox.ContentRect(card);

            GameFont prevFont = Text.Font;
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.MiddleLeft;

            if (pharna.feats.NullOrEmpty())
            {
                GUI.color = TextNormal.WithAlpha(0.6f);
                Widgets.Label(new Rect(content.x, content.y, content.width, RowHeight),
                    "FamiliaMyth.Panel.Empty".Translate());
            }
            else
            {
                List<FeatRecord> sorted = pharna.feats
                    .Where(f => f != null)
                    .OrderByDescending(f => f.tier)
                    .ThenByDescending(f => f.tick)
                    .ToList();

                float ry = content.y;
                foreach (FeatRecord record in sorted)
                {
                    DrawFeatCard(new Rect(content.x, ry, content.width, FeatCardH), record);
                    ry += FeatCardH + FeatCardGap;
                }
            }

            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = prevFont;

            return card.yMax + SectionGap;
        }

        //伟业卡：未使用 = 空心（tier 色描边）；已使用 = 实心（tier 色淡填充）
        private static void DrawFeatCard(Rect r, FeatRecord record)
        {
            Color tierColor = TierColorOf(record.tier);

            Widgets.DrawBoxSolid(r, CardBg);
            if (record.usedForLevel >= 0)
                Widgets.DrawBoxSolid(r, tierColor.WithAlpha(0.18f));
            else
                UI_GroupBox.DrawOutlinedBox(r, tierColor);
            Widgets.DrawBoxSolid(new Rect(r.x, r.y, 4f, r.height), tierColor);

            string traits = record.featTraits.NullOrEmpty()
                ? "-"
                : string.Join(" × ", record.featTraits.Where(t => t != null).Select(t => t.label));

            GUI.color = tierColor;
            Widgets.Label(new Rect(r.x + 10f, r.y + 2f, r.width - 16f, 18f),
                "FamiliaMyth.Panel.FeatTitle".Translate(record.tier, traits));

            GUI.color = TextNormal;
            Widgets.Label(new Rect(r.x + 10f, r.y + 21f, r.width - 16f, 16f),
                "FamiliaMyth.Panel.FeatScore".Translate(
                    record.battleMagnitude.ToString("F0"), record.share.ToStringPercent()));

            GUI.color = TextNormal.WithAlpha(0.72f);
            Widgets.Label(new Rect(r.x + 10f, r.y + 40f, r.width - 16f, 16f),
                "FamiliaMyth.Panel.FeatWhen".Translate(
                    GenDate.DateFullStringAt(record.tick, Vector2.zero), DescribeTile(record.tileId)));

            GUI.color = Color.white;
        }

        private static Color TierColorOf(int tier)
            => TierColors[Mathf.Clamp(tier - 1, 0, TierColors.Length - 1)];

        //地点：取该地块的主要生物群系名；无效或越界地块返回占位符
        private static string DescribeTile(int tileId)
        {
            if (tileId < 0 || Find.WorldGrid == null)
                return "FamiliaMyth.FeatUnknownPlace".Translate();

            try
            {
                Tile tile = Find.WorldGrid[new PlanetTile(tileId)];
                return tile?.PrimaryBiome?.LabelCap ?? "FamiliaMyth.FeatUnknownPlace".Translate();
            }
            catch
            {
                return "FamiliaMyth.FeatUnknownPlace".Translate();
            }
        }
    }
}
