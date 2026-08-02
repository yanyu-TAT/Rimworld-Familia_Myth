using FamiliaMyth.Pharna;
using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    [StaticConstructorOnStartup]
    public class Spell : Ability, IExposable, IAscendReward
    {
        private static readonly Color LineColor = new(0.3f, 0.8f, 0.9f);
        private static readonly Texture2D SpellKeyTexture = ContentFinder<Texture2D>.Get("Familia_Myth/UI/Commands/SpellKey", true);

        public SpellDef spellDef;

        public RewardType RewardType => RewardType.Spell;

        protected int LineCount => spellDef?.incantationLines.Count ?? 1;
        protected int IntervalTicks => (int)spellDef.lineInterval * 60;
        protected int CurrentTick => Find.TickManager.TicksGame;

        public RewardRarity Rarity => spellDef.rarity;

        public string Label => spellDef.label;

        public string Description => spellDef.description;

        public List<Projectile_MagicMissileBase> activeProjectiles = new();

        protected int lastLineTick = -9999;
        protected bool wasWarmingUp = false;
        protected int lineIndex = 0; 

        public virtual void RewardInitialize()
        {
            spellDef = DefDatabase<SpellDef>.GetNamed(GetType().Name, false);
            def = spellDef; 
        }

        public override void AbilityTick()
        {
            base.AbilityTick();
            if (spellDef == null || spellDef.incantationLines.NullOrEmpty())
                return;

            if (verb == null || verb.CasterPawn == null)
                return;

            if (!wasWarmingUp && verb.WarmingUp)
            {
                //开始咏唱
                lineIndex = 0;
                wasWarmingUp = true;
                WarmUpStart();
            }
            if (wasWarmingUp && !verb.WarmingUp)
            {
                //结尾
                wasWarmingUp = false;
                ThrowIncantation(lineIndex++);
                WarmUpFinish();
            }

            if (wasWarmingUp)
            {
                if (CurrentTick - lastLineTick >= IntervalTicks)
                    ThrowIncantation(lineIndex++);
                TickWarmUp();
            }
        }

        private void ThrowIncantation(int a)
        {
            if (a >= spellDef.incantationLines.Count) return;
            Vector3 loc = pawn.DrawPos + new Vector3(0f, 0f, 1.5f);
            MoteMaker.ThrowText(loc, pawn.Map, spellDef.incantationLines[a], LineColor);
            lastLineTick = CurrentTick;
        }

        protected virtual void OnSpellKeyCasted()
        {
            if (spellDef.hasSpellKey)
            {
                Vector3 loc = pawn.DrawPos + new Vector3(0f, 0f, 1.5f);
                MoteMaker.ThrowText(loc, pawn.Map, spellDef.spellKey, LineColor);
                NotifyAll("SpellKey");
            }
        }

        protected virtual void WarmUpStart() { }

        protected virtual void WarmUpFinish() { }

        protected virtual void TickWarmUp() { }

        protected void NotifyAll(string message)
        {
            var toDos = GetActionsFor(message);
            foreach (var toDo in toDos)
                toDo();
        }

        /// <summary>
        /// 获取接受对应消息后对应魔法弹的行为引用
        /// </summary>
        /// <param name="message"></param>
        /// <returns>无参无返回值的方法引用集合</returns>
        protected List<Action> GetActionsFor(string message) => activeProjectiles.Select(p => p.NotifyBy(message)).ToList();

        public virtual void Apply(Hediff_Pharna pharna)
        {
            pawn = pharna.pawn;
            Initialize();
            verb.verbProps.warmupTime = spellDef.Warmup;
            pharna.spells.Add(this);
        }

        public virtual float GetCurrentSpellPower()
        {
            var caster = pawn;
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

        public override IEnumerable<Command> GetGizmos()
        {
            foreach (var command in base.GetGizmos())
                yield return command;

            if (spellDef.hasSpellKey)
            {
                Command_Action spellKey = new()
                {
                    defaultLabel = "引爆咒：" + spellDef.label,
                    defaultDesc = spellDef.spellKeyDescription,
                    icon = SpellKeyTexture,
                    action = () =>
                    {
                        OnSpellKeyCasted();
                    }
                };
                yield return spellKey;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref lastLineTick, "lastLineTick");
            Scribe_Values.Look(ref lineIndex, "lineIndex");
            Scribe_Values.Look(ref wasWarmingUp, "wasWarmingUp");
            Scribe_Collections.Look(ref activeProjectiles, "activeProjectiles", LookMode.Reference);
        }
    }

    public interface ISpellPowerModifier
    {
        void ApplySpellPowerModifier(ref float power);
    }
}