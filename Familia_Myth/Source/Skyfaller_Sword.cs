using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class Skyfaller_Sword : Skyfaller
    {
        private static FleckDef fleckDef;
        private static FleckDef FleckDeff =>
            fleckDef ??= DefDatabase<FleckDef>.GetNamed("SwordRainImpact");

        public DamageInfo dinfo;
        public Pawn target;
        public Skill_SwordRain skill;

        protected override void Impact()
        {
            var map = Map;
            var pos = Position;
            skill.swordCount--;
            if (target != null && target.Spawned && !target.Dead){
                target.TakeDamage(dinfo);
                if (target.Downed || target.Dead)
                {
                    skill.kill = true;
                }
            }

            //已经是最后一把剑
            if (skill.swordCount <= 0)
            {
                //效果结束
                skill.OnCastFinish();
            }

            base.Impact();

            FleckMaker.Static(pos.ToVector3Shifted() + Vector3.up * def.Altitude,
                map,
                FleckDeff,
                0.8f);
        }
    }
}
