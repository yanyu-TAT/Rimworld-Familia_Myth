using FamiliaMyth.Pharna;
using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    [StaticConstructorOnStartup]
    public class Spell : Ability, IAscendReward
    {
        private static readonly Texture2D SpellKeyTexture = ContentFinder<Texture2D>.Get("Familia_Myth/UI/Commands/SpellKey", true);

        private static ThingDef magicCircleDef;
        private static ThingDef MagicCircleDef =>
            magicCircleDef ??= DefDatabase<ThingDef>.GetNamed("Mote_MagicCircleLayer");

        private static StatDef spellPowerDef;
        private static StatDef SpellPowerDef =>
            spellPowerDef ??= DefDatabase<StatDef>.GetNamed("SpellPower");

        private static ThingDef incantationTextDef;
        private static ThingDef IncantationDef =>
            incantationTextDef ??= DefDatabase<ThingDef>.GetNamed("Mote_Incantation");

        protected SpellDef spellDef;

        public RewardType RewardType => RewardType.Spell;

        protected int LineCount => spellDef?.incantationLines.Count - 1 ?? 1;
        protected int IntervalTicks => (int)spellDef.lineInterval * 60;
        protected static int CurrentTick => Find.TickManager.TicksGame;

        public RewardRarity Rarity => spellDef.rarity;
        public string Label => spellDef.label;
        public string Description => spellDef.description;

        protected float PsyfocusCostPerTick => (def?.PsyfocusCost / (spellDef.Warmup * 60f)) ?? 0f;
        protected float EntropyGainPerTick => (def?.EntropyGain / (spellDef.Warmup * 60f)) ?? 0f;

        public List<Projectile_MagicMissileBase> activeProjectiles = new();

        public Mote_Incantation activeLine;

        public List<Mote_MagicCircle> activeMotes = new();

        protected int lastLineTick = -9999;
        protected bool wasWarmingUp = false;
        protected int lineIndex = 0;

        protected float actualPsy;
        protected float actualEnt;

        public bool stunning = false;

        public virtual void RewardInitialize()
        {
            spellDef = DefDatabase<SpellDef>.GetNamed(GetType().Name, false);
            def = spellDef;

            //尝试随机生成魔法阵样式
            if (spellDef.magicCircles.NullOrEmpty())
                spellDef.magicCircles = spellDef.DefaultMagicCircles;
        }

        public override AcceptanceReport CanCast
        {
            get
            {
                AcceptanceReport canCast = base.CanCast;
                if (!canCast) return canCast;

                var check = pawn?.GetPharna()?.CheckStunning ?? true;
                if (!check)
                    return $"{check.Reason}效果结束前不可重复释放";

                var tracker = pawn?.psychicEntropy;
                if (tracker == null) return true;

                float gain = def.EntropyGain - tracker.RecoveryRate * spellDef.Warmup;
                if (tracker.WouldOverflowEntropy(gain))
                    return $"{pawn.Name.ToStringShort}的心灵熵将会超出阈值";

                float cost = def.PsyfocusCost;
                if (cost > float.Epsilon && cost > tracker.CurrentPsyfocus + 0.0005f)
                    return $"{pawn.Name.ToStringShort}的精神力不足";

                return true;
            }
        }

        public override void AbilityTick()
        {
            base.AbilityTick();
            if (spellDef == null)
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
                stunning = true;
                actualEnt = 0;
                actualPsy = 0;
            }
            if (wasWarmingUp && !verb.WarmingUp)
            {
                //结尾
                wasWarmingUp = false;
                stunning = false;

                //保证最后一句生成
                if (lineIndex < LineCount)
                    ThrowIncantation(LineCount);

                WarmUpFinish(lineIndex >= LineCount);
                MagicCircleEnd();
            }

            if (wasWarmingUp)
            {
                //生成咏唱词
                if (CurrentTick - lastLineTick >= IntervalTicks)
                {
                    ThrowIncantation(lineIndex);
                    lineIndex++;
                }
                TickWarmUp();
                TickMagicCircle();

                //咏唱时每tick消耗精神力和增加心灵熵
                var tracker = pawn?.psychicEntropy;
                if (tracker == null) return;
                float prePsy = tracker.CurrentPsyfocus;
                float preEnt = tracker.EntropyValue;
                tracker.OffsetPsyfocusDirectly(-PsyfocusCostPerTick);
                tracker.TryAddEntropy(EntropyGainPerTick);

                //记录实际值
                actualPsy += Mathf.Max(0, prePsy - tracker.CurrentPsyfocus);
                actualEnt += Mathf.Max(0, tracker.EntropyValue - preEnt);
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

        /// <returns>是否成功生成该行咏唱文字</returns>
        private void ThrowIncantation(int a)
        {
            if (a >= spellDef.incantationLines.Count) return;
            ThrowIncantation(spellDef.incantationLines[a]);
        }

        private void ThrowIncantation(string s)
        {
            //销毁上一句
            if (activeLine != null && !activeLine.Destroyed)
                activeLine.Destroy();

            Vector3 loc = new(0f, 0f, 0.5f);
            var mote = ThingMaker.MakeThing(IncantationDef) as Mote_Incantation;
            mote.Init(s, spellDef.themeColor, spellDef.lineInterval);
            mote.Attach(pawn, loc);
            GenSpawn.Spawn(mote, (pawn.DrawPos + loc).ToIntVec3(), pawn.Map);
            activeLine = mote;

            lastLineTick = CurrentTick;
            //Log.Message(s);
        }

        protected virtual void OnSpellKeyCasted()
        {
            if (spellDef.hasSpellKey)
            {
                ThrowIncantation(spellDef.spellKey);
                NotifyAll("SpellKey");
            }
        }

        protected virtual void WarmUpStart() { }

        /// <summary>
        /// 咏唱结束，如果有后摇则应在其期间禁用其他主动效果
        /// </summary>
        /// <param name="castDone">咏唱是否完整结束</param>
        protected virtual void WarmUpFinish(bool castDone) { }

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
                var a = pawn?.GetPharna()?.CheckStunning ?? true;
                Command_Action spellKey = new()
                {
                    defaultLabel = "引爆咒：" + spellDef.label,
                    defaultDesc = spellDef.spellKeyDescription,
                    icon = SpellKeyTexture,
                    action = () =>
                    {
                        OnSpellKeyCasted();
                    },
                    Disabled = !a,
                    disabledReason = a.Reason
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
            Scribe_Values.Look(ref actualEnt, "actualEnt");
            Scribe_Values.Look(ref actualPsy, "actualPsy");
            Scribe_Collections.Look(ref activeProjectiles, "activeProjectiles", LookMode.Reference);
            Scribe_Collections.Look(ref activeMotes, "activeMotes", LookMode.Reference);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                foreach (var mote in activeMotes)
                    mote?.Maintain();
            }
        }
    }

    public interface ISpellPowerModifier
    {
        void ApplySpellPowerModifier(ref float power);
    }
}