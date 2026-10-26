using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Patch
{
    /// <summary>
    /// 自定义 ShaderTypeDef 的兜底：原版按 Assets/Data/&lt;mod 文件夹名&gt;/ 或 &lt;packageId&gt;/ 查找资源，
    /// 工坊订阅后文件夹名是数字 id，路径会失配。这里在缓存为空时改为按文件名扫描自身 AssetBundle，
    /// 命中即写回缓存并跳过原方法；未命中则放行原逻辑。
    /// </summary>
    [HarmonyPatch(typeof(ShaderTypeDef), nameof(ShaderTypeDef.Shader), MethodType.Getter)]
    public static class Patch_ShaderTypeDef
    {
        private static readonly FieldInfo ShaderIntField = AccessTools.Field(typeof(ShaderTypeDef), "shaderInt");

        private static bool Prefix(ShaderTypeDef __instance, ref Shader __result)
        {
            try
            {
                if (ShaderIntField?.GetValue(__instance) is Shader)
                    return true;

                Shader shader = FamiliaMythShader.FindInOwnBundles(__instance);
                if (shader == null) return true;

                ShaderIntField.SetValue(__instance, shader);
                __result = shader;
                return false;
            }
            catch (Exception e)
            {
                Log.Error($"[FamiliaMyth] Failed to resolve shader for ShaderTypeDef '{__instance?.defName}': {e}");
                return true;
            }
        }
    }
}
