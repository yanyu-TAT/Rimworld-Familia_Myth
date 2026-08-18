using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class Mote_Clock : Mote
    {
        public IntVec3 center;
        public float radius;
        public float ratio;

        private static readonly Color LineColor = new(0.6f, 0.65f, 0.78f);
        private static readonly Color GlowColor = new(0.6f, 0.65f, 0.78f, 0.25f);
        private static Material lineMat;
        private static Material LineMat =>
                lineMat ??= MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.Transparent, LineColor);
        private static Material glowMat;
        private static Material GlowMat =>
                glowMat ??= MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.Transparent, GlowColor);

        private Graphic_Mote clockGraphic;
        public override Graphic Graphic => clockGraphic;

        private Mote_ClockHand moteHourHand;
        private Mote_ClockHand moteMinuteHand;

        private static ThingDef clockHandDef;
        private static ThingDef ClockHandDef =>
            clockHandDef ??= DefDatabase<ThingDef>.GetNamed("Mote_ClockHand");

        public void Init(IntVec3 center, float radius)
        {
            this.center = center;
            this.radius = radius;

            float size = radius * 2f;
            var gd = new GraphicData
            {
                graphicClass = typeof(Graphic_Mote),
                texPath = "Familia_Myth/Things/Mote/ClockNumbers",
                shaderType = DefDatabase<ShaderTypeDef>.GetNamed("MoteGlow"),
                drawSize = new Vector2(size, size),
                color = GlowColor
            };
            clockGraphic = (Graphic_Mote)gd.Graphic;

            moteHourHand = MoteMaker.MakeStaticMote(
                center.ToVector3ShiftedWithAltitude(AltitudeLayer.MoteLow.AltitudeFor() + 0.01f), 
                Map, ClockHandDef, 0.75f, true, 0f) as Mote_ClockHand;

            moteMinuteHand = MoteMaker.MakeStaticMote(
                center.ToVector3ShiftedWithAltitude(AltitudeLayer.MoteLow.AltitudeFor() + 0.02f), 
                Map, ClockHandDef, 1f, true, 0f) as Mote_ClockHand;

            moteHourHand?.Maintain();
            moteMinuteHand?.Maintain();
        }

        protected override void TimeInterval(float deltaTime)
        {
            base.TimeInterval(deltaTime);
            if (moteHourHand != null)
            {
                float h = Mathf.Floor((1.001f - ratio) * 12f) * 30f;
                moteHourHand.angle = h;
                moteHourHand.Maintain();
            }
            if (moteMinuteHand != null)
            {
                float m = (1f - ratio) * 360f * 12f;
                moteMinuteHand.angle = m;
                moteMinuteHand.Maintain();
            }
        }

        protected override bool EndOfLife => false;

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);
            DrawArcs(GlowMat, 3f);
            DrawArcs(LineMat, 1f);
        }

        private void DrawArcs(Material mat, float width)
        {
            var cent = center.ToVector3ShiftedWithAltitude(AltitudeLayer.MoteLow);
            float angle = ratio * 360f;
            if (angle <= 0) return;
            int segment = Mathf.RoundToInt(angle / 3f);

            Vector3? prev = null;
            for (int i = 0; i < segment; i++)
            {
                float rad = ((float)i / segment * angle + 90f) * Mathf.Deg2Rad;
                var pos = cent + new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * radius;
                if (prev.HasValue)
                    GenDraw.DrawLineBetween(prev.Value, pos, mat, width);
                prev = pos;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref center, "center");
            Scribe_Values.Look(ref radius, "radius");
            Scribe_Values.Look(ref ratio, "ratio");
            Scribe_References.Look(ref moteHourHand, "moteHourHand");
            Scribe_References.Look(ref moteMinuteHand, "moteMinuteHand");

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                Maintain();
                moteHourHand?.Maintain();
                moteMinuteHand?.Maintain();
            }
        }
    }
}
