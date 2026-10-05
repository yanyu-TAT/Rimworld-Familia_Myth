using System.Collections.Generic;
using FamiliaMyth.Familia;
using FamiliaMyth.Pharna;
using RimWorld;
using RimWorld.Planet;
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
        //文化图标卡：上下内边距 + 图标 + 间距 + 名称
        private const float IconBlockHeight = M * 2f + IconCardSize + 8f + IconNameHeight;
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

        //可升级提示的呼吸参数
        private const float PulseSpeed = 3.2f;
        private const float PulseFillMinAlpha = 0.05f;
        private const float PulseFillMaxAlpha = 0.12f;
        private const float PulseOutlineMinAlpha = 0.30f;
        private const float PulseOutlineMaxAlpha = 0.85f;
        //悬停反馈叠白强度
        private const float HoverOverlayAlpha = 0.04f;

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
                GUI.DrawTexture(iconR, ideo.Icon);
            }
            else
            {
                Widgets.DrawBoxSolid(iconR, UI_Palette.CardBg);
            }

            Rect nameR = new(c.x, iconR.yMax + 8f, c.width, IconNameHeight);
            using (UI_Scope.Text(GameFont.Small, TextAnchor.UpperCenter))
            using (UI_Scope.Color(accent))
            using (UI_Scope.Group(nameR))
            {
                string cultureName = ideo?.name ?? "FamiliaMyth.UnknownCulture".Translate();
                Widgets.Label(GenUI.AtZero(nameR), cultureName);
            }
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
            UI_Cursor cur = new(0f, 0f, view.width);
            foreach (var cat in cats)
            {
                DrawCategoryCard(cur.Row(CategoryCardHeight), cat, tracker);
                cur.Gap(CategoryCardGap);
            }
            Widgets.EndScrollView();
        }

        private static void DrawCategoryCard(Rect r, FamiliaCategoryDef cat, GameComponent_FamiliaRankTracker tracker)
        {
            UI_GroupBox.DrawPlainBg(r, accentColor: UI_GroupBox.ColorAccent);

            Rect c = new(r.x + 6f, r.y + M, r.width - 6f - M, r.height - 2f * M);

            var state = tracker?.GetOrAddState(cat);
            var curRank = state?.curRankDef;
            var nxRank = tracker.GetNextRank(cat);
            bool canRankUp = tracker.CanRankUp(cat);

            //分类名 + 当前等级 + 下一级
            Rect row1 = new(c.x, c.y, c.width, Text.SmallFontHeight);

            string catStr = cat.LabelCap;
            Rect catRect = new(row1.x, row1.y, Mathf.Min(UI_Text.Width(catStr, GameFont.Small) + 2f, row1.width * 0.28f), row1.height);
            using (UI_Scope.Text(GameFont.Small, TextAnchor.MiddleLeft))
            using (UI_Scope.Color(UI_GroupBox.ColorAccent))
            using (UI_Scope.Group(catRect))
                Widgets.Label(GenUI.AtZero(catRect), catStr);

            string curStr = curRank?.LabelCap ?? "—";
            string nextStr = nxRank != null ? ("→ " + nxRank.LabelCap) : "";

            float curW = UI_Text.Width(curStr, GameFont.Small) + 2f;
            float nextW = Mathf.Min(UI_Text.Width(nextStr, GameFont.Small) + 2f, 120f);
            Rect curRect = new(catRect.xMax + 6f, row1.y, curW, row1.height);
            Rect nextRect = new(row1.xMax - nextW, row1.y, nextW, row1.height);
            if (curRect.xMax > nextRect.xMin - 4f)
                curRect.xMax = nextRect.xMin - 4f;

            using (UI_Scope.Text(GameFont.Small, TextAnchor.MiddleLeft))
            using (UI_Scope.Group(curRect))
                Widgets.Label(GenUI.AtZero(curRect), UI_Text.Truncate(curStr, curRect.width, GameFont.Small));

            if (!string.IsNullOrEmpty(nextStr))
            {
                using (UI_Scope.Text(GameFont.Small, TextAnchor.MiddleRight))
                using (UI_Scope.Color(UI_Palette.TextSubtle))
                using (UI_Scope.Group(nextRect))
                    Widgets.Label(GenUI.AtZero(nextRect), UI_Text.Truncate(nextStr, nextRect.width, GameFont.Small));
            }

            //进度条（满级判定统一走 tracker：返回 <=0 即无更高评级）
            Rect row2 = new(c.x, row1.yMax + 4f, c.width, 20f);
            int cur = state?.curRankUpPoints ?? 0;
            int req = tracker.GetUpgradePointsRequired(cat);
            bool maxed = req <= 0;
            float frac = maxed ? 1f : Mathf.Clamp01((float)cur / req);
            string numStr = maxed ? "—" : $"{cur} / {req}";
            float numW = Mathf.Max(56f, UI_Text.Width(numStr) + 8f);
            Rect barR = new(row2.x, row2.y + 4f, row2.width - numW - 6f, 12f);
            Rect numR = new(barR.xMax + 6f, row2.y, numW, 20f);

            UI_Bar.Draw(barR, frac, maxed ? UI_Palette.Ready : UI_Palette.Accent);

            using (UI_Scope.Text(GameFont.Tiny, TextAnchor.MiddleRight))
            using (UI_Scope.Color(UI_Palette.TextNormal))
            using (UI_Scope.Group(numR))
                Widgets.Label(GenUI.AtZero(numR), numStr);

            //状态提示
            Rect row3 = new(c.x, row2.yMax + 4f, c.width, 18f);
            using (UI_Scope.Text(GameFont.Tiny, TextAnchor.MiddleLeft))
            {
                if (canRankUp)
                {
                    using (UI_Scope.Color(UI_Palette.Warning))
                        Widgets.Label(row3, "FamiliaMyth.UI.CategoryRankUpHint".Translate());
                }
                else if (maxed)
                {
                    using (UI_Scope.Color(UI_Palette.TextNormal))
                        Widgets.Label(row3, "FamiliaMyth.UI.CategoryMaxRank".Translate());
                }
                else
                {
                    using (UI_Scope.Color(UI_Palette.TextNormal))
                    {
                        int pct = Mathf.RoundToInt(frac * 100f);
                        Widgets.Label(row3, "FamiliaMyth.UI.CategoryProgress".Translate(pct));
                    }
                }
            }

            DrawCardFeedback(r, UI_Palette.Accent, canRankUp);
        }

        //卡片反馈：悬停叠白；可升级时叠呼吸描边与淡填充（仅 Repaint 绘制）
        private static void DrawCardFeedback(Rect r, Color accent, bool pulsing)
        {
            if (Mouse.IsOver(r))
                Widgets.DrawBoxSolid(r, new Color(1f, 1f, 1f, HoverOverlayAlpha));

            if (!pulsing || Event.current.type != EventType.Repaint)
                return;

            float k = (Mathf.Sin(Time.realtimeSinceStartup * PulseSpeed) + 1f) * 0.5f;
            Widgets.DrawBoxSolid(r, accent.WithAlpha(Mathf.Lerp(PulseFillMinAlpha, PulseFillMaxAlpha, k)));
            UI_GroupBox.DrawOutlinedBox(r,
                accent.WithAlpha(Mathf.Lerp(PulseOutlineMinAlpha, PulseOutlineMaxAlpha, k)),
                UI_GroupBox.SectionBorderThickness);
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
            UI_Cursor cur = new(0f, 0f, view.width);
            foreach (var p in members)
            {
                if (!rewardScrollByPawn.TryGetValue(p.thingIDNumber, out var rs))
                    rs = Vector2.zero;
                DrawMemberCard(cur.Row(MemberCardHeight), p.GetPharna(), ref rs);
                rewardScrollByPawn[p.thingIDNumber] = rs;
                cur.Gap(MemberCardGap);
            }
            Widgets.EndScrollView();
        }

        private static void DrawMemberCard(Rect r, Hediff_Pharna pharna, ref Vector2 rewardScrollPos)
        {
            Pawn pawn = pharna.pawn;
            Color accent = pawn.story?.favoriteColor?.color ?? UI_GroupBox.ColorAccent;
            UI_GroupBox.DrawPlainBg(r, accentColor: accent);

            using (UI_Scope.Group(r))
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
                    GUI.DrawTexture(pr, rt);
                }
                catch { }

                Rect nameR = new(pcR.x, pr.yMax + 6f, pcR.width, Text.SmallFontHeight * 2 + 2f);
                using (UI_Scope.Text(GameFont.Small, TextAnchor.UpperCenter))
                using (UI_Scope.Group(nameR))
                    Widgets.Label(GenUI.AtZero(nameR), UI_Text.Truncate(pawn.NameFullColored, nameR.width, GameFont.Small));

                Rect bioR = new(pcR.x, nameR.yMax + 2f, pcR.width, Text.LineHeightOf(GameFont.Tiny) * 2 + 2f);
                using (UI_Scope.Text(GameFont.Tiny, TextAnchor.UpperLeft))
                using (UI_Scope.Color(UI_GroupBox.ColorAccent))
                    Widgets.Label(bioR, "FamiliaMyth.UI.MemberBio".Translate(pharna.level));

                //头像列可点击：跳转并选中该眷族
                if (Mouse.IsOver(pcR))
                    Widgets.DrawBoxSolid(pcR, new Color(1f, 1f, 1f, HoverOverlayAlpha));
                TooltipHandler.TipRegion(pcR, () => "FamiliaMyth.UI.JumpToPawnTip".Translate(),
                    Gen.HashCombineInt(pawn.thingIDNumber, 197308));
                if (Widgets.ButtonInvisible(pcR))
                    JumpToPawn(pawn);

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

            DrawCardFeedback(r, accent, false);
        }

        //跳转并选中该眷族，随后关闭当前主标签页（与原版历史页跳转同一流程）
        private static void JumpToPawn(Pawn pawn)
        {
            if (pawn == null)
                return;

            GlobalTargetInfo target = new(pawn);
            if (!CameraJumper.CanJump(target))
                return;

            CameraJumper.TryJumpAndSelect(target);
            Find.MainTabsRoot.EscapeCurrentTab();
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
            UI_Cursor cur = new(0f, 0f, view.width);
            foreach (var reward in list)
            {
                DrawRewardCard(cur.Row(SmallCardH), reward);
                cur.Gap(SmallCardGap);
            }
            Widgets.EndScrollView();
        }

        private static void DrawRewardCard(Rect r, IAscendReward reward)
        {
            Widgets.DrawBoxSolid(r, UI_Palette.CardBg);
            Widgets.DrawBoxSolid(new Rect(r.x, r.y, 3f, r.height), reward.RarityColor);

            string prefix = reward.RewardType switch
            {
                RewardType.DevelopableAbility => "★",
                RewardType.Skill => "✦",
                RewardType.Spell => "✧",
                _ => "•"
            };

            Rect labelR = new(r.x + 8f, r.y, r.width - 10f, r.height);
            using (UI_Scope.Text(GameFont.Tiny, TextAnchor.MiddleLeft))
            using (UI_Scope.Color(reward.RarityColor))
            using (UI_Scope.Group(labelR))
                Widgets.Label(GenUI.AtZero(labelR), UI_Text.Truncate($"{prefix} {reward.Label}", labelR.width));

            TooltipHandler.TipRegion(r, () => $"{reward.Label}\n\n{reward.Description}",
                Gen.HashCombineInt(reward.GetHashCode(), 197307));

            if (Mouse.IsOver(r))
                Widgets.DrawBoxSolid(r, new Color(1f, 1f, 1f, HoverOverlayAlpha));
        }
    }
}
