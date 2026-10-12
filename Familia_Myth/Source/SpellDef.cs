using FamiliaMyth.Pharna;
using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class SpellDef : AbilityDef
    {
        public SpellDef()
        {
            gizmoClass = typeof(Command_Spell);
        }

        public Type spellClass;

        /// <summary>
        /// 施放消耗的魔力
        /// </summary>
        public float manaCost = 100f;

        public RewardRarity rarity;
        public SpellType spellType = SpellType.Special;

        public List<string> incantationLines;
        public bool hasSpellKey = false;
        public string spellKey = "";
        public string spellKeyDescription = "";
        public float lineInterval = 1f;

        public List<MagicCircleLayer> magicCircles;

        public Color themeColor = Color.white;

        public bool canConcurrentCast = true;

        /// <summary>
        /// 咏唱秒数
        /// </summary>
        public float WarmupTime => (incantationLines.Count - 1) * lineInterval;

        public Spell CreateSpell()
        {
            var spell = (Spell)Activator.CreateInstance(spellClass);
            spell.def = this;
            return spell;
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (var s in base.ConfigErrors())
                yield return s;

            if (rarity == RewardRarity.Unique && magicCircles.NullOrEmpty())
                yield return "Unique Spells Should Have Unique Magic Circles";
        }
    }

    public enum SpellType
    {
        None,       //无类型，仅用于魔力输出的计算
        Attack,     //攻击魔法
        Heal,       //治疗魔法
        Defense,    //防御魔法
        Buff,       //增益魔法
        Debuff,     //减益魔法
        Enchant,    //赋予魔法
        Field,      //领域魔法
        Summon,     //召唤魔法
        Special     //其他特殊魔法（分身，速攻等）
    }

    /// <summary>
    /// 描述施法者咏唱时在其脚下展开的魔法阵
    /// </summary>
    public class MagicCircleLayer : IExposable
    {
        public string texPath;
        public float appearProgress = 0f;   //出现时间占咏唱时间的百分比
        public Color drawColor = Color.white;   //额外覆盖的颜色
        public float initialRotation = 0f;  //初始角度
        public float rotate = 0f;   //转速，顺正逆负
        public float altitude = 0f; //高度
        public float scaleFrom = 1f;    //初始缩放
        public Vector2 size = new(1f, 1f); //绘制尺寸
        public ShaderTypeDef shaderType; //默认MoteGlow

        public void ExposeData()
        {
            Scribe_Values.Look(ref texPath, "texPath");
            Scribe_Values.Look(ref appearProgress, "appearProgress");
            Scribe_Values.Look(ref drawColor, "drawColor", Color.white);
            Scribe_Values.Look(ref initialRotation, "initialRotation");
            Scribe_Values.Look(ref rotate, "rotate");
            Scribe_Values.Look(ref altitude, "altitude");
            Scribe_Values.Look(ref scaleFrom, "scaleFrom", 1f);
            Scribe_Values.Look(ref size, "size", new(1f, 1f));
            Scribe_Defs.Look(ref shaderType, "shaderType");
        }
    }
}