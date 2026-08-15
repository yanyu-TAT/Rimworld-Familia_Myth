using FamiliaMyth.Pharna;
using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    [StaticConstructorOnStartup]
    public class Spell : Ability, IAscendReward
    {
        private static readonly Color LineColor = new(0.3f, 0.8f, 0.9f);
        private static readonly Texture2D SpellKeyTexture = ContentFinder<Texture2D>.Get("Familia_Myth/UI/Commands/SpellKey", true);

        private static ThingDef magicCircleDef;
        private static ThingDef MagicCircleDef =>
            magicCircleDef ??= DefDatabase<ThingDef>.GetNamed("Mote_MagicCircleLayer");

        private static StatDef spellPowerDef;
        private static StatDef SpellPowerDef =>
            spellPowerDef ??= DefDatabase<StatDef>.GetNamed("SpellPower");

        public SpellDef spellDef;

        public RewardType RewardType => RewardType.Spell;

        protected int LineCount => spellDef?.incantationLines.Count ?? 1;
        protected int IntervalTicks => (int)spellDef.lineInterval * 60;
        protected static int CurrentTick => Find.TickManager.TicksGame;

        public RewardRarity Rarity => spellDef.rarity;

        public string Label => spellDef.label;

        public string Description => spellDef.description;

        public List<Projectile_MagicMissileBase> activeProjectiles = new();

        public List<Mote_MagicCircle> activeMotes = new();

        protected int lastLineTick = -9999;
        protected bool wasWarmingUp = false;
        protected int lineIndex = 0; 

        public virtual void RewardInitialize()
        {
            spellDef = DefDatabase<SpellDef>.GetNamed(GetType().Name, false);
            def = spellDef;

            //尝试随机生成魔法阵样式
            if (spellDef.magicCircles.NullOrEmpty())
                spellDef.magicCircles = spellDef.DefaultMagicCircles;
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
                MagicCircleStart();
            }
            if (wasWarmingUp && !verb.WarmingUp)
            {
                //结尾
                wasWarmingUp = false;
                ThrowIncantation(lineIndex++);
                WarmUpFinish();
                MagicCircleEnd();
            }

            if (wasWarmingUp)
            {
                //咏唱进行中
                if (CurrentTick - lastLineTick >= IntervalTicks)
                    ThrowIncantation(lineIndex++);
                TickWarmUp();
                TickMagicCircle();
            }
        }

        private void MagicCircleEnd()
        {
            activeMotes.Clear();
        }

        private void MagicCircleStart()
        {
            if (spellDef.magicCircles.NullOrEmpty()) return;
            activeMotes.Clear();
            TickMagicCircle();
        }

        private void TickMagicCircle()
        {
            if (spellDef.magicCircles.NullOrEmpty() || pawn?.Map == null) return;

            //生成
            var layers = spellDef.magicCircles;
            float progress = verb.WarmupProgress;
            for (int i = 0; i < layers.Count; i++)
            {
                if (i < activeMotes.Count && activeMotes[i]?.Spawned == true) continue;
                if (progress < layers[i].appearProgress) continue;
                var mote = (Mote_MagicCircle)ThingMaker.MakeThing(MagicCircleDef);
                mote.Init(layers[i]);
                mote.Attach(pawn);
                GenSpawn.Spawn(mote, pawn.Position, pawn.Map);

                if (i < activeMotes.Count) 
                    activeMotes[i] = mote;
                else 
                    activeMotes.Add(mote);
            }

            //维持
            foreach (var mote in activeMotes) mote?.Maintain();
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
            if (pawn == null) return 1f;
            return pawn.GetStatValue(SpellPowerDef);
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