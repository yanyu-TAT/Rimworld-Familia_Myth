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
        private static readonly Texture2D CanceKeyTexture = ContentFinder<Texture2D>.Get("UI/Designators/Cancel");

        private static ThingDef magicCircleDef;
        private static ThingDef MagicCircleDef =>
            magicCircleDef ??= DefDatabase<ThingDef>.GetNamed("Mote_MagicCircleLayer");

        private static StatDef spellPowerDef;
        private static StatDef SpellPowerDef =>
            spellPowerDef ??= DefDatabase<StatDef>.GetNamed("SpellPower");

        private static ThingDef incantationTextDef;
        private static ThingDef IncantationDef =>
            incantationTextDef ??= DefDatabase<ThingDef>.GetNamed("Mote_Incantation");

        public SpellDef Def => def as SpellDef;

        public RewardType RewardType => RewardType.Spell;

        protected int LineCount => Def?.incantationLines.Count - 1 ?? 1;

        protected int warmupTicksLeft;
        protected int WarmupTicks => Def.WarmupTime.SecondsToTicks();
        protected float WarmupProgress => Mathf.Clamp01(1f - (float)warmupTicksLeft / WarmupTicks);

        protected static int CurrentTick => Find.TickManager.TicksGame;

        public RewardRarity Rarity => Def.rarity;
        public string Label => Def.label;
        public string Description => Def.description;

        protected float PsyfocusCostPerTick => (def?.PsyfocusCost / (Def.WarmupTime * 60f)) ?? 0f;
        protected float EntropyGainPerTick => (def?.EntropyGain / (Def.WarmupTime * 60f)) ?? 0f;

        public List<Projectile_MagicMissileBase> activeProjectiles = new();

        public Mote_Incantation activeLine;

        public List<Mote_MagicCircle> activeMotes = new();

        protected bool isWarmingUp = false;
        protected bool wasWarmingUp = false;
        public bool IsWarmingUp => isWarmingUp;
        protected int lineIndex = 0;

        protected float actualPsy;
        protected float actualEnt;

        public bool stunning = false;

        public bool CanConcurrentCast => Def.canConcurrentCast;

        public virtual void RewardInitialize()
        {
            def = DefDatabase<SpellDef>.GetNamed(GetType().Name, false);

            //尝试随机生成魔法阵样式
            if (Def.magicCircles.NullOrEmpty())
                Def.magicCircles = Def.DefaultMagicCircles;
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

                float gain = def.EntropyGain - tracker.RecoveryRate * Def.WarmupTime;
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
            if (Def == null)
                return;

            if (verb == null || verb.CasterPawn == null)
                return;

            if (!isWarmingUp)
            {
                //开始咏唱
                if (verb.WarmingUp && !wasWarmingUp)
                {
                    lineIndex = 1;
                    ThrowIncantation(0);
                    warmupTicksLeft = WarmupTicks - 1;
                    isWarmingUp = true;
                    stunning = true;
                    actualEnt = 0;
                    actualPsy = 0;
                    WarmUpStart();
                    MagicCircleStart();
                }
                wasWarmingUp = verb.WarmingUp;
                return;
            }

            wasWarmingUp = verb.WarmingUp;

            //被眩晕暂停咏唱
            if (pawn.stances.stunner.Stunned)
            {
                foreach (var mote in activeMotes)
                    mote?.Maintain();
                return;
            }

            warmupTicksLeft--;
            //完成咏唱
            if (warmupTicksLeft <= 0)
            {
                CastComplete(true);
                return;
            }

            //咏唱中断
            if (!verb.WarmingUp)
            {
                Log.Message($"Left Ticks:{warmupTicksLeft}");
                CastComplete(false);
                return;
            }

            //根据进度生成咏唱词
            int targetLine = Mathf.Min((int)(WarmupProgress * LineCount + 0.01f), LineCount - 1);
            while (lineIndex <= targetLine)
                ThrowIncantation(lineIndex++);

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

        private void CastComplete(bool castDone)
        {
            //结尾
            isWarmingUp = false;
            stunning = false;

            //保证最后一句生成
            if (lineIndex <= LineCount)
                ThrowIncantation(LineCount);

            //Log.Message($"castDone: {castDone}");
            WarmUpFinish(castDone);
            MagicCircleEnd();
            pawn.stances.SetStance(new Stance_Mobile());
        }

        private void OnCanceled()
        {
            if (pawn.Spawned && pawn.stances.curStance is Stance_Warmup)
                pawn.stances.SetStance(new Stance_Mobile());
        }

        private void MagicCircleEnd()
        {
            activeMotes.Clear();
        }

        private void MagicCircleStart()
        {
            if (Def.magicCircles.NullOrEmpty()) return;
            activeMotes.Clear();
            TickMagicCircle();
        }

        private void TickMagicCircle()
        {
            if (Def.magicCircles.NullOrEmpty() || pawn?.Map == null) return;

            //生成
            var layers = Def.magicCircles;
            for (int i = 0; i < layers.Count; i++)
            {
                if (i < activeMotes.Count && activeMotes[i]?.Spawned == true) continue;
                if (WarmupProgress < layers[i].appearProgress) continue;
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
            if (a >= Def.incantationLines.Count) return;
            ThrowIncantation(Def.incantationLines[a]);
        }

        private void ThrowIncantation(string s)
        {
            //销毁上一句
            if (activeLine != null && !activeLine.Destroyed)
                activeLine.Destroy();

            Vector3 loc = new(0f, 0f, 0.5f);
            var mote = ThingMaker.MakeThing(IncantationDef) as Mote_Incantation;
            mote.Init(s, Def.themeColor, Def.lineInterval);
            mote.Attach(pawn, loc);
            GenSpawn.Spawn(mote, (pawn.DrawPos + loc).ToIntVec3(), pawn.Map);
            activeLine = mote;
            //Log.Message(s);
        }

        protected virtual void OnSpellKeyCasted()
        {
            if (Def.hasSpellKey)
            {
                ThrowIncantation(Def.spellKey);
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
            verb.verbProps.warmupTime = Def.WarmupTime;
            if (verb is Verb_CastSpell castSpell && this is ISpellWithTargetRange spellWithTargetRange)
                castSpell.spell = spellWithTargetRange;
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
                //正在释放时隐藏图标
                if (command is not Command_Ability || !isWarmingUp)
                    yield return command;

            if (isWarmingUp)
            {
                yield return new Command_Action()
                {
                    defaultLabel = $"取消释放{Def.label}",
                    defaultDesc = $"取消正在释放的{Def.label}",
                    icon = CanceKeyTexture,
                    action = OnCanceled
                };
            }

            if (Def.hasSpellKey)
            {
                var a = pawn?.GetPharna()?.CheckStunning ?? true;
                Command_Action spellKey = new()
                {
                    defaultLabel = "引爆咒：" + Def.label,
                    defaultDesc = Def.spellKeyDescription,
                    icon = SpellKeyTexture,
                    action = OnSpellKeyCasted,
                    Disabled = !a,
                    disabledReason = a.Reason
                };
                yield return spellKey;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref warmupTicksLeft, "warmupTicksLeft");
            Scribe_Values.Look(ref lineIndex, "lineIndex");
            Scribe_Values.Look(ref isWarmingUp, "isWarmingUp");
            Scribe_Values.Look(ref actualEnt, "actualEnt");
            Scribe_Values.Look(ref actualPsy, "actualPsy");
            Scribe_Values.Look(ref wasWarmingUp, "wasWarmingUp");
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