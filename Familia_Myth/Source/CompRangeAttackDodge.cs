using RimWorld;
using Verse;

namespace FamiliaMyth.Pharna
{
    public class CompRangeAttackDodge : ThingComp
    {
        public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            absorbed = false;

            //仅对远程非爆炸伤害生效
            if (!dinfo.Def.isRanged || dinfo.Def.isExplosive)
                return;

            //获取远程闪避率
            float dodgeChance = parent.GetStatValue(StatDef.Named("Familia_RangedDodgeChance"));

            if (dodgeChance > 0f && Rand.Chance(dodgeChance))
            {
                // 显示闪避文字
                MoteMaker.ThrowText(
                    parent.DrawPos,
                    parent.Map,
                    "FamiliaMyth.TextMote_Dodge".Translate(),
                    1.9f);
                absorbed = true;
            }
        }
    }

    public class CompProperties_RangeAttackDodge : CompProperties
    {
        public CompProperties_RangeAttackDodge()
        {
            compClass = typeof(CompRangeAttackDodge);
        }
    }
}
