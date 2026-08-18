using FamiliaMyth.Familia;
using FamiliaMyth.Pharna;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace FamiliaMyth.UI
{
    public static class UI_FamiliaPage
    {
        private static Vector2 categoryScrollPos;
        private static Vector2 memberScrollPos;
        private static readonly Dictionary<int, Vector2> rewardScrollByPawn = new();

        private const float IconCardSize = 120f;
        private const float IconNameHeight = 24f;
        private const float IconBlockHeight = IconCardSize + 8f + IconNameHeight;
        private const float ColumnGap = 12f;
        private const float CategoryColumnWidth = 360f;
        private const float CategoryCardHeight = 90f;
        private const float CategoryCardGap = 8f;
        private const float MemberCardHeight = 232f;
        private const float MemberCardGap = 10f;
        private const float PortraitColW = 110f;
        private const float PortraitW = 92f;
        private const float PortraitH = 124f;
        private const float RadarColW = 178f;
        private const float SmallCardH = 28f;
        private const float SmallCardGap = 3f;
        private const float M = 6f;

        public static void Draw(Rect r)
        {
            Rect body = r;

            Rect iconBlock = new(body.x, body.y, CategoryColumnWidth, IconBlockHeight);
            DrawIconCard(iconBlock);

            Rect catList = new(body.x, iconBlock.yMax + 8f, CategoryColumnWidth, body.yMax - (iconBlock.yMax + 8f));
            DrawCategories(catList);

            Rect memList = new(body.x + CategoryColumnWidth + ColumnGap, body.y,
                body.width - CategoryColumnWidth - ColumnGap, body.height);
            DrawMembers(memList);
        }

        private static void DrawIconCard(Rect r)
        {
            Ideo ideo = Find.FactionManager?.OfPlayer?.ideos?.PrimaryIdeo;
            Color accent = ideo?.Color ?? UI_GroupBox.ColorAccent;
            UI_GroupBox.DrawPlainBg(r, accentColor: accent);

            Rect c = new(r.x + M, r.y + M, r.width - 2f * M, r.height - 2f * M);

            Rect iconR = new(c.x + (c.width - IconCardSize) / 2f, c.y, IconCardSize, IconCardSize);
            if (ideo != null && ideo.Icon != null)
            {
                GUI.color = Color.white;
                GUI.DrawTexture(iconR, ideo.Icon);
            }
            else
            {
                Widgets.DrawBoxSolid(iconR, new Color(0.15f, 0.15f, 0.18f, 0.9f));
            }

            Rect nameR = new(c.x, iconR.yMax + 8f, c.width, IconNameHeight);
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperCenter;
            GUI.color = accent;
            GUI.BeginGroup(nameR);
            Widgets.Label(GenUI.AtZero(nameR), ideo?.name ?? "未知文化");
            GUI.EndGroup();
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
        }

        private static void DrawCategories(Rect r)
        {
            var tracker = GameComponent_FamiliaRankTracker.Instance;
            var cats = DefDatabase<FamiliaCategoryDef>.AllDefs
                .OrderBy(c => c.sortOrder)
                .ToList();

            float total = cats.Count * (CategoryCardHeight + CategoryCardGap);
            Rect view = new(0f, 0f, r.width, Mathf.Max(total, r.height));

            Widgets.BeginScrollView(r, ref categoryScrollPos, view, showScrollbars: false);
            float y = 0f;
            foreach (var cat in cats)
            {
                DrawCategoryCard(new(0f, y, view.width, CategoryCardHeight), cat, tracker);
                y += CategoryCardGap + CategoryCardHeight;
            }
            Widgets.EndScrollView();
        }

        private static void DrawCategoryCard(Rect r, FamiliaCategoryDef cat, GameComponent_FamiliaRankTracker tracker)
        {
            UI_GroupBox.DrawPlainBg(r, accentColor: UI_GroupBox.ColorAccent);

            Rect c = new(r.x + 6f, r.y + M, r.width - 6f - M, r.height - 2f * M);

            var state = tracker?.GetState(cat);
            var curRank = state?.curRankDef;
            var nxRank = tracker.GetNextRank(cat);

            //分类名 + 当前等级 + 下一级
            Rect row1 = new(c.x, c.y, c.width, Text.SmallFontHeight);

            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleLeft;
            GUI.color = UI_GroupBox.ColorAccent;
            string catStr = cat.LabelCap;
            Vector2 catSz = Text.CalcSize(catStr);
            Rect catRect = new(row1.x, row1.y, Mathf.Min(catSz.x + 2f, row1.width * 0.28f), row1.height);
            GUI.BeginGroup(catRect);
            Widgets.Label(GenUI.AtZero(catRect), catStr);
            GUI.EndGroup();

            GUI.color = Color.white;
            string curStr = curRank?.LabelCap ?? "—";
            Vector2 curSz = Text.CalcSize(curStr);
            float curX = catRect.xMax + 6f;
            Rect curRect = new(curX, row1.y, curSz.x + 2f, row1.height);

            string nextStr = nxRank != null ? ("→ " + nxRank.LabelCap) : "";
            Vector2 nextSz = Text.CalcSize(nextStr);
            float nextW = Mathf.Min(nextSz.x + 2f, 120f);
            Rect nextRect = new(row1.xMax - nextW, row1.y, nextW, row1.height);

            if (curRect.xMax > nextRect.xMin - 4f)
                curRect.xMax = nextRect.xMin - 4f;

            GUI.BeginGroup(curRect);
            Widgets.Label(GenUI.AtZero(curRect), curStr);
            GUI.EndGroup();

            if (!string.IsNullOrEmpty(nextStr))
            {
                GUI.color = new Color(0.82f, 0.82f, 0.9f);
                Text.Anchor = TextAnchor.MiddleRight;
                GUI.BeginGroup(nextRect);
                Widgets.Label(GenUI.AtZero(nextRect), nextStr);
                GUI.EndGroup();
            }
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;

            //进度条
            Rect row2 = new(c.x, row1.yMax + 4f, c.width, 20f);
            int cur = state?.curRankUpPoints ?? 0;
            int req = nxRank?.upgradePointsRequired ?? 0;
            float frac = req > 0 ? Mathf.Clamp01((float)cur / req) : 0f;
            Text.Font = GameFont.Tiny;
            string numStr = $"{cur} / {req}";
            Vector2 numSz = Text.CalcSize(numStr);
            float numW = Mathf.Max(56f, numSz.x + 8f);
            Rect barR = new(row2.x, row2.y, row2.width - numW - 6f, 20f);
            Rect numR = new(barR.xMax + 6f, row2.y, numW, 20f);
            Widgets.DrawBoxSolid(barR, new Color(0.12f, 0.12f, 0.15f, 0.9f));
            Widgets.FillableBar(barR, frac);
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.MiddleRight;
            GUI.color = new Color(0.9f, 0.9f, 0.95f);
            GUI.BeginGroup(numR);
            Widgets.Label(GenUI.AtZero(numR), numStr);
            GUI.EndGroup();
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;

            //状态提示
            Rect row3 = new(c.x, row2.yMax + 4f, c.width, 18f);
            Text.Font = GameFont.Tiny;
            if (tracker.CanRankUp(cat))
            {
                GUI.color = new Color(1f, 0.75f, 0.2f);
                Widgets.Label(row3, "◆ 完成强制任务即可升级评级");
            }
            else if (req <= 0)
            {
                GUI.color = new Color(0.7f, 0.7f, 0.8f);
                Widgets.Label(row3, "此方向已无更高评级");
            }
            else
            {
                GUI.color = new Color(0.8f, 0.8f, 0.9f);
                int pct = Mathf.RoundToInt(frac * 100f);
                Widgets.Label(row3, $"进度 {pct}%");
            }
            GUI.color = Color.white;
        }

        private static void DrawMembers(Rect r)
        {
            if (Find.CurrentMap == null) return;

            List<Pawn> members = new();
            foreach (var p in Find.CurrentMap.mapPawns.AllPawnsSpawned)
                if (p.IsColonist && p.Faction == Faction.OfPlayerSilentFail && p.GetPharna() != null)
                    members.Add(p);

            var aliveIds = new HashSet<int>(members.Select(p => p.thingIDNumber));
            var deadKeys = rewardScrollByPawn.Keys.Where(k => !aliveIds.Contains(k)).ToList();
            foreach (var k in deadKeys) rewardScrollByPawn.Remove(k);

            float total = members.Count * (MemberCardHeight + MemberCardGap);
            Rect view = new(0f, 0f, r.width, Mathf.Max(r.height, total));

            Widgets.BeginScrollView(r, ref memberScrollPos, view, showScrollbars: false);
            float y = 0f;
            foreach (var p in members)
            {
                if (!rewardScrollByPawn.TryGetValue(p.thingIDNumber, out var rs))
                    rs = Vector2.zero;
                DrawMemberCard(new(0f, y, view.width, MemberCardHeight), p.GetPharna(), ref rs);
                rewardScrollByPawn[p.thingIDNumber] = rs;
                y += MemberCardHeight + MemberCardGap;
            }
            Widgets.EndScrollView();
        }

        private static void DrawMemberCard(Rect r, Hediff_Pharna pharna, ref Vector2 rewardScrollPos)
        {
            Pawn pawn = pharna.pawn;
            Color accent = pawn.story?.favoriteColor?.color ?? UI_GroupBox.ColorAccent;
            UI_GroupBox.DrawPlainBg(r, accentColor: accent);

            GUI.BeginGroup(r);
            try
            {
                Rect c = new(M, M, r.width - 2f * M, r.height - 2f * M);

                Rect pcR = new(c.x, c.y, PortraitColW, c.height);
                float colX2 = pcR.xMax + 8f;
                Rect rcR = new(colX2, c.y, RadarColW, c.height);
                float rewardW = Mathf.Max(120f, c.xMax - (rcR.xMax + 8f));
                Rect rwR = new(rcR.xMax + 8f, c.y, rewardW, c.height);

                //头像 + 姓名 + 等级伟业
                Rect pr = new(pcR.x + (pcR.width - PortraitW) / 2f, pcR.y, PortraitW, PortraitH);
                try
                {
                    var rt = PortraitsCache.Get(pawn, new Vector2(PortraitW, PortraitH),
                        Rot4.South, Vector3.zero, 1f, true, true, true, true, null, null, false, null);
                    GUI.color = Color.white;
                    GUI.DrawTexture(pr, rt);
                }
                catch { }

                Rect nameR = new(pcR.x, pr.yMax + 6f, pcR.width, Text.SmallFontHeight * 2 + 2f);
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.UpperCenter;
                GUI.color = Color.white;
                GUI.BeginGroup(nameR);
                Widgets.Label(GenUI.AtZero(nameR), pawn.NameFullColored);
                GUI.EndGroup();

                Rect bioR = new(pcR.x, nameR.yMax + 2f, pcR.width, Text.LineHeightOf(GameFont.Tiny) * 2 + 2f);
                Text.Font = GameFont.Tiny;
                Text.Anchor = TextAnchor.UpperLeft;
                GUI.color = UI_GroupBox.ColorAccent;
                Widgets.Label(bioR, $"Lv.{pharna.level}\n伟业：{(int)pharna.exploits}");
                GUI.color = Color.white;
                Text.Anchor = TextAnchor.UpperLeft;

                //雷达图
                Rect radarInner = new(rcR.x + 10f, rcR.y + Mathf.Max(0f, (rcR.height - RadarColW) / 2f) + 10f,
                    rcR.width - 20f, rcR.height - 20f - Mathf.Max(0f, (rcR.height - RadarColW) / 2f));
                float rsSide = Mathf.Min(radarInner.width, radarInner.height);
                if (rsSide <= 10f) rsSide = 10f;
                radarInner = new Rect(
                    radarInner.x + (radarInner.width - rsSide) / 2f,
                    radarInner.y + (radarInner.height - rsSide) / 2f,
                    rsSide, rsSide);
                (string label, float value)[] list = new (string, float)[6];
                for (int i = 0; i < 6; i++)
                {
                    list[i].value = pharna.GetCurrentStatus((PharnaStatusType)i);
                    list[i].label = Hediff_Pharna.GetStatusName((PharnaStatusType)i);
                }
                UI_RadarChart.Draw(radarInner, list, 12f);

                //奖励卡
                DrawRewards(rwR, pharna, ref rewardScrollPos);
            }
            finally { GUI.EndGroup(); }
        }

        private static void DrawRewards(Rect r, Hediff_Pharna pharna, ref Vector2 scroll)
        {
            List<IAscendReward> list = new();
            if (pharna.spells != null) list.AddRange(pharna.spells);
            if (pharna.skills != null) list.AddRange(pharna.skills);
            if (pharna.developableAbilities != null) list.AddRange(pharna.developableAbilities);
            list.Sort((a, b) => ((int)b.Rarity).CompareTo((int)a.Rarity));

            float total = list.Count * (SmallCardH + SmallCardGap);
            Rect view = new(0f, 0f, r.width, Mathf.Max(r.height, total));

            Widgets.BeginScrollView(r, ref scroll, view, showScrollbars: false);
            float y = 0f;
            foreach (var reward in list)
            {
                DrawRewardCard(new(0f, y, view.width, SmallCardH), reward);
                y += SmallCardH + SmallCardGap;
            }
            Widgets.EndScrollView();
        }

        private static void DrawRewardCard(Rect r, IAscendReward reward)
        {
            Widgets.DrawBoxSolid(r, new Color(20f / 255f, 22f / 255f, 30f / 255f, 0.95f));
            Widgets.DrawBoxSolid(new Rect(r.x, r.y, 3f, r.height), reward.RarityColor);

            string prefix = reward.RewardType switch
            {
                RewardType.DevelopableAbility => "★",
                RewardType.Skill => "✦",
                RewardType.Spell => "✧",
                _ => "•"
            };

            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.MiddleLeft;
            GUI.color = reward.RarityColor;
            Rect labelR = new(r.x + 8f, r.y, r.width - 10f, r.height);
            GUI.BeginGroup(labelR);
            Widgets.Label(GenUI.AtZero(labelR), $"{prefix} {reward.Label}");
            GUI.EndGroup();
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;

            TooltipHandler.TipRegion(r, new TipSignal($"{reward.Label}\n\n{reward.Description}"));
        }
    }
}