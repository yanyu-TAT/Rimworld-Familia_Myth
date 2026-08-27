using FamiliaMyth.Pharna;
using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class SpellDef : AbilityDef
    {
        public Type spellClass;

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

        public List<MagicCircleLayer> DefaultMagicCircles
        {
            get
            {
                var result = new List<MagicCircleLayer>();
                int cnt = (int)rarity + 1;
                for (int i = 0; i < cnt; i++)
                {
                    var layer = new MagicCircleLayer
                    {
                        texPath = $"Familia_Myth/Things/Mote/MagicCircles/{Rand.Range(1, 4)}",
                        drawColor = themeColor,
                        scaleFrom = 0.5f,
                        size = new((i + 1) * 5, (i + 1) * 5),
                        altitude = i * 0.01f,
                        rotate = Rand.Range(-20f, 20f),
                        appearProgress = i * 0.25f,
                        initialRotation = Rand.Range(0f, 360f)
                    };
                    result.Add(layer);
                }
                return result;
            }
        }

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

    public class MagicCircleLayer
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
    }
}