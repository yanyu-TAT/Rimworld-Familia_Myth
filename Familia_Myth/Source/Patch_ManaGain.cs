using System.Reflection;
using FamiliaMyth.Abilities;
using FamiliaMyth.Pharna;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace FamiliaMyth.Patch
{
    [HarmonyPatch(typeof(Pawn_PsychicEntropyTracker), nameof(Pawn_PsychicEntropyTracker.TryAddEntropy))]
    public static class Patch_ManaGain_Entropy
    {
        private static DevelopableAbilityDef magicDef;
        private static DevelopableAbilityDef MagicDef => magicDef ??= DefDatabase<DevelopableAbilityDef>.GetNamed("DevelopableAbility_Magic");
        public static void Prefix(ref float __state, Pawn_PsychicEntropyTracker __instance)
        {
            try
            {
                __state = __instance.EntropyValue;
            }
            catch (Exception ex)
            {
                //置极大值，令 Postfix 的 amount 恒为负而直接跳过结算
                __state = float.MaxValue;
                Log.Error($"[FamiliaMyth] {nameof(Patch_ManaGain_Entropy)} prefix error: {ex}");
            }
        }

        public static void Postfix(Pawn_PsychicEntropyTracker __instance, float __state)
        {
            try
            {
                if (float.IsNaN(__state) || __state > 1e9f) return; //Prefix 未取到有效初始值
                float amount = __instance.EntropyValue - __state;
                if (amount <= 0) return;
                var pawn = __instance.Pawn;
                var pharna = pawn.GetPharna();
                if (pharna == null) return;

                //每点心灵熵0.1点
                pharna.GainStatus(PharnaStatusType.Mana, amount * 0.1f);

                //魔导（无该发展能力时跳过）
                var magic = pharna.developableAbilities?.FirstOrDefault(p => p.def == MagicDef);
                //每点心灵熵0.05点
                magic?.GainProficiency(amount * DevelopableAbility_Magic.ProfGainPerEntropy);
            }
            catch (Exception ex)
            {
                Log.Error($"[FamiliaMyth] {nameof(Patch_ManaGain_Entropy)} postfix error: {ex}");
            }
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(Pawn_PsychicEntropyTracker), nameof(Pawn_PsychicEntropyTracker.NeedToShowGizmo))]
        public static void Postfix_NeedToShowGizmo(ref bool __result, Pawn_PsychicEntropyTracker __instance)
        {
            try
            {
                if (__result) return;
                //拥有魔法时强制显示灵能ui
                if ((__instance.Pawn.GetPharna()?.spells?.Count > 0))
                {
                    __result = true;
                    //初始化精神力
                    if (__instance.CurrentPsyfocus < 0f)
                        __instance.SetInitialPsyfocusLevel();
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[FamiliaMyth] {nameof(Patch_ManaGain_Entropy)}.Postfix_NeedToShowGizmo error: {ex}");
            }
        }

        [HarmonyPatch(typeof(Pawn_PsychicEntropyTracker), "NeedsPsyfocus", MethodType.Getter)]
        public static class Patch_ManaGain_NeedsPsyfocus
        {
            public static void Postfix(ref bool __result, Pawn_PsychicEntropyTracker __instance)
            {
                try
                {
                    if (__result) return;

                    //排除原版无法进行冥想的情况
                    if (__instance.Pawn.Suspended) return;
                    if (!__instance.Pawn.Spawned && !__instance.Pawn.IsCaravanMember()) return;

                    //检查是否有魔法
                    var pharna = __instance.Pawn.GetPharna();
                    if (pharna?.spells?.Count > 0)
                        __result = true;
                }
                catch (Exception ex)
                {
                    Log.Error($"[FamiliaMyth] {nameof(Patch_ManaGain_NeedsPsyfocus)} error: {ex}");
                }
            }
        }
    }

    /// <summary>
    /// 有魔法但无原版灵能（Psylink）的眷族，在判定层被视为拥有灵能。
    /// 仅覆盖返回值，<see cref="Pawn_PsychicEntropyTracker.Psylink"/> 本身仍为 null；
    /// 原版对 HasPsylink 的全部消费者均只作布尔判断，无解引用。
    /// </summary>
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.HasPsylink), MethodType.Getter)]
    public static class Patch_ManaGain_HasPsylink
    {
        public static void Postfix(Pawn __instance, ref bool __result)
        {
            try
            {
                //真灵能者直接放行，不付 GetPharna 的遍历开销
                if (__result) return;
                if (__instance.psychicEntropy == null) return;
                if (__instance.GetPharna()?.spells?.Count > 0) //每次遍历可能性能开销大，之后考虑添加缓存
                    __result = true;
            }
            catch (Exception ex)
            {
                Log.Error($"[FamiliaMyth] {nameof(Patch_ManaGain_HasPsylink)} error: {ex}");
            }
        }
    }

    /// <summary>
    /// 让有魔法但无灵能的眷族也按段位衰减 Psyfocus，使「冥想回精神力」成为闭环。
    /// 原值为 0 当且仅当 GetPsylinkLevel() == 0（FallRatePerPsyfocusBand 三项恒为正）。
    /// </summary>
    [HarmonyPatch]
    public static class Patch_ManaGain_PsyfocusFall
    {
        //PsyfocusFallPerDay 为 private 属性，无法用 nameof 定位
        public static MethodBase TargetMethod()
        {
            return AccessTools.PropertyGetter(typeof(Pawn_PsychicEntropyTracker), "PsyfocusFallPerDay");
        }

        public static void Postfix(Pawn_PsychicEntropyTracker __instance, ref float __result)
        {
            try
            {
                if (__result > 0f) return;
                if (__instance.Pawn?.GetPharna()?.spells?.Count > 0)    //同前 :119
                    __result = Pawn_PsychicEntropyTracker.FallRatePerPsyfocusBand[__instance.PsyfocusBand];
            }
            catch (Exception ex)
            {
                Log.Error($"[FamiliaMyth] {nameof(Patch_ManaGain_PsyfocusFall)} error: {ex}");
            }
        }
    }
}
