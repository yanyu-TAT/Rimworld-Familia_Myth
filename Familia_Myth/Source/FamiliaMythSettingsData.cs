using FamiliaMyth.Pharna;
using Verse;

namespace FamiliaMyth.Core
{
    /// <summary>
    /// 模组设置数据，由原版 Mod.GetSettings 读写到 Config 目录
    /// </summary>
    public class FamiliaMythSettingsData : ModSettings
    {
        public bool guildContent;

        private SimpleCurve curveStrength;
        private SimpleCurve curveEndurance;
        private SimpleCurve curveDexterity;
        private SimpleCurve curveAgility;
        private SimpleCurve curveMana;
        private SimpleCurve curveWill;

        /// <summary>
        /// 六维之一当前生效的加成曲线；配置缺失时写入内置默认曲线的副本
        /// </summary>
        public SimpleCurve GetCurve(PharnaStatusType statusType)
        {
            SimpleCurve curve = Pick(statusType);
            if (curve is null)
            {
                curve = SetCurve(statusType, Clone(PharnaCurve.BuiltinDefault(statusType)));
            }
            return curve;
        }

        /// <summary>
        /// 把六维之一恢复为内置默认曲线
        /// </summary>
        public void ResetCurve(PharnaStatusType statusType)
        {
            SetCurve(statusType, Clone(PharnaCurve.BuiltinDefault(statusType)));
        }

        /// <summary>
        /// 把六维曲线全部恢复为内置默认值
        /// </summary>
        public void ResetAllCurves()
        {
            ResetCurve(PharnaStatusType.Strength);
            ResetCurve(PharnaStatusType.Endurance);
            ResetCurve(PharnaStatusType.Dexterity);
            ResetCurve(PharnaStatusType.Agility);
            ResetCurve(PharnaStatusType.Mana);
            ResetCurve(PharnaStatusType.Will);
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref guildContent, "guildContent", false);
            curveStrength = ScribeCurve(curveStrength, "curveStrength", PharnaStatusType.Strength);
            curveEndurance = ScribeCurve(curveEndurance, "curveEndurance", PharnaStatusType.Endurance);
            curveDexterity = ScribeCurve(curveDexterity, "curveDexterity", PharnaStatusType.Dexterity);
            curveAgility = ScribeCurve(curveAgility, "curveAgility", PharnaStatusType.Agility);
            curveMana = ScribeCurve(curveMana, "curveMana", PharnaStatusType.Mana);
            curveWill = ScribeCurve(curveWill, "curveWill", PharnaStatusType.Will);
        }

        //读写单条曲线：节点缺失时以默认曲线副本代替，避免字段留空
        private static SimpleCurve ScribeCurve(SimpleCurve curve, string label, PharnaStatusType statusType)
        {
            if (curve is null)
            {
                curve = Clone(PharnaCurve.BuiltinDefault(statusType));
            }
            Scribe_Deep.Look(ref curve, label);
            if (curve is null)
            {
                curve = Clone(PharnaCurve.BuiltinDefault(statusType));
            }
            return curve;
        }

        private static SimpleCurve Clone(SimpleCurve source) => new SimpleCurve(source.Points);

        private SimpleCurve Pick(PharnaStatusType statusType) => statusType switch
        {
            PharnaStatusType.Strength => curveStrength,
            PharnaStatusType.Endurance => curveEndurance,
            PharnaStatusType.Dexterity => curveDexterity,
            PharnaStatusType.Agility => curveAgility,
            PharnaStatusType.Mana => curveMana,
            PharnaStatusType.Will => curveWill,
            _ => null
        };

        private SimpleCurve SetCurve(PharnaStatusType statusType, SimpleCurve curve)
        {
            switch (statusType)
            {
                case PharnaStatusType.Strength: curveStrength = curve; break;
                case PharnaStatusType.Endurance: curveEndurance = curve; break;
                case PharnaStatusType.Dexterity: curveDexterity = curve; break;
                case PharnaStatusType.Agility: curveAgility = curve; break;
                case PharnaStatusType.Mana: curveMana = curve; break;
                case PharnaStatusType.Will: curveWill = curve; break;
            }
            return curve;
        }
    }
}
