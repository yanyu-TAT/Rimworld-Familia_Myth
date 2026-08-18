using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    [StaticConstructorOnStartup]
    public class Command_Skill : Command_Action
    {
        private readonly Skill skill;
        private static readonly Texture2D CooldownTex =
            SolidColorMaterials.NewSolidColorTexture(new Color(0.3f, 0.7f, 0.3f, 0.55f));

        public Command_Skill(Skill skill)
        {
            this.skill = skill;
            defaultLabel = skill.Label;
            defaultDesc = skill.Description;
            if (!skill.def.iconPath.NullOrEmpty())
                icon = ContentFinder<Texture2D>.Get(skill.def.iconPath, false);
        }

        public override bool Disabled
        {
            get
            {
                var report = skill.CanCast;
                disabled = !report;
                disabledReason = report.Reason;
                return disabled;
            }
        }

        public override void GizmoUpdateOnMouseover()
        {
            base.GizmoUpdateOnMouseover();
            skill.OnHovering();
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            var result =  base.GizmoOnGUI(topLeft, maxWidth, parms);

            float f = Mathf.Round(skill.CooldownProgress * 64) / 64;
            if (f > 0f)
            {
                Rect rect = new(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);

                //冷却时淡绿色覆盖层随进度从上至下
                float coverH = rect.height * f;
                GUI.DrawTexture(
                    new Rect(rect.x, rect.y + (rect.height - coverH),
                             rect.width, coverH),
                    CooldownTex);
            }

            return result;
        }
    }
}
