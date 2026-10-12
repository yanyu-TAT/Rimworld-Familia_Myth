using System.Runtime.Serialization;
using RimWorld;
using Verse;

namespace FamiliaMyth.Abilities
{
    /// <summary>
    /// 魔法图标的命令。继承原版 <see cref="Command_Psycast"/>，使原版心灵熵条（PsychicEntropyGizmo）
    /// 能按 Command_Psycast 认出它，从而在悬停时预览本次施法将增加的熵。
    /// </summary>
    public class Command_Spell : Command_Psycast
    {
        public Command_Spell(Ability ability, Pawn pawn) : base(ProxyFor(ability.def, pawn), pawn)
        {
            //基类构造期间用的是影子，此处换回真实 Ability
            this.ability = ability;
        }

        /// <summary>
        /// 在原版角标（神经热量 / 精神力）之后追加本次施法的魔力消耗
        /// </summary>
        public override string TopRightLabel
        {
            get
            {
                string label = base.TopRightLabel;
                if (ability.def is not SpellDef spellDef || spellDef.manaCost <= float.Epsilon)
                    return label;

                string mana = "FamiliaMyth.Ability.ManaLetter".Translate() + ": " + spellDef.manaCost.ToString("F0");
                return label.NullOrEmpty() ? mana : label + "\n" + mana;
            }
        }

        /// <summary>
        /// 在原版描述末尾追加本次施法的魔力消耗
        /// </summary>
        public override string Tooltip
        {
            get
            {
                string tooltip = base.Tooltip;
                if (ability.def is not SpellDef spellDef || spellDef.manaCost <= float.Epsilon)
                    return tooltip;

                return tooltip + "\n" + "FamiliaMyth.Ability.ManaCost".Translate(spellDef.manaCost.ToString("F0"));
            }
        }

        /// <summary>
        /// 仅作 def 载体的 Psycast，不参与初始化，也不进入任何集合。
        /// </summary>
        private static Psycast ProxyFor(AbilityDef def, Pawn pawn)
        {
            var proxy = (Psycast)FormatterServices.GetUninitializedObject(typeof(Psycast));
            proxy.def = def;
            proxy.pawn = pawn;
            return proxy;
        }

        /// <summary>
        /// 沿用 <see cref="Command_Ability"/> 的禁用判定，不采用 Psycast 的整段熵预检。
        /// </summary>
        protected override void DisabledCheck()
        {
            disabled = ability.GizmoDisabled(out var reason);
            if (disabled)
                DisableWithReason(reason.CapitalizeFirst());
        }
    }
}
