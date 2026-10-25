using System.Text;
using FamiliaMyth.Core;
using FamiliaMyth.Pharna;
using FamiliaMyth.UI;
using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Feat
{
    //一场战斗的结算快照：一次计算，开发者日志与玩家信件两个视图共用
    public class FeatBattleReport
    {
        public string battleName;
        public int tickAbs;
        public int tileId;

        //战斗事实
        public int enemyCount;
        public float threat;
        public float maxCombatPower;
        public int defeatedCount;
        public int friendlyCount;
        public int downedCount;
        //我方死亡或倒地人数
        public int friendlyLostCount;
        public int participantCount;
        public int battleDuration;

        //战斗层性质与总分
        public List<FeatTraitDef> battleTraits;
        public float battleMagnitude;
        public float battleMultiplier;

        //仅开发者日志使用
        public float totalContribution;
        public float enemyDamageTotal;
        public float friendliesDamageTotal;
        public float shareSum;

        //我方参战者逐人明细（含未受恩惠者）
        public List<FeatReportRow> rows;
        //受恩惠参战者，供信件跳转定位
        public List<Pawn> participants;

        private static LetterDef cachedLetterDef;

        private static LetterDef FeatLetterDef =>
            cachedLetterDef ??= DefDatabase<LetterDef>.GetNamed("FamiliaMyth_FeatAchieved", false);

        //性质文本：标签与系数并列；空列表返回 null
        public static string DescribeTraits(List<FeatTraitDef> traits, string separator = "、")
        {
            if (traits.NullOrEmpty()) return null;

            List<string> parts = new();
            for (int i = 0; i < traits.Count; i++)
            {
                FeatTraitDef trait = traits[i];
                if (trait != null) parts.Add($"{trait.label}×{trait.multiplier.ToString("F2")}");
            }

            return parts.Count == 0 ? null : string.Join(separator, parts);
        }

        //开发者日志：保留合账、Σ占比等核对字段
        public void WriteLog()
        {
            if (rows == null) return;

            StringBuilder sb = new();
            sb.AppendLine($"[FamiliaMyth] 战斗结算：{battleName ?? "未知战斗"}");
            sb.AppendLine($"  战斗事实：敌方 {enemyCount} 人｜战力合计 {threat:F1}｜最强单体 {maxCombatPower:F1}｜已倒下或阵亡 {defeatedCount}"
                + $"；我方 {friendlyCount} 人｜倒地 {downedCount}｜受恩惠参与者 {participantCount}；战斗总时间：{battleDuration.ToStringTicksToPeriod()}");
            sb.AppendLine($"  性质判定：{DescribeTraits(battleTraits) ?? "无"} → 战斗总得分 {battleMagnitude:F1}（敌方战力 {threat:F1} × 性质系数 {battleMultiplier:F2}）");
            sb.AppendLine($"  总贡献：{totalContribution:F1}｜总输出：{enemyDamageTotal:F1}");
            sb.AppendLine($"  合账：敌方承受 {enemyDamageTotal:F1}｜我方输出 {friendliesDamageTotal:F1}"
                + $"｜差额 {enemyDamageTotal - friendliesDamageTotal:F1}｜Σ占比 {shareSum * 100f:F1}%");
            sb.AppendLine($"  贡献明细（输出｜承伤×{FeatContext.TakenWeight:F1}｜支援 → 合计｜占比｜个人得分｜档位）：");

            for (int i = 0; i < rows.Count; i++)
            {
                FeatReportRow row = rows[i];
                if (row?.pawn == null) continue;

                string reward = row.rewarded
                    ? $"｜{row.score:F1}｜{(row.tier > 0 ? $"{row.tier} 级" : "未达档位")}{PersonalText(row)}"
                    : "";

                sb.AppendLine($"    {(row.rewarded ? "[恩惠]" : "[    ]")} {row.pawn.Name.ToStringShort}（{row.pawn.KindLabel}）："
                    + $"{row.dealt:F1}｜{row.taken * FeatContext.TakenWeight:F1}｜{row.support:F1}"
                    + $" → {row.contribution:F1}｜{row.share.ToStringPercent()}{reward}");
            }

            Log.Message(sb.ToString());
        }

        //玩家信件：一场战斗一封，多人达标合并在正文里逐人列出
        public void SendLetter()
        {
            if (rows == null) return;

            List<FeatReportRow> rewarded = new();
            for (int i = 0; i < rows.Count; i++)
            {
                FeatReportRow row = rows[i];
                if (row != null && row.rewarded && row.tier > 0) rewarded.Add(row);
            }
            if (rewarded.Count == 0) return;

            LetterDef def = FeatLetterDef;
            if (def == null)
            {
                Log.Error("[FamiliaMyth] Can't find LetterDef FamiliaMyth_FeatAchieved, feat letter skipped.");
                return;
            }
            if (Find.LetterStack == null) return;

            Letter letter = LetterMaker.MakeLetter(
                "FamiliaMyth.FeatLetter.Label".Translate(),
                BuildLetterText(rewarded),
                def,
                new LookTargets(participants));

            Find.LetterStack.ReceiveLetter(letter);

            for (int i = 0; i < rewarded.Count; i++)
                TutorialLetterHelper.TryCheckAscendable(rewarded[i].pawn?.GetPharna());
        }

        //TaggedString 转 string 会剥离富文本标签，正文一律取 RawText
        private string BuildLetterText(List<FeatReportRow> rewarded)
        {
            StringBuilder sb = new();

            string battle = battleName.NullOrEmpty()
                ? "FamiliaMyth.FeatUnknownBattle".Translate().RawText
                : battleName;

            //开篇：达成者名单
            sb.AppendLine("FamiliaMyth.FeatLetter.Intro".Translate(ComposeNames(rewarded), battle).RawText);
            sb.AppendLine();

            //战斗详情
            sb.AppendLine("FamiliaMyth.FeatLetter.SectionBattle".Translate().RawText);
            sb.AppendLine("FamiliaMyth.FeatLetter.EnemyLine".Translate(
                Blue(enemyCount.ToString()), Blue(threat.ToString("F0")), Blue(defeatedCount.ToString())).RawText);
            sb.AppendLine("FamiliaMyth.FeatLetter.FriendlyLine".Translate(
                Blue(friendlyCount.ToString()), Blue(friendlyLostCount.ToString())).RawText);
            sb.AppendLine("FamiliaMyth.FeatLetter.DurationLine".Translate(
                Blue(battleDuration.ToStringTicksToPeriod())).RawText);
            sb.AppendLine("FamiliaMyth.FeatLetter.ScoreLine".Translate(
                Blue(battleMagnitude.ToString("F0")), Blue(threat.ToString("F0")),
                Blue(battleMultiplier.ToString("F2"))).RawText);

            string multipliers = ComposeTraitMultipliers();
            if (!multipliers.NullOrEmpty())
                sb.AppendLine("FamiliaMyth.FeatLetter.MultipliersLine".Translate(multipliers).RawText);

            sb.AppendLine();

            //贡献度分析
            sb.AppendLine("FamiliaMyth.FeatLetter.SectionRecords".Translate().RawText);
            for (int i = 0; i < rewarded.Count; i++)
            {
                FeatReportRow row = rewarded[i];
                sb.AppendLine("FamiliaMyth.FeatLetter.Row".Translate(
                    Colored("FamiliaMyth.FeatLetter.TierText".Translate(row.tier).RawText,
                        UI_Palette.TierColorOf(row.tier)),
                    ColoredNameOf(row.pawn),
                    Blue(row.dealt.ToString("F0")), Blue(row.taken.ToString("F0")),
                    Blue(row.support.ToString("F0")), Blue(row.share.ToStringPercent()),
                    Blue(row.score.ToString("F0"))).RawText);

                if (row.personalTraits.NullOrEmpty()) continue;
                for (int j = 0; j < row.personalTraits.Count; j++)
                {
                    FeatTraitDef trait = row.personalTraits[j];
                    if (trait == null) continue;
                    sb.AppendLine("FamiliaMyth.FeatLetter.TraitLine".Translate(
                        trait.label, Blue(trait.multiplier.ToString("F2"))).RawText);
                }
            }

            sb.AppendLine();
            AppendAscendSection(sb, rewarded);

            return sb.ToString().TrimEnd();
        }

        //战斗层系数展开：性质名 + 各自系数；未命中任何性质时返回 null
        private string ComposeTraitMultipliers()
        {
            if (battleTraits.NullOrEmpty()) return null;

            List<string> parts = new();
            for (int i = 0; i < battleTraits.Count; i++)
            {
                FeatTraitDef trait = battleTraits[i];
                if (trait == null) continue;
                parts.Add(trait.label + " ×" + Blue(trait.multiplier.ToString("F2")));
            }

            return parts.Count == 0 ? null : string.Join(" · ", parts);
        }

        //开篇名单：最多三名，多出的用「等人」截断
        private static string ComposeNames(List<FeatReportRow> rewarded)
        {
            const int MaxNames = 3;

            List<string> names = new();
            for (int i = 0; i < rewarded.Count && i < MaxNames; i++)
                names.Add(ColoredNameOf(rewarded[i].pawn));

            string joined = string.Join("FamiliaMyth.FeatLetter.NameSeparator".Translate().RawText, names);
            return rewarded.Count > MaxNames
                ? joined + "FamiliaMyth.FeatLetter.Etc".Translate().RawText
                : joined;
        }

        //升华条件：与仪式内的 CanAscend 同源判据，逐人一句总结
        private static void AppendAscendSection(StringBuilder sb, List<FeatReportRow> rewarded)
        {
            sb.AppendLine("FamiliaMyth.FeatLetter.SectionAscend".Translate().RawText);

            for (int i = 0; i < rewarded.Count; i++)
            {
                FeatReportRow row = rewarded[i];
                Hediff_Pharna pharna = row.pawn?.GetPharna();
                if (pharna == null) continue;

                if (pharna.AtLevelCap)
                {
                    sb.AppendLine("FamiliaMyth.FeatLetter.AscendMaxLevel".Translate(
                        ColoredNameOf(row.pawn)).RawText);
                    continue;
                }

                PharnaStatusType top = TopStatus(pharna);
                float topValue = pharna.GetCurrentStatus(top);
                bool ready = pharna.HasUsableFeat() && pharna.MeetsStatusRequirement();

                string phrase = ready
                    ? "FamiliaMyth.FeatLetter.AscendReadyPhrase".Translate().RawText
                    : "FamiliaMyth.FeatLetter.AscendNotReadyPhrase".Translate().RawText;

                sb.AppendLine("FamiliaMyth.FeatLetter.AscendLine".Translate(
                    ColoredNameOf(row.pawn),
                    Gold("FamiliaMyth.FeatLetter.LevelText".Translate(pharna.level + 1).RawText),
                    Gold(Hediff_Pharna.GetStatusName(top)),
                    Gold(Hediff_Pharna.GetRank(topValue)),
                    Gold(((int)topValue).ToString()),
                    ready ? Gold(phrase) : Red(phrase)).RawText);
            }
        }

        //六维中当前值最高的一项
        private static PharnaStatusType TopStatus(Hediff_Pharna pharna)
        {
            PharnaStatusType top = PharnaStatusType.Strength;
            float max = pharna.GetCurrentStatus(top);
            for (int i = 1; i < 6; i++)
            {
                PharnaStatusType type = (PharnaStatusType)i;
                float value = pharna.GetCurrentStatus(type);
                if (value > max)
                {
                    max = value;
                    top = type;
                }
            }
            return top;
        }

        private static string NameOf(Pawn pawn) =>
            pawn?.Name?.ToStringShort ?? pawn?.LabelShort ?? "-";

        //个人层性质文本，附在该参与者明细行末
        private static string PersonalText(FeatReportRow row)
        {
            string traits = DescribeTraits(row.personalTraits);
            return traits.NullOrEmpty() ? "" : "｜个人：" + traits;
        }

        //名字染角色喜好色，无喜好色时用正文色
        private static string ColoredNameOf(Pawn pawn) =>
            Colored(NameOf(pawn), pawn?.story?.favoriteColor?.color ?? UI_Palette.TextNormal);

        //信件的数值与结论用色
        private static string Blue(string text) => Colored(text, UI_Palette.LetterValue);
        private static string Gold(string text) => Colored(text, UI_Palette.Accent);
        private static string Red(string text) => Colored(text, UI_Palette.LetterWarn);

        private static string Colored(string text, Color color) =>
            $"<color=#{ToHex(color)}>{text}</color>";

        private static string ToHex(Color color) =>
            ((int)(Mathf.Clamp01(color.r) * 255f)).ToString("X2")
            + ((int)(Mathf.Clamp01(color.g) * 255f)).ToString("X2")
            + ((int)(Mathf.Clamp01(color.b) * 255f)).ToString("X2");
    }

    //报告里的一行：某名我方参战者在这场战斗中的贡献
    public class FeatReportRow
    {
        public Pawn pawn;
        public float dealt;
        public float taken;
        public float support;
        public float contribution;
        public float share;
        public float score;
        public int tier;
        public bool rewarded;
        public List<FeatTraitDef> personalTraits;
    }
}
