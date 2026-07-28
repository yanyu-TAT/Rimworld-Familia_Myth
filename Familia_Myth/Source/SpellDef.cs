using FamiliaMyth.Pharna;
using RimWorld;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class SpellDef : AbilityDef
    {
        public Type spellClass;

        public RewardRarity rarity;
        public SpellType spellType = SpellType.Special;

        public List<string> incantationLines;
        public float lineInterval = 1f;
        public float Warmup => (incantationLines.Count - 1) * lineInterval;

        public Spell CreateSpell()
        {
            var spell = (Spell)Activator.CreateInstance(spellClass);
            spell.def = this;
            return spell;
        }
    }

    public enum SpellType
    {
        Attack,     //攻击魔法
        Heal,       //治疗魔法
        Defense,    //防御魔法
        Buff,       //增益魔法
        Debuff,     //减益魔法
        Enchant,    //赋予魔法
        Special     //其他特殊魔法（分身，速攻等）
    }
}