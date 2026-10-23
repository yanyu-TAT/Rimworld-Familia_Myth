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
        //滚动内容底部留白，避免最后一行紧贴视窗下沿
        private const float ScrollBottomPad = 8f;

        //能力行四段宽度：名称 / 评级 / 数值
        private const float W_Name = 80f;
        private const float W_Rank = 24f;
        private const float W_Value = 96f;

        //名称列内缩，避开铭刻行的左侧色条
        private const float NameIndent = 6f;

        //行内段落间距
        private const float SegmentGap = 6f;

        //条目卡左侧色条宽度
        private const float AccentBarW = 3f;

        //铭刻顶栏高度，以及右端让开原版关闭按钮的留白
        private const float TopBarH = 24f;
        private const float CloseBtnReserve = 24f;

        //进度条跳动：紧追速率（1/秒）、收尾阈值、满亮差距（一档）、最大叠加亮度
        private const float FollowRate = 3f;
        private const float SettleEpsilon = 0.1f;
        private const float FullFlashGap = 100f;
        private const float FlashAlpha = 1f;

        //条跳动：显示值向真实值追赶
        private readonly Dictionary<DevelopableAbility, float> motions = new();
        private readonly Dictionary<PharnaStatusType, float> statusMotions = new();
        private readonly List<DevelopableAbility> staleKeys = new();

        private Vector2 scroll;

        //铭刻选择模式：顶栏按钮进入，点击能力值行完成后退出
        private bool selectingInscription;
        private Pawn lastSelPawn;

        public ITab_Pawn_Pharna()
        {
            size = new Vector2(TabWidth, TabHeight);
            labelKey = "FamiliaMyth.TabPharna";
        }

        //仅持有恩惠者显示本页签
        public override bool IsVisible => SelPawn?.GetPharna() != null;

        //页签基类没有 OnClose，选择模式需在打开与点击外部时复位
        public override void OnOpen()
        {
            selectingInscription = false;
            motions.Clear();
            statusMotions.Clear();
        }

        public override void Notify_ClickOutsideWindow() => selectingInscription = false;

        protected override void FillTab()
        {
            //页签实例会被复用，切换选中对象时必须清掉未完成的选择模式
            if (SelPawn != lastSelPawn)
            {
                lastSelPawn = SelPawn;
                selectingInscription = false;
                motions.Clear();
                statusMotions.Clear();
            }

            Hediff_Pharna pharna = SelPawn?.GetPharna();
            if (pharna == null) return;

            Rect rect = new Rect(0f, 0f, TabWidth, TabHeight).ContractedBy(10f);
            Rect scrollRect = new(rect.x, rect.y + TopBarH, rect.width, rect.height - TopBarH);
            Rect view = new(0f, 0f, scrollRect.width - 16f, Mathf.Max(MeasureHeight(pharna), scrollRect.height));

            Widgets.BeginScrollView(scrollRect, ref scroll, view);
            UI_Cursor cur = new(0f, 0f, view.width);
            DrawAbilitySection(pharna, ref cur);
            cur.Gap(SectionGap);
            DrawDevelopableSection(pharna, ref cur);
            cur.Gap(SectionGap);
            DrawSkillSpellSection(pharna, ref cur);
            cur.Gap(SectionGap);
            DrawFeatSection(pharna, ref cur);
            Widgets.EndScrollView();

            DrawInscriptionBar(pharna, new Rect(rect.x, rect.y, rect.width, TopBarH));
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
            => SectionHeight(AbilityInnerHeight()) + SectionGap
             + SectionHeight(DevelopableInnerHeight(pharna)) + SectionGap
             + SectionHeight(SkillSpellInnerHeight(pharna)) + SectionGap
             + SectionHeight(FeatInnerHeight(pharna)) + ScrollBottomPad;

        //==================== 能力 ====================

        private void DrawAbilitySection(Hediff_Pharna pharna, ref UI_Cursor cur)
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

        //铭刻行：左侧色条、行底与进度条三处用铭刻色；选择模式下点击任意行即完成铭刻
        private void DrawStatusRow(Hediff_Pharna pharna, PharnaStatusType type, Rect r, int index)
        {
            bool inscribed = pharna.inscriptionType == type;

            if (selectingInscription && Mouse.IsOver(r))
                Widgets.DrawBoxSolid(r, UI_Palette.Inscribed.WithAlpha(0.15f));

            if (inscribed)
                Widgets.DrawBoxSolid(r, UI_Palette.Inscribed.WithAlpha(0.10f));
            else if ((index & 1) == 0)
                Widgets.DrawBoxSolid(r, UI_Palette.RowStripe);

            if (inscribed)
                Widgets.DrawBoxSolid(new Rect(r.x, r.y, AccentBarW, r.height), UI_Palette.Inscribed);

            float now = pharna.GetCurrentStatus(type);
            float potential = pharna.GetPotential(type);
            float total = now + potential;
            float decay = pharna.GetGrowthDecay(type);
            //铭刻项无衰减，直接用品色区分；其余随增速衰减由白转暖色
            Color barColor = inscribed ? UI_Palette.Inscribed : Color.Lerp(Color.white, UI_Palette.Decay, 1f - decay);
            bool reached = now >= Hediff_Pharna.AscendStatusThreshold;

            using (UI_Scope.Color(UI_Palette.TextNormal))
                Widgets.Label(new Rect(r.x + NameIndent, r.y, W_Name - NameIndent, r.height), Hediff_Pharna.GetStatusName(type));

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
                float displayed = StepMotion(type, now);
                float frac = displayed / pharna.maxStatus;
                Rect bar = new(barX, r.y + 6f, barW, r.height - 12f);

                //高亮叠在填充之上、门槛刻度之下，不遮挡刻度线
                UI_Bar.Draw(bar, frac, barColor);
                float flashAlpha = FlashAlpha * Mathf.Clamp01((now - displayed) / FullFlashGap);
                if (flashAlpha > 0f)
                    Widgets.DrawBoxSolid(new Rect(bar.x, bar.y, bar.width * Mathf.Clamp01(frac), bar.height),
                        Color.white.WithAlpha(flashAlpha));

                UI_Bar.DrawThreshold(bar, Hediff_Pharna.AscendStatusThreshold / pharna.maxStatus,
                    reached ? UI_Palette.Disabled : UI_Palette.Accent);
            }

            TooltipHandler.TipRegion(r, () => StatusTooltip(pharna, type, now, potential, total, decay),
                Gen.HashCombineInt(pharna.GetHashCode(), 197305 + (int)type));

            if (selectingInscription && Widgets.ButtonInvisible(r) && pharna.TrySetInscription(type))
                selectingInscription = false;
        }

        //数值格式在参数侧完成，文本键内不写格式说明符
        private string StatusTooltip(Hediff_Pharna pharna, PharnaStatusType type, float now, float potential, float total, float decay)
        {
            string text = Hediff_Pharna.GetStatusName(type)
                + "\n\n" + (string)"FamiliaMyth.Panel.StatusTooltip".Translate(
                    ((int)now).ToString(), ((int)potential).ToString(), ((int)total).ToString())
                + "\n" + (string)"FamiliaMyth.Panel.GrowthEfficiency".Translate(decay.ToStringPercent());

            if (pharna.inscriptionType == type)
                text += "\n" + (string)"FamiliaMyth.Panel.InscribedDesc".Translate();
            else if (selectingInscription)
                text += "\n" + (string)"FamiliaMyth.Panel.InscriptionPickTip".Translate();

            return text;
        }

        //==================== 铭刻顶栏 ====================

        //右端让开原版关闭按钮（Widgets.CloseButtonFor 占 xMax-22..xMax-4、y+4..y+22）
        private void DrawInscriptionBar(Hediff_Pharna pharna, Rect bar)
        {
            const float ButtonW = 72f;
            const float TextW = 200f;
            const float Gap = 6f;

            float right = bar.xMax - CloseBtnReserve;
            Rect buttonRect = new(right - ButtonW, bar.y + 1f, ButtonW, bar.height - 2f);
            Rect textRect = new(right - ButtonW - Gap - TextW, bar.y, TextW, bar.height);

            bool active = selectingInscription || pharna.inscriptionAvailable;
            string label = selectingInscription
                ? (string)"FamiliaMyth.Panel.InscriptionSelecting".Translate()
                : pharna.inscriptionAvailable
                    ? (string)"FamiliaMyth.Panel.InscriptionReady".Translate()
                    : (string)"FamiliaMyth.Panel.InscriptionUnavailable".Translate();

            using (UI_Scope.Text(GameFont.Small, TextAnchor.MiddleRight))
            using (UI_Scope.Color(active ? UI_Palette.Inscribed : UI_Palette.Disabled))
                Widgets.Label(textRect, label);

            string buttonLabel = selectingInscription
                ? (string)"FamiliaMyth.UI.Cancel".Translate()
                : (string)"FamiliaMyth.UI.InscriptionButton".Translate();

            if (active && Widgets.ButtonText(buttonRect, buttonLabel, drawBackground: true, doMouseoverSound: true))
                selectingInscription = !selectingInscription;
        }

        //==================== 发展能力 ====================

        private void DrawDevelopableSection(Hediff_Pharna pharna, ref UI_Cursor cur)
        {
            PruneMotions(pharna);

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

        //发展能力行：★ 名称 + 品质 + 熟练度文本 + 熟练度条（稀有度着色，条随成长跳动）
        private void DrawDevelopableRow(Rect r, DevelopableAbility ability, int index)
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
            float actual = ability.Proficiency;
            float displayed = StepMotion(ability, actual);

            //数字用真实值，条用追赶后的显示值
            float numW = 110f;
            Rect numR = new(r.x + nameW, r.y, numW, r.height);
            using (UI_Scope.Color(UI_Palette.TextValue))
                Widgets.Label(numR, "FamiliaMyth.Panel.DevelopableProficiency".Translate(
                    ((int)actual).ToString(), ((int)maxProf).ToString()));

            float barX = numR.xMax + SegmentGap;
            float barW = r.xMax - barX - SegmentGap;
            if (barW > 8f)
            {
                float frac = displayed / maxProf;
                Rect bar = new(barX, r.y + 6f, barW, r.height - 12f);
                UI_Bar.Draw(bar, frac, color);

                //亮度与剩余差距挂钩：一档差距满亮，追上即灭
                float flashAlpha = FlashAlpha * Mathf.Clamp01((actual - displayed) / FullFlashGap);
                if (flashAlpha > 0f)
                    Widgets.DrawBoxSolid(new Rect(bar.x, bar.y, bar.width * Mathf.Clamp01(frac), bar.height),
                        Color.white.WithAlpha(flashAlpha));
            }

            TooltipHandler.TipRegion(r, () => $"{ability.Label}\n\n{ability.Description}",
                Gen.HashCombineInt(ability.GetHashCode(), 197307));
        }

        //推进一帧的显示值；known 为 false 表示该条首次出现，直接对齐
        private static float Advance(float displayed, bool known, float actual)
        {
            if (!known) return actual;

            //只在 Repaint 事件里累加，避免同帧多次 OnGUI 让追赶速度翻倍
            if (Event.current.type != EventType.Repaint)
                return displayed;

            //回落（重建 / 读档）直接对齐，不播倒退
            if (actual <= displayed) return actual;

            displayed = Mathf.Lerp(displayed, actual, 1f - Mathf.Exp(-FollowRate * Time.unscaledDeltaTime));
            return actual - displayed < SettleEpsilon ? actual : displayed;
        }

        private float StepMotion(DevelopableAbility ability, float actual)
        {
            bool known = motions.TryGetValue(ability, out float displayed);
            displayed = Advance(displayed, known, actual);
            motions[ability] = displayed;
            return displayed;
        }

        private float StepMotion(PharnaStatusType type, float actual)
        {
            bool known = statusMotions.TryGetValue(type, out float displayed);
            displayed = Advance(displayed, known, actual);
            statusMotions[type] = displayed;
            return displayed;
        }

        //恩惠重建后能力对象会换新，清掉已不在列表中的动画状态
        private void PruneMotions(Hediff_Pharna pharna)
        {
            int alive = pharna.developableAbilities?.Count ?? 0;
            if (motions.Count == 0 || motions.Count == alive) return;

            staleKeys.Clear();
            foreach (DevelopableAbility ability in motions.Keys)
                if (pharna.developableAbilities == null || !pharna.developableAbilities.Contains(ability))
                    staleKeys.Add(ability);

            foreach (DevelopableAbility ability in staleKeys)
                motions.Remove(ability);
            staleKeys.Clear();
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

            string traits = FeatTraitLine(record);
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

        //tooltip：战斗总得分 / 个人贡献与个人得分 / 战斗层与个人层性质系数
        private static string FeatScoreTooltip(FeatRecord record)
        {
            float personalScore = record.battleMagnitude * record.share * record.personalMultiplier;
            List<string> lines = new()
            {
                "FamiliaMyth.Panel.FeatScore".Translate(record.battleMagnitude.ToString("F0")),
                "FamiliaMyth.Panel.FeatShare".Translate(record.share.ToStringPercent(), personalScore.ToString("F0")),
            };

            string battleTraits = JoinTraitLabels(record.featTraits);
            if (battleTraits != null) lines.Add(battleTraits);

            string personalTraits = JoinTraitLabels(record.personalTraits);
            if (personalTraits != null)
                lines.Add("FamiliaMyth.Panel.FeatPersonalTraits".Translate(personalTraits));

            return string.Join("\n", lines);
        }

        //卡面性质：战斗层与个人层 label 合并；无性质时占位
        private static string FeatTraitLine(FeatRecord record)
        {
            List<string> labels = new();
            CollectTraitLabels(record.featTraits, labels);
            CollectTraitLabels(record.personalTraits, labels);
            return labels.Count == 0 ? "-" : string.Join(" × ", labels);
        }

        private static void CollectTraitLabels(List<FeatTraitDef> traits, List<string> labels)
        {
            if (traits == null) return;
            for (int i = 0; i < traits.Count; i++)
                if (traits[i] != null) labels.Add(traits[i].label);
        }

        //tooltip 性质行：标签与系数并列；空列表返回 null
        private static string JoinTraitLabels(List<FeatTraitDef> traits)
        {
            if (traits.NullOrEmpty()) return null;
            string joined = string.Join(" · ", traits
                .Where(t => t != null)
                .Select(t => $"{t.label} ×{t.multiplier.ToString("F1")}"));
            return joined.NullOrEmpty() ? null : joined;
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
