using FamiliaMyth.Pharna;
using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class Spell_FusilladeFlameSpears : Spell
    {
        //扇形角度
        private const int FanAngle = 120;
        //半径/半弧长+半径
        private const float ProgressRate = 1 / (Mathf.PI / 180 * FanAngle + 1);
        //扇形弧线分段数
        private const int ArcLineCount = 60;
        //指示线宽度
        private const float IndicatorLineWIdth = 0.15f;

        private static FleckDef lineIndicatorDef;
        private static FleckDef LineIndicatorDef =>
            lineIndicatorDef ??= DefDatabase<FleckDef>.GetNamed("LineIndicator");

        private int Radius => (int)verb.EffectiveRange;

        public override void Apply(Hediff_Pharna pharna)
        {
            base.Apply(pharna);
            if (verb is Verb_CastAbility_FanTarget fanTarget)
                fanTarget.fanAngle = 120;
            else
                Log.Warning("[FamiliaMyth]Verb For Spell_FusilladeFlameSpears should have been of type Verb_CastAbility_FanTarget but not");
        }

        protected override void TickWarmUp()
        {
            base.TickWarmUp();
            //每2帧重绘
            if (Find.TickManager.TicksGame % 2 == 0)
                DrawWithProgress(verb.WarmupProgress, 0.3f);
        }

        protected override void WarmUpFinish()
        {
            base.WarmUpFinish();
            //咏唱结束时重绘完整指示线，传入更长的留存时间
            DrawWithProgress(1f, 30);
        }

        /// <summary>
        /// 绘制扇形指示线
        /// </summary>
        /// <param name="progress">咏唱进度</param>
        /// <param name="particleSolidTime">粒子留存时间</param>
        private void DrawWithProgress(float progress, float particleSolidTime)
        {
            if (pawn?.Map is not Map map)
                return;

            float facing = verb.CurrentTarget.IsValid ? (verb.CurrentTarget.Cell - pawn.Position).AngleFlat : pawn.Rotation.AsAngle;
            float startAngle = facing - FanAngle / 2f;
            float endAngle = facing + FanAngle / 2f;
            var center = pawn.DrawPos + Vector3.up * 0.2f;
            var curRadius = Radius * Mathf.Clamp01(progress / ProgressRate);
            var curArcAngle = FanAngle * Mathf.Clamp01((progress - ProgressRate) / (1f - ProgressRate));

            //半径
            var A = center + AngleToVec(startAngle) * curRadius;
            var B = center + AngleToVec(endAngle) * curRadius;
            ConnectingLine(center, A, particleSolidTime, map, IndicatorLineWIdth);
            ConnectingLine(center, B, particleSolidTime, map, IndicatorLineWIdth);

            if (curArcAngle > 0)
                //两端到弧中点弧线
                for (int i = 0; i < (int)(curArcAngle / FanAngle * ArcLineCount); i++)
                {
                    var t1 = (float)i / ArcLineCount;
                    var t2 = (float)(i + 1) / ArcLineCount;
                    var a1 = center + AngleToVec(Mathf.Lerp(startAngle, endAngle, t1)) * Radius;
                    var b1 = center + AngleToVec(Mathf.Lerp(startAngle, endAngle, t2)) * Radius;
                    var a2 = center + AngleToVec(Mathf.Lerp(endAngle, startAngle, t2)) * Radius;
                    var b2 = center + AngleToVec(Mathf.Lerp(endAngle, startAngle, t1)) * Radius;
                    ConnectingLine(a2, b2, particleSolidTime, map, IndicatorLineWIdth);
                    ConnectingLine(a1, b1, particleSolidTime, map, IndicatorLineWIdth);
                }
        }

        public static void ConnectingLine(Vector3 start, Vector3 end, float particleSolidTime, Map map, float width = 1f)
        {
            Vector3 vector = end - start;
            float x = vector.MagnitudeHorizontal();
            FleckCreationData dataStatic = FleckMaker.GetDataStatic(start + vector * 0.5f, map, LineIndicatorDef);
            dataStatic.solidTimeOverride = particleSolidTime;
            dataStatic.exactScale = new Vector3(x, 1f, width);
            dataStatic.rotation = Mathf.Atan2(0f - vector.z, vector.x) * 57.29578f;
            dataStatic.instanceColor = new(1, 0.15f, 0.05f, 0.9f);
            map.flecks.CreateFleck(dataStatic);
        }

        private static Vector3 AngleToVec(float angleDeg)
        {
            return Quaternion.AngleAxis(angleDeg, Vector3.up) * Vector3.forward;
        }
    }
}
