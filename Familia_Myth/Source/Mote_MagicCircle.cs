using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class Mote_MagicCircle : MoteAttached
    {
        private Graphic_Mote graphic;

        public void Init(MagicCircleLayer layer)
        {
            instanceColor = layer.drawColor;
            rotationRate = layer.rotate;
            yOffset = layer.altitude;
            linearScale = new(layer.scaleFrom, 1f, layer.scaleFrom);
            exactRotation = layer.initialRotation;

            var gd = new GraphicData
            {
                graphicClass = typeof(Graphic_Mote),
                texPath = layer.texPath,
                shaderType = layer.shaderType ?? DefDatabase<ShaderTypeDef>.GetNamed("MoteGlow"),
                drawSize = layer.size,
                color = Color.white
            };
            graphic = (Graphic_Mote)gd.Graphic;
        }

        public override Graphic Graphic => graphic;

        protected override void TimeInterval(float deltaTime)
        {
            base.TimeInterval(deltaTime);

            float t = Mathf.Clamp01(AgeSecs / def.mote.fadeInTime);
            float s = Mathf.Lerp(linearScale.x, 1f, t);
            linearScale = new(s, 1f, s);

            exactRotation += rotationRate * deltaTime;
        }
    }
}
