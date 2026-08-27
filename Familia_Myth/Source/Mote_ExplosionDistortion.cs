using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class Mote_ExplosionDistortion : Mote
    {
        private static readonly MaterialPropertyBlock Mpb = new();

        private static Material distortionMat;
        private static Material DistortionMat =>
            distortionMat ??= MaterialPool.MatFrom(
                "Things/Mote/Black",
                ShaderDatabase.LoadShader("Map/MoteLargeDistortionWave"),
                MapMaterialRenderQueues.Tornado);

        private static readonly Texture2D DistortionTex =
            ContentFinder<Texture2D>.Get("Things/Mote/PsychicDistortionCurrents", true);
        private static readonly Texture2D NoiseTex =
            ContentFinder<Texture2D>.Get("Things/Mote/PsycastNoise", true);

        // 爆炸特效色调：红色 → 紫色（随收缩进程渐变）
        private static readonly Color RedTone = new(1f, 0.35f, 0.2f);
        private static readonly Color PurpleTone = new(0.62f, 0.2f, 0.9f);

        private float startSize;   // 起始边长（= 爆炸直径）
        private float duration;    // 收缩时长（秒）

        public void Init(float radius, int delayTicks)
        {
            startSize = radius * 2;
            duration = delayTicks / 60f;
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            //由外向内收缩
            float t = Mathf.Clamp01(AgeSecs / duration);
            float size = Mathf.Lerp(startSize, 0f, t);
            float fade = 1f - t;

            Mpb.SetTexture("_DistortionTex", DistortionTex);
            Mpb.SetTexture("_NoiseTex", NoiseTex);
            Mpb.SetFloat("_distortionIntensity", 0.4f * fade);
            Mpb.SetFloat("_brightnessMultiplier", 1.5f);
            //收缩过程中由红渐变到紫
            Mpb.SetColor(ShaderPropertyIDs.Color, Color.Lerp(RedTone, PurpleTone, t) * fade);

            var pos = new Vector3(drawLoc.x, def.altitudeLayer.AltitudeFor() + yOffset, drawLoc.z);
            Graphics.DrawMesh(MeshPool.plane10,
                Matrix4x4.TRS(pos, Quaternion.identity, new Vector3(size, 1f, size)),
                DistortionMat, 0, null, 0, Mpb);
        }
    }
}
