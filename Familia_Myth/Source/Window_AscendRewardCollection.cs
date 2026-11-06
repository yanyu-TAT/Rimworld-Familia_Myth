using System.Collections.Generic;
using FamiliaMyth.Abilities;
using FamiliaMyth.Pharna;
using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.UI
{
    /// <summary>
    /// 恩惠升华奖励图鉴：书籍风单页列表 + 自动分页 + 详情页
    /// 列表页按类型分组，条目在升华窗口中展示过即点亮，未点亮显示为「？？？」
    /// </summary>
    public class Window_AscendRewardCollection : Window_FamiliaBase
    {
        private enum ViewPage
        {
            Index,
            Detail
        }

        //一行流元素：分组标题或一行条目（最多两张卡）
        private struct FlowRow
        {
            public bool IsHeader;
            public RewardType Type;
            public AscendRewardCatalogEntry Left;
            public int LeftNumber;
            public AscendRewardCatalogEntry Right;
            public int RightNumber;
            public bool HasRight;
            public float Height;
        }

        //==================== 尺寸 ====================

        private const float WindowWidth = 880f;
        private const float WindowHeight = 660f;
        private const float MaxScreenHeightFraction = 0.92f;

        //书皮到纸面的内缩，以及纸面左侧装订线宽度
        private const float PageInset = 12f;
        private const float SpineWidth = 10f;

        private const float PagePadTop = 18f;
        private const float PagePadBottom = 12f;
        private const float PagePadRight = 34f;
        private const float PagePadLeft = 44f;

        private const float PageHeaderHeight = 34f;
        private const float PageFooterHeight = 48f;

        private const int EntryColumns = 2;
        private const float EntryCardHeight = 34f;
        private const float EntryCardGap = 6f;
        private const float EntryColumnGap = 16f;
        private const float GroupHeaderHeight = 26f;
        private const float GroupHeaderGap = 8f;

        private const float RarityBarW = 3f;
        private const float NameIndent = 8f;
        private const float NumberWidth = 50f;
        private const float ProgressWidth = 120f;

        private const float DetailIconSize = 96f;
        private const float DetailIconGap = 18f;
        private const float DetailTitleHeight = 38f;
        private const float DetailTitleGap = 40f;
        private const float DetailMetaHeight = 22f;
        private const float DetailMetaGap = 26f;

        //页脚：按钮行与 ESC 提示行
        private const float FooterButtonWidth = 110f;
        private const float PageFooterRowHeight = 24f;
        private const float EscHintHeight = 14f;

        //书脊折痕：中部横向渐变，越靠中间越深
        private const float SpineFoldWidth = 130f;
        private const float SpineFoldAlpha = 0.16f;

        //稀有度在纸面上的加深比例（向墨色混合）
        private const float InkRarityBlend = 0.42f;

        //咏唱词：主题色向墨色混合并降透明度
        private const float CastInkBlend = 0.55f;
        private const float CastInkAlpha = 0.82f;

        //持有者名字：喜好色向墨色混合
        private const float HolderInkBlend = 0.30f;

        //趣闻区：详情页右下角，宽度要放得下最长的一整段中文
        private const float AnecdoteWidth = 320f;
        private const float AnecdoteGap = 16f;

        //==================== 状态 ====================

        private static readonly RewardType[] DisplayOrder =
        {
            RewardType.DevelopableAbility,
            RewardType.Skill,
            RewardType.Spell
        };

        private readonly List<FlowRow> rows = new();
        private readonly List<List<FlowRow>> pages = new();

        private ViewPage page = ViewPage.Index;
        private int indexPage;
        private AscendRewardCatalogEntry detail;
        private int detailReturnPage;

        //持有者缓存：每帧一次全图扫描，按 defName 归类
        private int holderFrame = -1;
        private readonly Dictionary<string, List<Pawn>> holderByDef = new();

        private static readonly List<Pawn> EmptyHolders = new();

        //书脊渐变纹理，首次绘制时构建
        private static Texture2D spineFold;

        public override Vector2 InitialSize
        {
            get
            {
                float height = Mathf.Min(WindowHeight, Screen.height * MaxScreenHeightFraction / Prefs.UIScale);
                return new Vector2(WindowWidth, height);
            }
        }

        public Window_AscendRewardCollection()
        {
            forcePause = false;
            absorbInputAroundWindow = false;
            preventCameraMotion = false;
            doCloseX = false;
            closeOnCancel = true;
            doWindowBackground = false;
            draggable = true;
            resizeable = false;
        }

        protected override bool HasHeaderArea => false;

        //窗口边缘到书皮不再留原版底色
        protected override float Margin => 0f;

        public override void PostOpen()
        {
            base.PostOpen();
            BuildFlow();
        }

        protected override void DrawBackLayer(Rect inRect)
        {
            Widgets.DrawBoxSolid(inRect, UI_Palette.BookCover);
        }

        protected override void DrawContent(Rect inRect)
        {
            Rect paper = inRect.ContractedBy(PageInset);

            DrawPaper(paper);

            Rect header = HeaderRect(paper);
            Rect footer = FooterRect(paper);
            Rect body = new(paper.x + PagePadLeft, header.yMax + 6f,
                paper.width - PagePadLeft - PagePadRight, footer.y - (header.yMax + 6f) - 6f);

            if (page == ViewPage.Detail)
                DrawDetailPage(header, body, footer);
            else
                DrawIndexPage(header, body, footer);

            //折痕压在所有内容之上，形成内陷感
            DrawSpineFold(paper);
        }

        //==================== 纸张与页框 ====================

        private static void DrawPaper(Rect paper)
        {
            Widgets.DrawBoxSolid(paper, UI_Palette.BookPaper);
            Widgets.DrawBoxSolid(new Rect(paper.x, paper.y, SpineWidth, paper.height), UI_Palette.BookSpine);
            Widgets.DrawBoxSolid(new Rect(paper.x + SpineWidth, paper.y, 2f, paper.height), UI_Palette.BookRule);
            UI_GroupBox.DrawOutlinedBox(paper, UI_Palette.BookRule, 1f);
        }

        private static Rect HeaderRect(Rect paper)
            => new(paper.x + PagePadLeft, paper.y + PagePadTop, paper.width - PagePadLeft - PagePadRight, PageHeaderHeight);

        private static Rect FooterRect(Rect paper)
            => new(paper.x + PagePadLeft, paper.yMax - PagePadBottom - PageFooterHeight,
                paper.width - PagePadLeft - PagePadRight, PageFooterHeight);

        private static Texture2D SpineFold
        {
            get
            {
                if (spineFold != null) return spineFold;

                const int Width = 64;
                spineFold = new Texture2D(Width, 1, TextureFormat.RGBA32, false)
                {
                    hideFlags = HideFlags.HideAndDontSave,
                    wrapMode = TextureWrapMode.Clamp
                };

                for (int x = 0; x < Width; x++)
                {
                    float t = Mathf.Abs(x / (float)(Width - 1) * 2f - 1f);
                    float alpha = Mathf.Lerp(SpineFoldAlpha, 0f, t);
                    spineFold.SetPixel(x, 0, new Color(0.22f, 0.17f, 0.12f, alpha));
                }

                spineFold.Apply();
                return spineFold;
            }
        }

        private static void DrawSpineFold(Rect paper)
        {
            if (Event.current.type != EventType.Repaint) return;

            float width = Mathf.Min(SpineFoldWidth, paper.width * 0.25f);
            GUI.DrawTexture(new Rect(paper.center.x - width / 2f, paper.y, width, paper.height), SpineFold);
        }

        //==================== 文本与控件 ====================

        //图鉴窗口统一入口：纸面上不允许裸调 Widgets.Label
        private static void DrawInk(Rect r, string text, Color ink, GameFont font = GameFont.Small,
            TextAnchor anchor = TextAnchor.MiddleLeft)
        {
            if (text.NullOrEmpty()) return;
            using (UI_Scope.Text(font, anchor))
            using (UI_Scope.Color(ink))
                Widgets.Label(r, text);
        }

        //纸面按钮：自绘底色与文字，返回是否被点击
        private static bool DrawTextButton(Rect r, string label, bool active)
        {
            if (active && Mouse.IsOver(r))
                Widgets.DrawBoxSolid(r, UI_Palette.BookPaperAlt);

            Color ink = active ? UI_Palette.BookInk : UI_Palette.BookInkSoft.WithAlpha(0.35f);
            DrawInk(r, label, ink, GameFont.Small, TextAnchor.MiddleCenter);

            return active && Widgets.ButtonInvisible(r);
        }

        private static void DrawRule(Rect r) => Widgets.DrawBoxSolid(new Rect(r.x, r.y, r.width, 1f), UI_Palette.BookRule);

        //==================== 页眉 ====================

        private void DrawHeader(Rect r, string title)
        {
            DrawInk(r, title, UI_Palette.BookInk, GameFont.Medium, TextAnchor.MiddleLeft);
            DrawRule(new Rect(r.x, r.yMax - 2f, r.width, 1f));

            Rect pr = new(r.xMax - ProgressWidth, r.y, ProgressWidth, r.height);
            DrawInk(pr, ProgressText(), UI_Palette.BookInkSoft, GameFont.Tiny, TextAnchor.MiddleRight);
        }

        //==================== 列表页 ====================

        private void DrawIndexPage(Rect header, Rect body, Rect footer)
        {
            if (rows.Count == 0) BuildFlow();
            EnsurePages(body.height);

            DrawHeader(header, (string)"FamiliaMyth.AscendReward.Title".Translate());

            if (pages.Count == 0)
            {
                DrawInk(body, "FamiliaMyth.AscendReward.Empty".Translate(), UI_Palette.BookInkSoft,
                    GameFont.Small, TextAnchor.MiddleCenter);
                return;
            }

            indexPage = Mathf.Clamp(indexPage, 0, pages.Count - 1);
            List<FlowRow> pageRows = pages[indexPage];

            float colW = (body.width - EntryColumnGap) / EntryColumns;
            float y = body.y;
            for (int i = 0; i < pageRows.Count; i++)
            {
                FlowRow row = pageRows[i];
                if (row.IsHeader)
                {
                    DrawGroupHeader(new Rect(body.x, y, body.width, GroupHeaderHeight), row.Type);
                }
                else
                {
                    DrawEntryCard(new Rect(body.x, y, colW, EntryCardHeight), row.Left, row.LeftNumber);
                    if (row.HasRight)
                        DrawEntryCard(new Rect(body.x + colW + EntryColumnGap, y, colW, EntryCardHeight),
                            row.Right, row.RightNumber);
                }

                y += row.Height;
            }

            DrawFooter(footer, showBack: false);
        }

        private void DrawGroupHeader(Rect r, RewardType type)
        {
            DrawInk(r, GroupLabel(type), UI_Palette.BookInk, GameFont.Small, TextAnchor.MiddleLeft);

            int total = AscendRewardCatalog.CountOf(type);
            int seen = SeenCount(type);
            string counter = "FamiliaMyth.AscendReward.Progress".Translate(seen.ToString(), total.ToString());
            DrawInk(new Rect(r.xMax - ProgressWidth, r.y, ProgressWidth, r.height), counter,
                UI_Palette.BookInkSoft, GameFont.Tiny, TextAnchor.MiddleRight);

            DrawRule(new Rect(r.x, r.yMax - GroupHeaderGap + 2f, r.width, 1f));
        }

        private void DrawEntryCard(Rect r, AscendRewardCatalogEntry entry, int number)
        {
            if (entry.Def == null) return;

            bool collected = IsCollected(entry.DefName);

            //未点亮整张卡压淡，连色条与文字一起弱化
            Widgets.DrawBoxSolid(r, collected ? UI_Palette.BookPaperAlt : UI_Palette.BookPaperAlt.WithAlpha(0.4f));
            Widgets.DrawBoxSolid(new Rect(r.x, r.y, RarityBarW, r.height),
                collected ? InkRarity(entry.Rarity) : UI_Palette.BookInkSoft.WithAlpha(0.2f));

            string label = collected
                ? entry.Label
                : "FamiliaMyth.AscendReward.Unknown".Translate();

            Color nameInk = collected ? UI_Palette.BookInk : UI_Palette.BookInkSoft.WithAlpha(0.5f);
            Rect nameR = new(r.x + NameIndent, r.y, r.width - NameIndent - NumberWidth - 6f, r.height);
            DrawInk(nameR, UI_Text.Truncate(label, nameR.width, GameFont.Small), nameInk,
                GameFont.Small, TextAnchor.MiddleLeft);

            //已点亮显示稀有度名，未点亮显示编号
            string right = collected ? RarityLabel(entry.Rarity) : number.ToString();
            DrawInk(new Rect(r.xMax - NumberWidth, r.y, NumberWidth - 6f, r.height), right,
                collected ? InkRarity(entry.Rarity) : UI_Palette.BookInkSoft.WithAlpha(0.4f),
                GameFont.Tiny, TextAnchor.MiddleRight);

            if (IsHeld(entry.DefName))
                Widgets.DrawBoxSolid(new Rect(r.xMax - 6f, r.y + 4f, 4f, r.height - 8f), UI_Palette.Accent);

            string tooltip = collected
                ? entry.Description
                : (string)"FamiliaMyth.AscendReward.UnknownHint".Translate();
            TooltipHandler.TipRegion(r, new TipSignal(tooltip, Gen.HashCombineInt(number, 197410)));

            if (Mouse.IsOver(r))
                Widgets.DrawBoxSolid(r, new Color(0f, 0f, 0f, 0.04f));

            if (collected && Widgets.ButtonInvisible(r))
            {
                detailReturnPage = indexPage;
                detail = entry;
                page = ViewPage.Detail;
            }
        }

        //==================== 详情页 ====================

        private void DrawDetailPage(Rect header, Rect body, Rect footer)
        {
            AscendRewardCatalogEntry entry = detail;
            if (entry.Def == null)
            {
                page = ViewPage.Index;
                return;
            }

            DrawHeader(header, GroupLabel(entry.Type));

            string anecdote = null;
            if (entry.Def is IAnecdoteProvider provider)
                anecdote = ReplaceLineBreaks(provider.Anecdote.RawText);

            bool hasAnecdote = !anecdote.NullOrEmpty();

            float textX = body.x;
            float textW = body.width;

            if (entry.Icon != null)
            {
                Rect iconR = new(body.x, body.y, DetailIconSize, DetailIconSize);
                GUI.DrawTexture(iconR, entry.Icon, ScaleMode.ScaleToFit);
                textX = iconR.xMax + DetailIconGap;
                textW = body.xMax - textX;
            }

            //有轶闻时正文列给右下角让出区域
            float contentW = hasAnecdote
                ? Mathf.Max(160f, body.xMax - AnecdoteWidth - AnecdoteGap - textX)
                : textW;

            float y = body.y;

            DrawInk(new Rect(textX, y, textW, DetailTitleHeight), entry.Label,
                UI_Palette.BookInk, GameFont.Medium, TextAnchor.UpperLeft);
            y += DetailTitleGap;

            string meta = $"{RarityLabel(entry.Rarity)} · {GroupLabel(entry.Type)}";
            DrawInk(new Rect(textX, y, contentW, DetailMetaHeight), meta, InkRarity(entry.Rarity), GameFont.Small);
            y += DetailMetaGap;

            DrawRule(new Rect(textX, y, contentW, 1f));
            y += 10f;

            string description = IndentParagraphs(entry.Description);
            float descH = Text.CalcHeight(description, contentW);
            DrawInk(new Rect(textX, y, contentW, descH), description,
                UI_Palette.BookInk, GameFont.Small, TextAnchor.UpperLeft);
            y += descH + 12f;

            DrawDetailFacts(entry, textX, ref y, contentW);

            y += 10f;
            DrawHolders(entry.DefName, textX, ref y, contentW);

            if (hasAnecdote)
                DrawAnecdote(body, anecdote);

            DrawFooter(footer, showBack: true);
        }

        //类型特有信息：字段都取自现有 Def，不引入新数据
        private void DrawDetailFacts(AscendRewardCatalogEntry entry, float x, ref float y, float width)
        {
            if (entry.Def is SpellDef spell)
            {
                DrawFact(x, ref y, width,
                    "FamiliaMyth.AscendReward.SpellTypeLine".Translate(SpellTypeLabel(spell.spellType)));

                float entropy = spell.GetStatValueAbstract(StatDefOf.Ability_EntropyGain);
                DrawFact(x, ref y, width,
                    "FamiliaMyth.AscendReward.SpellMana".Translate(spell.manaCost.ToString("F0"), entropy.ToString("F0")));

                if (!spell.incantationLines.NullOrEmpty())
                {
                    string quote = "\"\"\"\n" + string.Join("\n", spell.incantationLines) + "\n\"\"\"";

                    Color castInk = Color.Lerp(spell.themeColor, UI_Palette.BookInk, CastInkBlend)
                        .WithAlpha(CastInkAlpha);

                    float castH = Text.CalcHeight(quote, width);
                    DrawInk(new Rect(x, y, width, castH), quote, castInk, GameFont.Small, TextAnchor.UpperLeft);
                    y += castH + 6f;
                }
            }
            else if (entry.Def is FamiliaMyth.Abilities.SkillDef skill)
            {
                DrawFact(x, ref y, width,
                    "FamiliaMyth.AscendReward.SkillTypeLine".Translate(SkillTypeLabel(skill.skillType)));

                if (skill.cooldown > 0f)
                    DrawFact(x, ref y, width,
                        "FamiliaMyth.AscendReward.SkillCooldown".Translate(skill.cooldown.ToString("F0")));

                if (skill.maxCharge > 1)
                    DrawFact(x, ref y, width,
                        "FamiliaMyth.AscendReward.SkillCharge".Translate(skill.maxCharge.ToString()));

                if (skill.needTarget)
                    DrawFact(x, ref y, width, "FamiliaMyth.AscendReward.SkillNeedTarget".Translate());
            }
            else if (entry.Def is DevelopableAbilityDef ability)
            {
                DrawFact(x, ref y, width,
                    "FamiliaMyth.AscendReward.DevMaxProficiency".Translate(ability.maxProficiency.ToString("F0")));
            }
        }

        //信息行：高度随换行自适应，避免多行文本压到下一行
        private void DrawFact(float x, ref float y, float width, string text)
        {
            if (text.NullOrEmpty()) return;

            float height = Text.CalcHeight(text, width);
            DrawInk(new Rect(x, y, width, height), text, UI_Palette.BookInkSoft, GameFont.Small, TextAnchor.UpperLeft);
            y += height + 4f;
        }

        private void DrawHolders(string defName, float x, ref float y, float width)
        {
            List<Pawn> holders = Holders(defName);

            if (holders.Count == 0)
            {
                DrawInk(new Rect(x, y, width, 24f), "FamiliaMyth.AscendReward.NoHolder".Translate(),
                    UI_Palette.BookInkSoft, GameFont.Small);
                y += 24f;
                return;
            }

            string prefix = "FamiliaMyth.AscendReward.HolderPrefix".Translate();
            DrawInk(new Rect(x, y, width, 24f), prefix, UI_Palette.BookInkSoft, GameFont.Small);

            float cursorX = x + UI_Text.Width(prefix, GameFont.Small);
            for (int i = 0; i < holders.Count; i++)
            {
                if (i > 0)
                {
                    const string Separator = "、";
                    DrawInk(new Rect(cursorX, y, 20f, 24f), Separator, UI_Palette.BookInkSoft, GameFont.Small);
                    cursorX += UI_Text.Width(Separator, GameFont.Small);
                }

                string name = holders[i].Name.ToStringShort;
                float nameWidth = UI_Text.Width(name, GameFont.Small);
                DrawInk(new Rect(cursorX, y, nameWidth + 2f, 24f), name, HolderInk(holders[i]), GameFont.Small);
                cursorX += nameWidth + 2f;
            }

            y += 24f;
        }

        //趣闻：像读者留在页角的批注，直接写在右下角，不占边框
        private void DrawAnecdote(Rect body, string text)
        {
            float width = Mathf.Min(AnecdoteWidth, body.width * 0.42f);

            float height;
            using (UI_Scope.Text(GameFont.Tiny, TextAnchor.UpperLeft))
                height = Text.CalcHeight(text, width);

            Rect r = new(body.xMax - width, body.yMax - height, width, height);
            DrawInk(r, text, UI_Palette.BookInkSoft.WithAlpha(0.8f), GameFont.Tiny, TextAnchor.UpperLeft);
        }

        //TaggedString 字段不经过 XML 的换行转义，这里把字面 \n 还原为换行
        private static string ReplaceLineBreaks(string text)
            => text.NullOrEmpty() ? text : text.Replace("\\n", "\n");

        //每段首行缩进两个全角空格
        private static string IndentParagraphs(string text)
        {
            if (text.NullOrEmpty()) return text;

            string[] lines = text.Split('\n');
            for (int i = 0; i < lines.Length; i++)
                if (!lines[i].NullOrEmpty())
                    lines[i] = "　　" + lines[i];

            return string.Join("\n", lines);
        }

        //持有者名字用角色喜好色，向墨色混合以适配纸面
        private static Color HolderInk(Pawn pawn)
        {
            Color favorite = pawn?.story?.favoriteColor?.color ?? UI_Palette.BookInkSoft;
            return Color.Lerp(favorite, UI_Palette.BookInk, HolderInkBlend);
        }

        //==================== 页脚与翻页 ====================

        private void DrawFooter(Rect r, bool showBack)
        {
            DrawRule(new Rect(r.x, r.y, r.width, 1f));

            float rowY = r.y + 4f;
            Rect pageR = new(r.center.x - 80f, rowY, 160f, PageFooterRowHeight);

            if (showBack)
            {
                Rect back = new(r.x, rowY, FooterButtonWidth, PageFooterRowHeight);
                if (DrawTextButton(back, "FamiliaMyth.AscendReward.BackToIndex".Translate(), true))
                {
                    page = ViewPage.Index;
                    indexPage = Mathf.Clamp(detailReturnPage, 0, Mathf.Max(0, pages.Count - 1));
                }
            }
            else
            {
                Rect prev = new(r.x, rowY, FooterButtonWidth, PageFooterRowHeight);
                Rect next = new(r.xMax - FooterButtonWidth, rowY, FooterButtonWidth, PageFooterRowHeight);

                bool hasPrev = indexPage > 0;
                if (DrawTextButton(prev, "FamiliaMyth.AscendReward.PrevPage".Translate(), hasPrev))
                    indexPage--;

                bool hasNext = indexPage < pages.Count - 1;
                if (DrawTextButton(next, "FamiliaMyth.AscendReward.NextPage".Translate(), hasNext))
                    indexPage++;
            }

            int totalPages = Mathf.Max(1, pages.Count);
            int shownPage = showBack ? detailReturnPage + 1 : indexPage + 1;
            DrawInk(pageR, "FamiliaMyth.AscendReward.PageFooter".Translate(
                shownPage.ToString(), totalPages.ToString()), UI_Palette.BookInkSoft, GameFont.Tiny, TextAnchor.MiddleCenter);

            DrawInk(new Rect(r.x, r.y + 30f, r.width, EscHintHeight),
                "FamiliaMyth.AscendReward.EscHint".Translate(),
                UI_Palette.BookInkSoft.WithAlpha(0.55f), GameFont.Tiny, TextAnchor.MiddleRight);
        }

        //==================== 流构建与分页 ====================

        private void BuildFlow()
        {
            rows.Clear();
            pages.Clear();

            AscendRewardCatalog.EnsureBuilt();
            IReadOnlyList<AscendRewardCatalogEntry> all = AscendRewardCatalog.All;

            //编号按展示顺序连续分配，与分组顺序一致
            int number = 0;

            foreach (RewardType type in DisplayOrder)
            {
                List<AscendRewardCatalogEntry> group = new();
                for (int i = 0; i < all.Count; i++)
                    if (all[i].Type == type) group.Add(all[i]);

                if (group.Count == 0) continue;

                rows.Add(new FlowRow
                {
                    IsHeader = true,
                    Type = type,
                    Height = GroupHeaderHeight + GroupHeaderGap
                });

                for (int i = 0; i < group.Count; i += EntryColumns)
                {
                    bool hasRight = i + 1 < group.Count;
                    rows.Add(new FlowRow
                    {
                        Left = group[i],
                        LeftNumber = ++number,
                        Right = hasRight ? group[i + 1] : default,
                        RightNumber = hasRight ? ++number : 0,
                        HasRight = hasRight,
                        Height = EntryCardHeight + EntryCardGap
                    });
                }
            }
        }

        private void EnsurePages(float availableHeight)
        {
            if (pages.Count > 0 || rows.Count == 0) return;

            List<FlowRow> current = new();
            float used = 0f;
            for (int i = 0; i < rows.Count; i++)
            {
                FlowRow row = rows[i];
                if (used + row.Height > availableHeight && current.Count > 0)
                {
                    pages.Add(current);
                    current = new List<FlowRow>();
                    used = 0f;
                }

                current.Add(row);
                used += row.Height;
            }

            if (current.Count > 0) pages.Add(current);

            FixOrphanHeaders();
        }

        //页末只剩分组标题时把它挪到下一页，避免翻开的页只有一个标题
        private void FixOrphanHeaders()
        {
            for (int i = 0; i < pages.Count - 1; i++)
            {
                List<FlowRow> current = pages[i];
                if (current.Count == 0 || !current[current.Count - 1].IsHeader) continue;

                FlowRow header = current[current.Count - 1];
                current.RemoveAt(current.Count - 1);
                pages[i + 1].Insert(0, header);
            }
        }

        //==================== 查询与格式化 ====================

        private static bool IsCollected(string defName)
            => GameComponent_AscendRewardCollection.Instance?.IsCollected(defName) ?? false;

        private static int CollectedCount()
            => GameComponent_AscendRewardCollection.Instance?.CollectedCount ?? 0;

        private static string ProgressText()
            => "FamiliaMyth.AscendReward.Progress".Translate(
                CollectedCount().ToString(), AscendRewardCatalog.TotalCount.ToString());

        private static int SeenCount(RewardType type)
        {
            int n = 0;
            IReadOnlyList<AscendRewardCatalogEntry> all = AscendRewardCatalog.All;
            for (int i = 0; i < all.Count; i++)
                if (all[i].Type == type && IsCollected(all[i].DefName)) n++;
            return n;
        }

        //持有状态按 DefName 比对，不实例化奖励对象
        private bool IsHeld(string defName) => Holders(defName).Count > 0;

        private List<Pawn> Holders(string defName)
        {
            EnsureHolders();
            return defName != null && holderByDef.TryGetValue(defName, out List<Pawn> holders)
                ? holders
                : EmptyHolders;
        }

        //每帧一次全图扫描，按 defName 归类持有者
        private void EnsureHolders()
        {
            if (holderFrame == Time.frameCount) return;

            holderFrame = Time.frameCount;
            holderByDef.Clear();

            if (Find.CurrentMap == null) return;

            foreach (Pawn pawn in Find.CurrentMap.mapPawns.AllPawnsSpawned)
            {
                Hediff_Pharna pharna = pawn.GetPharna();
                if (pharna == null) continue;

                CollectHolders(pharna.spells, pawn);
                CollectHolders(pharna.skills, pawn);
                CollectHolders(pharna.developableAbilities, pawn);
            }
        }

        private void CollectHolders<T>(List<T> rewards, Pawn pawn) where T : class, IAscendReward
        {
            if (rewards == null) return;

            for (int i = 0; i < rewards.Count; i++)
            {
                string defName = rewards[i]?.DefName;
                if (defName.NullOrEmpty()) continue;

                if (!holderByDef.TryGetValue(defName, out List<Pawn> holders))
                {
                    holders = new List<Pawn>();
                    holderByDef[defName] = holders;
                }

                if (!holders.Contains(pawn)) holders.Add(pawn);
            }
        }

        private static string GroupLabel(RewardType type) => type switch
        {
            RewardType.DevelopableAbility => "FamiliaMyth.AscendReward.GroupDevelopable".Translate(),
            RewardType.Skill => "FamiliaMyth.AscendReward.GroupSkill".Translate(),
            RewardType.Spell => "FamiliaMyth.AscendReward.GroupSpell".Translate(),
            _ => "?"
        };

        private static string RarityLabel(RewardRarity rarity) => rarity switch
        {
            RewardRarity.Unique => "FamiliaMyth.UI.RarityUnique".Translate(),
            RewardRarity.Rare => "FamiliaMyth.UI.RarityRare".Translate(),
            RewardRarity.Uncommon => "FamiliaMyth.UI.RarityUncommon".Translate(),
            RewardRarity.Common => "FamiliaMyth.UI.RarityCommon".Translate(),
            _ => ""
        };

        //纸面配色下的稀有度：保留色相，向墨色混合以提高对比
        private static Color InkRarity(RewardRarity rarity)
            => Color.Lerp(RarityColor(rarity), UI_Palette.BookInk, InkRarityBlend);

        private static Color RarityColor(RewardRarity rarity) => rarity switch
        {
            RewardRarity.Common => UI_Palette.RarityCommon,
            RewardRarity.Uncommon => UI_Palette.RarityUncommon,
            RewardRarity.Rare => UI_Palette.RarityRare,
            RewardRarity.Unique => UI_Palette.RarityUnique,
            _ => Color.white
        };

        private static string SpellTypeLabel(SpellType type) => type switch
        {
            SpellType.Attack => "FamiliaMyth.AscendReward.SpellTypeAttack".Translate(),
            SpellType.Heal => "FamiliaMyth.AscendReward.SpellTypeHeal".Translate(),
            SpellType.Defense => "FamiliaMyth.AscendReward.SpellTypeDefense".Translate(),
            SpellType.Buff => "FamiliaMyth.AscendReward.SpellTypeBuff".Translate(),
            SpellType.Debuff => "FamiliaMyth.AscendReward.SpellTypeDebuff".Translate(),
            SpellType.Enchant => "FamiliaMyth.AscendReward.SpellTypeEnchant".Translate(),
            SpellType.Field => "FamiliaMyth.AscendReward.SpellTypeField".Translate(),
            SpellType.Summon => "FamiliaMyth.AscendReward.SpellTypeSummon".Translate(),
            SpellType.Special => "FamiliaMyth.AscendReward.SpellTypeSpecial".Translate(),
            _ => "FamiliaMyth.AscendReward.SpellTypeNone".Translate()
        };

        private static string SkillTypeLabel(FamiliaMyth.Abilities.SkillType type) => type switch
        {
            FamiliaMyth.Abilities.SkillType.Active => "FamiliaMyth.AscendReward.SkillTypeActive".Translate(),
            FamiliaMyth.Abilities.SkillType.Passive => "FamiliaMyth.AscendReward.SkillTypePassive".Translate(),
            _ => "FamiliaMyth.AscendReward.SkillTypeTriggered".Translate()
        };
    }
}
