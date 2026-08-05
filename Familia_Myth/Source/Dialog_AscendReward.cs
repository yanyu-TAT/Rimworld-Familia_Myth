using FamiliaMyth.Pharna;
using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.UI
{
    public class Dialog_AscendReward : Window
    {
        private readonly Hediff_Pharna pharna;
        private readonly List<IAscendReward> rewards;

        private Vector2 scrollPosition;

        private class Hieroglyph
        {
            public char symbol;
            public float phase;
            public float speed;
            public float size;
            public int edge;
            public int index;
        }
        private readonly List<Hieroglyph> hieroglyphs = new();

        private const int GlyphsPerEdge = 50;
        private const float GlyphMargin = 20f;

        private const float WindowWidth = 660f;
        private const float WindowHeight = 620f;
        private const float ContentWidth = 540f;
        private const float CardHeight = 80f;
        private const float CardSpacing = 10f;
        private const float CardPadding = 14f;

        private static char[] HieroglyphSymbols = "".ToArray();

        public Dialog_AscendReward(Hediff_Pharna pharna, List<IAscendReward> rewards)
        {
            this.pharna = pharna;
            List<string> rewardNames = pharna.GetRewardNames().ToList();
            this.rewards = rewards.Where(r => !rewardNames.Contains(r.GetType().Name)).ToList();

            forcePause = true;
            doCloseX = false;
            absorbInputAroundWindow = true;
            closeOnAccept = false;
            closeOnCancel = false;
            InitHieroglyphs();
        }

        private void InitHieroglyphs()
        {
            hieroglyphs.Clear();
            //从当前殖民者名字中获取字符作为神圣符文的候选字符
            var pawns = Find.CurrentMap.mapPawns.AllPawnsSpawned
                .Where(p => p.Faction == Faction.OfPlayer && p.IsColonist)
                .ToList();
            foreach ( var pawn in pawns)
            {
                string name = pawn.Name.ToString();
                foreach (char c in name)
                {
                    if (!HieroglyphSymbols.Contains(c))
                        HieroglyphSymbols = HieroglyphSymbols.Append(c).ToArray();
                }
            }
            for (int edge = 0; edge < 4; edge++)
            {
                for (int j = 0; j < GlyphsPerEdge; j++)
                {
                    hieroglyphs.Add(new Hieroglyph
                    {
                        symbol = HieroglyphSymbols[Rand.Range(0, HieroglyphSymbols.Length)],
                        phase = Rand.Value * Mathf.PI * 2f,
                        speed = Rand.Range(0.3f, 0.8f),
                        size = Rand.Range(14f, 18f),
                        edge = edge,
                        index = j
                    });
                }
            }
        }

        public override Vector2 InitialSize => new(WindowWidth, WindowHeight);

        public override void DoWindowContents(Rect rect)
        {
            // 先绘制神圣符文（在最底层）
            DrawHieroglyphs(rect);
            // 绘制窗口内容
            DrawContent(rect);
        }

        private void DrawContent(Rect rect)
        {
            // 内容区域居中显示
            float contentX = rect.x + (rect.width - ContentWidth) / 2f;
            float contentTop = 70f;
            float contentHeight = rect.height - contentTop - 20f;
            
            Rect contentRect = new(contentX, rect.y + contentTop, ContentWidth, contentHeight);

            float scrollbarWidth = 20f;
            float totalHeight = rewards.Count * (CardHeight + CardSpacing);
            Rect viewRect = new(0, 0, contentRect.width - scrollbarWidth, totalHeight);

            Widgets.BeginScrollView(contentRect, ref scrollPosition, viewRect);
            float curY = 0f;
            foreach (var reward in rewards)
            {
                Rect cardRect = new(0, curY, viewRect.width, CardHeight);
                DrawRewardCard(cardRect, reward);
                curY += CardHeight + CardSpacing;
            }
            Widgets.EndScrollView();
        }

        private void DrawHieroglyphs(Rect rect)
        {
            float time = Time.realtimeSinceStartup;
            GameFont prevFont = Text.Font;
            TextAnchor prevAnchor = Text.Anchor;

            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.MiddleCenter;

            for (int i = 0; i < hieroglyphs.Count; i++)
            {
                var glyph = hieroglyphs[i];
                float t = (time * glyph.speed + glyph.phase) % (Mathf.PI * 2f);

                float alpha = Mathf.Sin(t) * 0.4f + 0.4f;
                float floatOffset = Mathf.Sin(t * 2f) * 4f;

                float x, y;

                switch (glyph.edge)
                {
                    case 0: // 上边
                    {
                        float usableWidth = rect.width - GlyphMargin * 2f;
                        float spacing = usableWidth / (GlyphsPerEdge - 1);
                        x = rect.x + GlyphMargin + spacing * glyph.index;
                        y = rect.y + GlyphMargin + floatOffset;
                        break;
                    }
                    case 1: // 右边
                    {
                        float usableHeight = rect.height - GlyphMargin * 2f;
                        float spacing = usableHeight / (GlyphsPerEdge - 1);
                        x = rect.xMax - GlyphMargin + floatOffset;
                        y = rect.y + GlyphMargin + spacing * glyph.index;
                        break;
                    }
                    case 2: // 下边
                    {
                        float usableWidth = rect.width - GlyphMargin * 2f;
                        float spacing = usableWidth / (GlyphsPerEdge - 1);
                        x = rect.x + GlyphMargin + spacing * glyph.index;
                        y = rect.yMax - GlyphMargin + floatOffset;
                        break;
                    }
                    default: // 左边
                    {
                        float usableHeight = rect.height - GlyphMargin * 2f;
                        float spacing = usableHeight / (GlyphsPerEdge - 1);
                        x = rect.x + GlyphMargin + floatOffset;
                        y = rect.y + GlyphMargin + spacing * glyph.index;
                        break;
                    }
                }

                // 主字符：金色
                Color glyphColor = new(1f, 0.85f, 0.4f, alpha);
                GUI.color = glyphColor;
                
                Rect glyphRect = new(x - glyph.size / 2, y - glyph.size / 2, 
                    glyph.size, glyph.size);
                Widgets.Label(glyphRect, glyph.symbol.ToString());
            }

            GUI.color = Color.white;
            Text.Font = prevFont;
            Text.Anchor = prevAnchor;
        }

        private void DrawRewardCard(Rect cardRect, IAscendReward reward)
        {
            var color = reward.RarityColor;

            // 卡片背景
            Widgets.DrawBoxSolid(cardRect, new Color(0.1f, 0.1f, 0.13f, 0.92f));

            bool isMouseOver = Mouse.IsOver(cardRect);
            if (isMouseOver)
            {
                Widgets.DrawBoxSolid(cardRect, new Color(color.r, color.g, color.b, 0.12f));
            }

            // 稀有度左边框
            Rect borderRect = new(cardRect.x, cardRect.y, 3f, cardRect.height);
            Widgets.DrawBoxSolid(borderRect, color);

            // 悬停时边框外发光
            if (isMouseOver)
            {
                Rect glowRect = new(cardRect.x, cardRect.y, 5f, cardRect.height);
                Widgets.DrawBoxSolid(glowRect, new Color(color.r, color.g, color.b, 0.2f));
            }

            // 标题行
            Rect titleRect = new(
                cardRect.x + CardPadding + 5f,
                cardRect.y + 10f,
                cardRect.width - CardPadding * 2 - 5f,
                26f);

            Text.Font = GameFont.Small;
            GUI.color = isMouseOver ? Color.white : new(0.95f, 0.95f, 0.95f);
            Widgets.Label(titleRect, GetRewardLabel(reward));

            // 稀有度标签（标题右侧）
            GUI.color = color;
            Text.Anchor = TextAnchor.MiddleRight;
            Rect rarityRect = new(titleRect.xMax - 70f, titleRect.y, 65f, titleRect.height);
            Widgets.Label(rarityRect, reward.Rarity switch
            {
                RewardRarity.Unique => "★ 独有",
                RewardRarity.Rare => "★ 罕见",
                RewardRarity.Uncommon => "★ 稀有",
                RewardRarity.Common => "★ 通常",
                _ => ""
            });
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;

            // 描述行
            Rect descRect = new(
                cardRect.x + CardPadding + 5f,
                cardRect.y + 40f,
                cardRect.width - CardPadding * 2 - 5f,
                35f);
            Text.Font = GameFont.Tiny;
            GUI.color = new Color(0.65f, 0.65f, 0.65f);
            Widgets.Label(descRect, reward.Description ?? "");
            GUI.color = Color.white;
            Text.Font = GameFont.Small;

            if (Widgets.ButtonInvisible(cardRect))
            {
                reward.RewardInitialize();
                reward.Apply(pharna);
                Close();
            }
        }

        private static string GetRewardLabel(IAscendReward reward)
        {
            string typeLabel = reward.RewardType switch
            {
                RewardType.Spell => "魔法",
                RewardType.Skill => "技能",
                RewardType.DevelopableAbility => "发展能力",
                _ => "?"
            };

            return $"[{typeLabel}] {reward.Label}";
        }
    }
}
