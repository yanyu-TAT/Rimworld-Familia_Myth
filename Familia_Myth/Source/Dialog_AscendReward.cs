using System;
using System.Collections.Generic;
using FamiliaMyth.Pharna;
using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.UI
{
    public class Dialog_AscendReward : Window_FamiliaBase
    {
        private readonly Hediff_Pharna pharna;
        private readonly List<IAscendReward> rewards;

        private Vector2 scrollPosition;

        private class Hieroglyph
        {
            public string text;
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
        private const float ContentTop = 70f;
        private const float CardHeight = 80f;
        private const float CardSpacing = 10f;
        private const float CardPadding = 14f;

        //符文候选字符不足时的兜底字符集
        private const string FallbackSymbols = "神恩眷族冒险誓约永恒";

        private static readonly HashSet<char> symbolPool = new();
        private static char[] hieroglyphSymbols = Array.Empty<char>();

        //符文层铺满窗口，内容区即整个窗口
        protected override bool HasHeaderArea => false;

        public override Vector2 InitialSize => new(WindowWidth, WindowHeight);

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

            //从当前殖民者名字中收集字符作为符文候选
            var pawns = Find.CurrentMap.mapPawns.AllPawnsSpawned
                .Where(p => p.Faction == Faction.OfPlayer && p.IsColonist)
                .ToList();
            foreach (var pawn in pawns)
            {
                foreach (char c in pawn.Name.ToString())
                    symbolPool.Add(c);
            }
            if (symbolPool.Count == 0)
            {
                foreach (char c in FallbackSymbols)
                    symbolPool.Add(c);
            }
            hieroglyphSymbols = symbolPool.ToArray();

            for (int edge = 0; edge < 4; edge++)
            {
                for (int j = 0; j < GlyphsPerEdge; j++)
                {
                    hieroglyphs.Add(new Hieroglyph
                    {
                        text = hieroglyphSymbols[Rand.Range(0, hieroglyphSymbols.Length)].ToString(),
                        phase = Rand.Value * Mathf.PI * 2f,
                        speed = Rand.Range(0.3f, 0.8f),
                        size = Rand.Range(14f, 18f),
                        edge = edge,
                        index = j
                    });
                }
            }
        }

        protected override void DrawBackLayer(Rect inRect) => DrawHieroglyphs(inRect);

        protected override void DrawContent(Rect contentRect) => DrawRewardList(contentRect);

        private void DrawRewardList(Rect rect)
        {
            // 内容区域居中显示
            float contentX = rect.x + (rect.width - ContentWidth) / 2f;
            float contentHeight = rect.height - ContentTop - 20f;

            Rect contentRect = new(contentX, rect.y + ContentTop, ContentWidth, contentHeight);

            float scrollbarWidth = 20f;
            float totalHeight = rewards.Count * (CardHeight + CardSpacing);
            Rect viewRect = new(0, 0, contentRect.width - scrollbarWidth, totalHeight);

            Widgets.BeginScrollView(contentRect, ref scrollPosition, viewRect);
            UI_Cursor cur = new(0f, 0f, viewRect.width);
            foreach (var reward in rewards)
            {
                DrawRewardCard(cur.Row(CardHeight), reward);
                cur.Gap(CardSpacing);
            }
            Widgets.EndScrollView();
        }

        private void DrawHieroglyphs(Rect rect)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            float time = Time.realtimeSinceStartup;

            using (UI_Scope.Text(GameFont.Tiny, TextAnchor.MiddleCenter))
            {
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

                    using (UI_Scope.Color(new Color(1f, 0.85f, 0.4f, alpha)))
                    {
                        Rect glyphRect = new(x - glyph.size / 2, y - glyph.size / 2, glyph.size, glyph.size);
                        Widgets.Label(glyphRect, glyph.text);
                    }
                }
            }
        }

        private void DrawRewardCard(Rect cardRect, IAscendReward reward)
        {
            var color = reward.RarityColor;
            bool isMouseOver = Mouse.IsOver(cardRect);

            // 卡片背景
            Widgets.DrawBoxSolid(cardRect, UI_Palette.CardBg);
            if (isMouseOver)
                Widgets.DrawBoxSolid(cardRect, new Color(color.r, color.g, color.b, 0.12f));

            // 稀有度左边框
            Widgets.DrawBoxSolid(new Rect(cardRect.x, cardRect.y, 3f, cardRect.height), color);

            // 悬停时边框外发光
            if (isMouseOver)
                Widgets.DrawBoxSolid(new Rect(cardRect.x, cardRect.y, 5f, cardRect.height), new Color(color.r, color.g, color.b, 0.2f));

            // 标题行
            Rect titleRect = new(
                cardRect.x + CardPadding + 5f,
                cardRect.y + 10f,
                cardRect.width - CardPadding * 2 - 5f,
                26f);

            using (UI_Scope.Text(GameFont.Small, TextAnchor.MiddleLeft))
            using (UI_Scope.Color(isMouseOver ? Color.white : new Color(0.95f, 0.95f, 0.95f)))
                Widgets.Label(titleRect, GetRewardLabel(reward));

            // 稀有度标签（标题右侧）
            string rarityLabel = reward.Rarity switch
            {
                RewardRarity.Unique => (string)"FamiliaMyth.UI.RarityUnique".Translate(),
                RewardRarity.Rare => (string)"FamiliaMyth.UI.RarityRare".Translate(),
                RewardRarity.Uncommon => (string)"FamiliaMyth.UI.RarityUncommon".Translate(),
                RewardRarity.Common => (string)"FamiliaMyth.UI.RarityCommon".Translate(),
                _ => ""
            };
            using (UI_Scope.Text(GameFont.Small, TextAnchor.MiddleRight))
            using (UI_Scope.Color(color))
            {
                Rect rarityRect = new(titleRect.xMax - 70f, titleRect.y, 65f, titleRect.height);
                Widgets.Label(rarityRect, rarityLabel);
            }

            // 描述行
            Rect descRect = new(
                cardRect.x + CardPadding + 5f,
                cardRect.y + 40f,
                cardRect.width - CardPadding * 2 - 5f,
                35f);
            using (UI_Scope.Text(GameFont.Tiny, TextAnchor.UpperLeft))
            using (UI_Scope.Color(new Color(0.65f, 0.65f, 0.65f)))
                Widgets.Label(descRect, reward.Description ?? "");

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
                RewardType.Spell => (string)"FamiliaMyth.UI.RewardTypeSpell".Translate(),
                RewardType.Skill => (string)"FamiliaMyth.UI.RewardTypeSkill".Translate(),
                RewardType.DevelopableAbility => (string)"FamiliaMyth.UI.RewardTypeDevelopableAbility".Translate(),
                _ => "?"
            };

            return $"[{typeLabel}] {reward.Label}";
        }
    }
}
