using System.Globalization;
using FamiliaMyth.Abilities;
using FamiliaMyth.Pharna;
using RimWorld;
using Verse;

namespace FamiliaMyth.UI
{
    /// <summary>
    /// 恩惠加成统计：把目标 StatDef 上恩惠类 StatPart 的说明文本整理成「分类 → 属性 → 行」三级结构。
    /// </summary>
    public static class PharnaBonusStats
    {
        /// <summary>一条说明：左列说明、右列符号与数值、染色方向</summary>
        public readonly struct Line
        {
            public readonly string desc;
            public readonly string value;
            public readonly int sign;

            public Line(string desc, string value, int sign)
            {
                this.desc = desc;
                this.value = value;
                this.sign = sign;
            }
        }

        /// <summary>单个属性及其说明行</summary>
        public sealed class StatEntry
        {
            public readonly StatDef stat;
            public readonly List<Line> lines = new();

            public StatEntry(StatDef stat) => this.stat = stat;
        }

        /// <summary>原版属性分类及其下的属性</summary>
        public sealed class CategoryEntry
        {
            public readonly StatCategoryDef category;
            public readonly List<StatEntry> stats = new();

            public CategoryEntry(StatCategoryDef category) => this.category = category;
        }

        private const string LineSeparator = ": ";

        //判定「无实际变化」的数值容差：+0 / -0 / x100% 视为无加成
        private const float ChangeEpsilon = 1e-4f;

        private static List<StatDef> cachedCandidates;

        //带恩惠类 StatPart 的属性清单；Def 数据库加载后构建一次
        private static List<StatDef> Candidates
        {
            get
            {
                if (cachedCandidates == null)
                {
                    cachedCandidates = new List<StatDef>();
                    foreach (StatDef stat in DefDatabase<StatDef>.AllDefsListForReading)
                    {
                        if (stat?.parts == null) continue;
                        if (stat.parts.Any(p => p is StatPart_Pharna || p is StatPart_DevelopableAbility))
                            cachedCandidates.Add(stat);
                    }
                }
                return cachedCandidates;
            }
        }

        /// <summary>收集当前恩惠的全部加成说明；无加成的属性与分类不返回</summary>
        public static List<CategoryEntry> Collect(Hediff_Pharna pharna)
        {
            List<CategoryEntry> categories = new();
            Pawn pawn = pharna?.pawn;
            if (pawn == null) return categories;

            StatRequest request = StatRequest.For(pawn);
            foreach (StatDef stat in Candidates)
            {
                StatEntry entry = BuildEntry(stat, request);
                if (entry == null) continue;
                FindOrAdd(categories, stat.category).stats.Add(entry);
            }

            foreach (CategoryEntry category in categories)
                category.stats.Sort((a, b) => CompareStats(a.stat, b.stat));
            categories.Sort((a, b) => CompareCategories(a.category, b.category));
            return categories;
        }

        //取该属性上恩惠类 part 的说明文本，按行拆分并解析；无内容返回 null
        private static StatEntry BuildEntry(StatDef stat, StatRequest request)
        {
            StatEntry entry = null;
            foreach (StatPart part in stat.parts)
            {
                if (part is not StatPart_Pharna && part is not StatPart_DevelopableAbility)
                    continue;

                string text = part.ExplanationPart(request);
                if (text.NullOrEmpty()) continue;

                foreach (string rawLine in text.Split('\n'))
                {
                    string trimmed = rawLine.Trim();
                    if (trimmed.Length == 0) continue;

                    Line line = ParseLine(trimmed);
                    if (!HasChange(line.value)) continue;

                    entry ??= new StatEntry(stat);
                    entry.lines.Add(line);
                }
            }
            return entry;
        }

        //按最后一个 ": " 拆成说明与符号+数值
        private static Line ParseLine(string raw)
        {
            int index = raw.LastIndexOf(LineSeparator, StringComparison.Ordinal);
            if (index < 0)
                return new Line(raw, "", 0);

            string desc = raw.Substring(0, index).Trim();
            string value = raw.Substring(index + LineSeparator.Length).Trim();
            return new Line(desc, value, SignOf(value));
        }

        //正负只看符号+数值部分：+ 正、- 负、x/× 后百分比 ≥100% 为正
        private static int SignOf(string value)
        {
            if (value.Length == 0) return 0;

            char first = value[0];
            if (first == '+') return 1;
            if (first == '-') return -1;
            if (first != 'x' && first != 'X' && first != '×') return 0;

            string digits = value.Substring(1).Trim().TrimEnd('%');
            if (!float.TryParse(digits, NumberStyles.Float, CultureInfo.InvariantCulture, out float percent))
                return 0;
            return percent >= 100f ? 1 : -1;
        }

        //符号+数值是否代表实际变化：+0 / -0 / x100% 视为无加成
        private static bool HasChange(string value)
        {
            if (value.Length == 0) return false;

            char marker = value[0];
            bool factor = marker == 'x' || marker == 'X' || marker == '×';
            bool signed = marker == '+' || marker == '-';
            if (!factor && !signed) return true;

            string digits = value.Substring(1).Trim();
            bool percent = digits.EndsWith("%");
            if (percent) digits = digits.Substring(0, digits.Length - 1).Trim();

            int slash = digits.IndexOf('/');
            if (slash > 0) digits = digits.Substring(0, slash).Trim();

            if (!float.TryParse(digits, NumberStyles.Float, CultureInfo.InvariantCulture, out float number))
                return true;

            float baseline = factor ? (percent ? number / 100f : number) : number;
            return Math.Abs(baseline - (factor ? 1f : 0f)) > ChangeEpsilon;
        }

        private static CategoryEntry FindOrAdd(List<CategoryEntry> categories, StatCategoryDef category)
        {
            foreach (CategoryEntry entry in categories)
                if (entry.category == category) return entry;

            CategoryEntry created = new(category);
            categories.Add(created);
            return created;
        }

        //与原版信息卡一致：分类 displayOrder 升序、类内 displayPriorityInCategory 降序、再按 label
        private static int CompareStats(StatDef a, StatDef b)
        {
            int result = b.displayPriorityInCategory.CompareTo(a.displayPriorityInCategory);
            return result != 0 ? result : string.Compare(a.label, b.label, StringComparison.Ordinal);
        }

        private static int CompareCategories(StatCategoryDef a, StatCategoryDef b)
        {
            int aOrder = a?.displayOrder ?? int.MaxValue;
            int bOrder = b?.displayOrder ?? int.MaxValue;
            return aOrder != bOrder ? aOrder.CompareTo(bOrder) : string.Compare(a?.defName, b?.defName, StringComparison.Ordinal);
        }
    }
}
