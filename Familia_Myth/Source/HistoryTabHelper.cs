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

        public static bool IsFamiliaTabActive()
        {
            object obj = curTabField.GetValue(null);
            return obj != null && (byte)Convert.ToInt32(obj) == Sentinel;
        }

        public static void SelectFamilia()
        {
            object obj = Enum.ToObject(curTabField.FieldType, Sentinel);
            curTabField?.SetValue(null, obj);
        }

        public static List<TabRecord> GetTabs(MainTabWindow_History window) =>
            tabsField?.GetValue(window) as List<TabRecord> ?? new List<TabRecord>();
    }
}

namespace FamiliaMyth.Patch
{
    [HarmonyPatch(typeof(MainTabWindow_History), nameof(MainTabWindow_History.PreOpen))]
    public static class Patch_MainTabWindow_History_PreOpen
    {
        public static void Postfix(MainTabWindow_History __instance)
        {
            try
            {
                var tabs = HistoryTabHelper.GetTabs(__instance);
                string familiaTabLabel = "FamiliaMyth.UI.FamiliaTab".Translate();
                if (tabs.Any(t => t.label == familiaTabLabel)) return;

                tabs.Add(new(familiaTabLabel,
                    () => HistoryTabHelper.SelectFamilia(),
                    () => HistoryTabHelper.IsFamiliaTabActive()));
            }
            catch (Exception ex)
            {
                Log.Error($"[FamiliaMyth] {nameof(Patch_MainTabWindow_History_PreOpen)} error: {ex}");
            }
        }
    }

    [HarmonyPatch(typeof(MainTabWindow_History), nameof(MainTabWindow_History.DoWindowContents))]
    public static class Patch_MainTabWindow_History_DoWindowContents
    {
        public static bool Prefix(MainTabWindow_History __instance, Rect rect)
        {
            try
            {
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
                Log.Error($"[FamiliaMyth] {nameof(Patch_MainTabWindow_History_DoWindowContents)} error: {ex}");
                return true; //出错则回退原版历史页绘制
            }
        }
    }
}
