using FamiliaMyth.UI;
using HarmonyLib;
using RimWorld;
using System.Reflection;
using UnityEngine;
using Verse;

namespace FamiliaMyth.UI
{
    public static class HistoryTabHelper
    {
        private const byte Sentinel = 99;

        private static readonly FieldInfo curTabField =
            AccessTools.Field(typeof(MainTabWindow_History), "curTab");
        private static readonly FieldInfo tabsField =
            AccessTools.Field(typeof(MainTabWindow_History), "tabs");

        //原版字段缺失或哨兵值被占用时停用自建页签，回退原版历史页
        public static readonly bool Usable = ResolveUsable();

        private static bool ResolveUsable()
        {
            if (curTabField == null || tabsField == null)
            {
                Log.Error($"[FamiliaMyth] {nameof(HistoryTabHelper)}: 历史窗口字段缺失，眷族页签停用");
                return false;
            }

            try
            {
                if (Enum.IsDefined(curTabField.FieldType, Sentinel))
                {
                    Log.Error($"[FamiliaMyth] {nameof(HistoryTabHelper)}: 历史页签枚举已占用 {Sentinel}，眷族页签停用");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[FamiliaMyth] {nameof(HistoryTabHelper)}: 页签类型校验失败，眷族页签停用：{ex}");
                return false;
            }

            return true;
        }

        public static bool IsFamiliaTabActive()
        {
            if (!Usable)
                return false;

            object obj = curTabField.GetValue(null);
            return obj != null && (byte)Convert.ToInt32(obj) == Sentinel;
        }

        public static void SelectFamilia()
        {
            if (!Usable)
                return;

            object obj = Enum.ToObject(curTabField.FieldType, Sentinel);
            curTabField.SetValue(null, obj);
        }

        public static List<TabRecord> GetTabs(MainTabWindow_History window) =>
            tabsField?.GetValue(window) as List<TabRecord> ?? new List<TabRecord>();
    }
}

namespace FamiliaMyth.Patch
{
    [HarmonyPatch(typeof(MainTabWindow_History), nameof(MainTabWindow_History.PreOpen))]
    public static class Patch_FamiliaTab_PreOpen
    {
        public static void Postfix(MainTabWindow_History __instance)
        {
            try
            {
                if (!HistoryTabHelper.Usable) return;

                var tabs = HistoryTabHelper.GetTabs(__instance);
                string familiaTabLabel = "FamiliaMyth.UI.FamiliaTab".Translate();
                if (tabs.Any(t => t.label == familiaTabLabel)) return;

                tabs.Add(new(familiaTabLabel,
                    () => HistoryTabHelper.SelectFamilia(),
                    () => HistoryTabHelper.IsFamiliaTabActive()));
            }
            catch (Exception ex)
            {
                Log.Error($"[FamiliaMyth] {nameof(Patch_FamiliaTab_PreOpen)} error: {ex}");
            }
        }
    }

    [HarmonyPatch(typeof(MainTabWindow_History), nameof(MainTabWindow_History.DoWindowContents))]
    public static class Patch_FamiliaTab_Draw
    {
        public static bool Prefix(MainTabWindow_History __instance, Rect rect)
        {
            try
            {
                if (!HistoryTabHelper.Usable) return true;
                if (!HistoryTabHelper.IsFamiliaTabActive()) return true;

                var r = rect;
                r.yMin += 45f;
                TabDrawer.DrawTabs(r, HistoryTabHelper.GetTabs(__instance));
                // 与 DoGraphPage / DoStatisticsPage 对齐：内容再下移 17px
                r.yMin += 17f;
                UI_FamiliaPage.Draw(r);
                return false;
            }
            catch (Exception ex)
            {
                Log.Error($"[FamiliaMyth] {nameof(Patch_FamiliaTab_Draw)} error: {ex}");
                return true; //出错则回退原版历史页绘制
            }
        }
    }
}
