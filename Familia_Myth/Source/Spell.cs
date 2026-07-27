using FamiliaMyth.Pharna;
using RimWorld;
using System.Runtime.InteropServices;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class Spell : Ability, IExposable, IAscendReward
    {
        public SpellDef spellDef;

        public RewardType RewardType => RewardType.Spell;

        protected int LineCount => spellDef?.incantationLines.Count ?? 1;
        protected int IntervalTicks => 60;
        protected int CurrentTick => Find.TickManager.TicksGame;

        public RewardRarity Rarity => spellDef.rarity;

        public string Label => spellDef.label;

        public string Description => spellDef.description;

        protected int lastLineTick = -9999;

        public void RewardInitialize()
        {
            spellDef = DefDatabase<SpellDef>.GetNamed(GetType().Name, false);
            def = spellDef; 
        }

        public override void AbilityTick()
        {
            base.AbilityTick();
            if (spellDef == null || spellDef.incantationLines.NullOrEmpty())
                return;

            if (verb == null || !verb.WarmingUp || verb.CasterPawn == null)
                return;

            // 根据 Verb.WarmupProgress 分配咏唱行
            float warmupProgress = verb.WarmupProgress;
            int lineIndex = Mathf.FloorToInt(warmupProgress * LineCount);
            lineIndex = Mathf.Clamp(lineIndex, 0, LineCount - 1);
            if (CurrentTick - lastLineTick >= IntervalTicks)
            {
                Vector3 loc = pawn.DrawPos + new Vector3(0f, 0f, 1.5f);
                MoteMaker.ThrowText(loc, pawn.Map, spellDef.incantationLines[lineIndex], Color.blue);
                lastLineTick = CurrentTick;
            }
        }

        public void Apply(Hediff_Pharna pharna)
        {
            pawn = pharna.pawn;
            Initialize();
            pharna.spells.Add(this);
        }

        public virtual float GetCurrentSpellPower(Pawn caster)
        {
            if (caster == null) return 1f;

            float power = 1f;
            var pharna = Hediff_Pharna.GetPharna(caster);
            if (pharna != null)
            {
                power += pharna.GetFullStatus(PharnaStatusType.Mana) * 0.001f;

                if (pharna.skills != null)
                {
                    foreach (var skill in pharna.skills)
                    {
                        if (skill is ISpellPowerModifier modifier)
                        {
                            modifier.ApplySpellPowerModifier(ref power);
                        }
                    }
                }

                if (pharna.developableAbilities != null)
                {
                    foreach (var ability in pharna.developableAbilities)
                    {
                        if (ability is ISpellPowerModifier modifier)
                        {
                            modifier.ApplySpellPowerModifier(ref power);
                        }
                    }
                }

                if (caster?.health?.hediffSet != null)
                {
                    foreach (var hediff in caster.health.hediffSet.hediffs)
                    {
                        if (hediff is ISpellPowerModifier modifier)
                        {
                            modifier.ApplySpellPowerModifier(ref power);
                        }
                    }
                }
            }

            return Math.Max(power, 0f);
        }
    }

    public interface ISpellPowerModifier
    {
        void ApplySpellPowerModifier(ref float power);
    }
}