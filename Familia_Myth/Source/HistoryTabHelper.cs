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

        [HarmonyPatch(typeof(MainTabWindow_History), nameof(MainTabWindow_History.PreOpen))]
        public static class Patch_MainTabWindow_History_PreOpen
        {
            public static void Postfix(MainTabWindow_History __instance)
            {
                var tabs = GetTabs(__instance);
                if (tabs.Any(t => t.label == "眷族")) return;

                tabs.Add(new("眷族",
                    () => SelectFamilia(),
                    () => IsFamiliaTabActive()));
            }
        }

        [HarmonyPatch(typeof(MainTabWindow_History), nameof(MainTabWindow_History.DoWindowContents))]
        public static class Patch_MainTabWindow_History_DoWindowContents
        {
            public static bool Prefix(MainTabWindow_History __instance, Rect rect)
            {
                if (!IsFamiliaTabActive()) return true;

                var r = rect;
                r.yMin += 45f;
                TabDrawer.DrawTabs(r, GetTabs(__instance));
                // 与 DoGraphPage / DoStatisticsPage 对齐：内容再下移 17px
                r.yMin += 17f;
                UI_FamiliaPage.Draw(r);
                return false;
            }
        }
    }
}
