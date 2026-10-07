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
    //分区卡用淡暖金描边与纯白标题，条目卡不描边、以左侧色条区分类别
    public class ITab_Pawn_Pharna : ITab
    {
        private const float TabWidth = 680f;
        private const float TabHeight = 560f;
        private const float SectionGap = 8f;
        private const float RowHeight = 28f;
        private const float SmallCardH = 26f;
        private const float SmallCardGap = 2f;
        private const float FeatCardH = 74f;
        private const float FeatCardGap = 4f;

        //能力行四段宽度：名称 / 评级 / 数值
        private const float W_Name = 80f;
        private const float W_Rank = 24f;
        private const float W_Value = 96f;

        //行内段落间距
        private const float SegmentGap = 6f;

        //条目卡左侧色条宽度
        private const float AccentBarW = 3f;

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
            UI_Cursor cur = new(0f, 0f, view.width);
            DrawAbilitySection(pharna, ref cur);
            cur.Gap(SectionGap);
            DrawDevelopableSection(pharna, ref cur);
            cur.Gap(SectionGap);
            DrawSkillSpellSection(pharna, ref cur);
            cur.Gap(SectionGap);
            DrawFeatSection(pharna, ref cur);
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

        private static void DrawAbilitySection(Hediff_Pharna pharna, ref UI_Cursor cur)
        {
            Rect card = cur.Row(SectionHeight(AbilityInnerHeight()));
            UI_GroupBox.DrawOutlinedCardBg(card, "FamiliaMyth.Panel.SectionAbility".Translate(), UI_Palette.SectionBorder);
            UI_Cursor inner = new(UI_GroupBox.ContentRect(card));

            using (UI_Scope.Text(GameFont.Small, TextAnchor.MiddleLeft))
            {
                using (UI_Scope.Color(UI_Palette.TextNormal))
                    Widgets.Label(inner.Row(RowHeight),
                        "FamiliaMyth.Panel.Header".Translate(PawnName(pharna), pharna.level, (int)pharna.experience));

                static string PawnName(Hediff_Pharna p) => p.pawn != null ? p.pawn.NameFullColored.ToString() : "";

                for (int i = 0; i < 6; i++)
                    DrawStatusRow(pharna, (PharnaStatusType)i, inner.Row(RowHeight), i);
            }
        }

        private static void DrawStatusRow(Hediff_Pharna pharna, PharnaStatusType type, Rect r, int index)
        {
            if ((index & 1) == 0)
                Widgets.DrawBoxSolid(r, UI_Palette.RowStripe);

            float now = pharna.GetCurrentStatus(type);
            float potential = pharna.GetPotential(type);
            float total = now + potential;
            float decay = pharna.GetGrowthDecay(type);
            //进度条随增速衰减由白转暖色
            Color barColor = Color.Lerp(Color.white, UI_Palette.Decay, 1f - decay);
            bool reached = now >= Hediff_Pharna.AscendStatusThreshold;

            using (UI_Scope.Color(UI_Palette.TextNormal))
                Widgets.Label(new Rect(r.x, r.y, W_Name, r.height), Hediff_Pharna.GetStatusName(type));

            using (UI_Scope.Color(UI_Palette.Accent))
                Widgets.Label(new Rect(r.x + W_Name, r.y, W_Rank, r.height), Hediff_Pharna.GetRank(now));

            using (UI_Scope.Color(UI_Palette.TextValue))
                Widgets.Label(new Rect(r.x + W_Name + W_Rank, r.y, W_Value, r.height),
                    "FamiliaMyth.Panel.StatusValue".Translate(
                        ((int)now).ToString(), ((int)pharna.maxStatus).ToString()));

            float barX = r.x + W_Name + W_Rank + W_Value + SegmentGap;
            float barW = r.xMax - barX;
            if (barW > 8f)
            {
                Rect bar = new(barX, r.y + 6f, barW, r.height - 12f);
                UI_Bar.DrawWithThreshold(bar, now / pharna.maxStatus, barColor,
                    Hediff_Pharna.AscendStatusThreshold / pharna.maxStatus,
                    reached ? UI_Palette.Disabled : UI_Palette.Accent);
            }

            TooltipHandler.TipRegion(r, () => StatusTooltip(type, now, potential, total, decay),
                Gen.HashCombineInt(pharna.GetHashCode(), 197305 + (int)type));
        }

        //数值格式在参数侧完成，文本键内不写格式说明符
        private static string StatusTooltip(PharnaStatusType type, float now, float potential, float total, float decay)
        {
            return Hediff_Pharna.GetStatusName(type)
                + "\n\n" + "FamiliaMyth.Panel.StatusTooltip".Translate(
                    ((int)now).ToString(), ((int)potential).ToString(), ((int)total).ToString())
                + "\n" + "FamiliaMyth.Panel.GrowthEfficiency".Translate(decay.ToStringPercent());
        }

        //==================== 发展能力 ====================

        private static void DrawDevelopableSection(Hediff_Pharna pharna, ref UI_Cursor cur)
        {
            float innerHeight = DevelopableInnerHeight(pharna);
            Rect card = cur.Row(SectionHeight(innerHeight));
            UI_GroupBox.DrawOutlinedCardBg(card, "FamiliaMyth.Panel.SectionDevelopable".Translate(), UI_Palette.SectionBorder);
            UI_Cursor inner = new(UI_GroupBox.ContentRect(card));

            using (UI_Scope.Text(GameFont.Small, TextAnchor.MiddleLeft))
            {
                if (pharna.developableAbilities.NullOrEmpty())
                {
                    using (UI_Scope.Color(UI_Palette.TextNormal.WithAlpha(0.6f)))
                        Widgets.Label(inner.Row(RowHeight), "FamiliaMyth.Panel.Empty".Translate());
                    return;
                }

                for (int i = 0; i < pharna.developableAbilities.Count; i++)
                {
                    Rect row = inner.Row(RowHeight);
                    DevelopableAbility ability = pharna.developableAbilities[i];
                    if (ability == null)
                        continue;
                    DrawDevelopableRow(row, ability, i);
                }
            }
        }

        //发展能力行：★ 名称 + 品质 + 熟练度文本 + 熟练度条（稀有度着色）
        private static void DrawDevelopableRow(Rect r, DevelopableAbility ability, int index)
        {
            Color color = ((IAscendReward)ability).RarityColor;
            Widgets.DrawBoxSolid(r, UI_Palette.CardBg);
            if ((index & 1) == 0)
                Widgets.DrawBoxSolid(r, UI_Palette.RowStripe);
            Widgets.DrawBoxSolid(new Rect(r.x, r.y, AccentBarW, r.height), color);

            float nameW = r.width * 0.45f;
            string name = UI_Text.Truncate(
                $"★ {ability.Label}  {Hediff_Pharna.GetDegreeRank(ability.Degree)}", nameW - 8f, GameFont.Small);

            using (UI_Scope.Color(color))
                Widgets.Label(new Rect(r.x + 8f, r.y, nameW - 8f, r.height), name);

            float maxProf = ability.def?.maxProficiency ?? 999f;
            float numW = 110f;
            Rect numR = new(r.x + nameW, r.y, numW, r.height);
            using (UI_Scope.Color(UI_Palette.TextValue))
                Widgets.Label(numR, "FamiliaMyth.Panel.DevelopableProficiency".Translate(
                    ((int)ability.Proficiency).ToString(), ((int)maxProf).ToString()));

            float barX = numR.xMax + SegmentGap;
            float barW = r.xMax - barX - SegmentGap;
            if (barW > 8f)
            {
                Rect bar = new(barX, r.y + 6f, barW, r.height - 12f);
                UI_Bar.Draw(bar, ability.Proficiency / maxProf, color);
            }

            TooltipHandler.TipRegion(r, () => $"{ability.Label}\n\n{ability.Description}",
                Gen.HashCombineInt(ability.GetHashCode(), 197307));
        }

        //==================== 技能 / 魔法（两栏） ====================

        private static void DrawSkillSpellSection(Hediff_Pharna pharna, ref UI_Cursor cur)
        {
            float innerHeight = SkillSpellInnerHeight(pharna);
            float halfW = (cur.Width - SectionGap) / 2f;
            float y = cur.Y;
            Rect left = new(cur.X, y, halfW, SectionHeight(innerHeight));
            Rect right = new(cur.X + halfW + SectionGap, y, halfW, SectionHeight(innerHeight));
            cur.Gap(SectionHeight(innerHeight));

            UI_GroupBox.DrawOutlinedCardBg(left, "FamiliaMyth.Panel.SectionSkills".Translate(), UI_Palette.SectionBorder);
            UI_GroupBox.DrawOutlinedCardBg(right, "FamiliaMyth.Panel.SectionSpells".Translate(), UI_Palette.SectionBorder);

            using (UI_Scope.Text(GameFont.Small, TextAnchor.MiddleLeft))
            {
                DrawSmallCardList(UI_GroupBox.ContentRect(left), "✦", pharna.skills);
                DrawSmallCardList(UI_GroupBox.ContentRect(right), "✧", pharna.spells);
            }
        }

        private static void DrawSmallCardList(Rect content, string prefix, IEnumerable<IAscendReward> rewards)
        {
            UI_Cursor cur = new(content);

            if (rewards == null)
            {
                using (UI_Scope.Color(UI_Palette.TextNormal.WithAlpha(0.6f)))
                    Widgets.Label(cur.Row(SmallCardH), "FamiliaMyth.Panel.Empty".Translate());
                return;
            }

            foreach (IAscendReward reward in rewards)
            {
                Rect row = cur.Row(SmallCardH);
                if (reward == null)
                    continue;
                DrawSmallCard(row, prefix, reward);
                cur.Gap(SmallCardGap);
            }
        }

        //条目小卡：深底 + 左侧稀有度色条 + 稀有色文本
        private static void DrawSmallCard(Rect r, string prefix, IAscendReward reward)
        {
            Color color = reward.RarityColor;
            Widgets.DrawBoxSolid(r, UI_Palette.CardBg);
            Widgets.DrawBoxSolid(new Rect(r.x, r.y, AccentBarW, r.height), color);

            Rect labelR = new(r.x + 8f, r.y, r.width - 10f, r.height);
            using (UI_Scope.Color(color))
            using (UI_Scope.Group(labelR))
                Widgets.Label(GenUI.AtZero(labelR),
                    UI_Text.Truncate($"{prefix} {reward.Label}", labelR.width, GameFont.Small));

            TooltipHandler.TipRegion(r, () => $"{reward.Label}\n\n{reward.Description}",
                Gen.HashCombineInt(reward.GetHashCode(), 197307));
        }

        //==================== 伟业 ====================

        private static void DrawFeatSection(Hediff_Pharna pharna, ref UI_Cursor cur)
        {
            float innerHeight = FeatInnerHeight(pharna);
            Rect card = cur.Row(SectionHeight(innerHeight));
            UI_GroupBox.DrawOutlinedCardBg(card, "FamiliaMyth.Panel.SectionFeats".Translate(), UI_Palette.SectionBorder);
            UI_Cursor inner = new(UI_GroupBox.ContentRect(card));

            using (UI_Scope.Text(GameFont.Small, TextAnchor.MiddleLeft))
            {
                if (pharna.feats.NullOrEmpty())
                {
                    using (UI_Scope.Color(UI_Palette.TextNormal.WithAlpha(0.6f)))
                        Widgets.Label(inner.Row(RowHeight), "FamiliaMyth.Panel.Empty".Translate());
                    return;
                }

                List<FeatRecord> sorted = pharna.feats
                    .Where(f => f != null)
                    .OrderByDescending(f => f.tier)
                    .ThenByDescending(f => f.tick)
                    .ToList();

                foreach (FeatRecord record in sorted)
                {
                    DrawFeatCard(inner.Row(FeatCardH), record);
                    inner.Gap(FeatCardGap);
                }
            }
        }

        //伟业卡三行：档位·性质 / 战斗名 / 时间·地点；分数明细在 tooltip
        //未使用 = 原底 + 亮色条；已使用 = 档位色淡填充 + 半透明色条
        private static void DrawFeatCard(Rect r, FeatRecord record)
        {
            Color tierColor = UI_Palette.TierColorOf(record.tier);
            bool used = record.usedForLevel >= 0;

            Widgets.DrawBoxSolid(r, UI_Palette.CardBg);
            if (used)
                Widgets.DrawBoxSolid(r, tierColor.WithAlpha(0.10f));
            Widgets.DrawBoxSolid(new Rect(r.x, r.y, 4f, r.height), used ? tierColor.WithAlpha(0.7f) : tierColor);

            string traits = record.featTraits.NullOrEmpty()
                ? "-"
                : string.Join(" × ", record.featTraits.Where(t => t != null).Select(t => t.label));
            string battle = record.battleName.NullOrEmpty()
                ? "FamiliaMyth.FeatUnknownBattle".Translate()
                : record.battleName;

            float textX = r.x + 10f;
            float textW = r.width - 16f;

            using (UI_Scope.Color(tierColor))
                Widgets.Label(new Rect(textX, r.y + 4f, textW, 22f),
                    "FamiliaMyth.Panel.FeatTitle".Translate(record.tier, traits));

            using (UI_Scope.Color(UI_Palette.TextNormal))
                Widgets.Label(new Rect(textX, r.y + 26f, textW, 22f),
                    UI_Text.Truncate(battle, textW, GameFont.Small));

            using (UI_Scope.Color(UI_Palette.TextValue))
                Widgets.Label(new Rect(textX, r.y + 48f, textW, 22f),
                    "FamiliaMyth.Panel.FeatWhen".Translate(
                        GenDate.DateFullStringAt(record.tick, Vector2.zero), DescribeTile(record.tileId)));

            TooltipHandler.TipRegion(r, () => FeatScoreTooltip(record),
                Gen.HashCombineInt(record.GetHashCode(), 197306));
        }

        //tooltip：战斗总得分 / 个人贡献与个人得分 / 各性质系数
        private static string FeatScoreTooltip(FeatRecord record)
        {
            string score = "FamiliaMyth.Panel.FeatScore".Translate(record.battleMagnitude.ToString("F0"));
            string share = "FamiliaMyth.Panel.FeatShare".Translate(
                record.share.ToStringPercent(), (record.battleMagnitude * record.share).ToString("F0"));
            if (record.featTraits.NullOrEmpty()) return $"{score}\n{share}";

            string traits = string.Join(" · ", record.featTraits
                .Where(t => t != null)
                .Select(t => $"{t.label} ×{t.multiplier.ToString("F1")}"));
            return $"{score}\n{share}\n{traits}";
        }

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
