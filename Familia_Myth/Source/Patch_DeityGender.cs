using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace FamiliaMyth.Patch
{
    /// <summary>
    /// 为眷族信仰的固定神灵绑定性别：按 MemeDef.fixedDeityNameTypes 的顺序取性别，
    /// 因此改名字或切换语言都不影响匹配（玩家手改过名字的神灵会被跳过）。
    /// </summary>
    [HarmonyPatch(typeof(IdeoFoundation_Deity), nameof(IdeoFoundation_Deity.GenerateDeities))]
    public static class Patch_DeityGender
    {
        private const string MemeDefName = "Structure_FamiliaMyth";

        /// <summary>
        /// 与 Structure_FamiliaMyth 的 fixedDeityNameTypes 逐条对应；增删条目时须同步本表，缺项则跳过该神灵。
        /// </summary>
        private static readonly Gender[] GendersByIndex = { 
            //赫斯提亚
            Gender.Female, 
            //阿斯特莉亚
            Gender.Female, 
            //洛基
            Gender.Female,
            //赫菲斯托丝
            Gender.Female,
            //芙蕾雅
            Gender.Female,
            //赫尔墨斯
            Gender.Male,
            //阿尔特弥斯
            Gender.Female
        };

        public static void Postfix(IdeoFoundation_Deity __instance)
        {
            try
            {
                Ideo ideo = __instance?.ideo;
                if (ideo == null) return;

                MemeDef meme = DefDatabase<MemeDef>.GetNamed(MemeDefName, false);
                if (meme == null || !ideo.HasMeme(meme)) return;

                var nameTypes = meme.fixedDeityNameTypes;
                if (nameTypes.NullOrEmpty()) return;

                var deities = __instance.DeitiesListForReading;
                for (int i = 0; i < deities.Count; i++)
                {
                    int index = nameTypes.FindIndex(t => t.name == deities[i].name);
                    if (index < 0 || index >= GendersByIndex.Length) continue;
                    deities[i].gender = GendersByIndex[index];
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[FamiliaMyth] {nameof(Patch_DeityGender)} error: {ex}");
            }
        }
    }
}
