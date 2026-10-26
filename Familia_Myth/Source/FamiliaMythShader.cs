using System;
using UnityEngine;
using Verse;

namespace FamiliaMyth
{
    /// <summary>
    /// 自定义 shader 的解析入口：先从定义该 ShaderTypeDef 的 mod 自己的 AssetBundle 里按文件名查找，
    /// 命中即用；未命中再回落到原版 <see cref="ShaderTypeDef.Shader"/> 的路径查找。
    /// </summary>
    public static class FamiliaMythShader
    {
        private const string ShaderExtension = ".shader";

        public static Shader Resolve(string defName)
        {
            ShaderTypeDef def = DefDatabase<ShaderTypeDef>.GetNamed(defName, false);
            if (def == null)
            {
                Log.Error($"[FamiliaMyth] ShaderTypeDef not found: {defName}");
                return null;
            }
            return Resolve(def);
        }

        public static Shader Resolve(ShaderTypeDef def)
        {
            if (def == null) return null;
            return FindInOwnBundles(def) ?? def.Shader;
        }

        // 原版按 Assets/Data/<mod 文件夹名>/ 或 <packageId>/ 查找资源；工坊订阅后文件夹名是数字 id，
        // 这两种路径都可能失配，因此直接按 shaderPath 的文件名在自身 bundle 内枚举。
        // 供 Harmony 补丁在 ShaderTypeDef.Shader 解析失败时兜底
        public static Shader FindInOwnBundles(ShaderTypeDef def)
        {
            string fileName = def.shaderPath?.ToLowerInvariant();
            if (fileName.NullOrEmpty()) return null;

            ModContentPack pack = def.modContentPack;
            if (pack?.assetBundles == null) return null;

            List<AssetBundle> bundles = pack.assetBundles.loadedAssetBundles;
            if (bundles.NullOrEmpty()) return null;

            string suffix = "/" + fileName + ShaderExtension;
            try
            {
                for (int i = 0; i < bundles.Count; i++)
                {
                    AssetBundle bundle = bundles[i];
                    if (bundle == null) continue;

                    string[] assetNames = bundle.GetAllAssetNames();
                    for (int j = 0; j < assetNames.Length; j++)
                    {
                        string assetName = assetNames[j];
                        if (assetName == null || !assetName.ToLowerInvariant().EndsWith(suffix)) continue;
                        if (bundle.LoadAsset(assetName, typeof(Shader)) is Shader shader) return shader;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Warning($"[FamiliaMyth] Failed to scan own AssetBundle for shader '{def.shaderPath}': {e}");
            }
            return null;
        }
    }
}
