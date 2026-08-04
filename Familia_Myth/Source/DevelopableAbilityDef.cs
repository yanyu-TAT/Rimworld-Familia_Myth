using FamiliaMyth.Pharna;
using RimWorld;
using Verse;

namespace FamiliaMyth.Abilities
{
    /// <summary>
    /// 发展能力Def
    /// </summary>
    public class DevelopableAbilityDef : Def
    {
        public Type abilityClass;
        public float maxProficiency = 999f; //熟练度上限
        public RewardRarity rarity = RewardRarity.Common; //稀有度
        public List<StatModifier> modifiers = new();
        public string traitDefName; //对应的特性的DefName

        public DevelopableAbility CreateAbility()
        {
            var abi = (DevelopableAbility) Activator.CreateInstance(abilityClass);
            abi.def = this;
            return abi;
        }
    }
}