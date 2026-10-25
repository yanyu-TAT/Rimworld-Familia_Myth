using FamiliaMyth.Core;
using FamiliaMyth.Pharna;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

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

        private static StatDef manaControlDef;
        private static StatDef ManaControlDef =>
            manaControlDef ??= DefDatabase<StatDef>.GetNamed("ManaControl");

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
        public string DefName => Def?.defName;
        public string Label => Def.label;
        public string Description => Def.description;

        protected float ManaCostPerTick => (Def?.manaCost / (Def.WarmupTime * 60f)) ?? 0f;
        protected float EntropyGainPerTick => (def?.EntropyGain / (Def.WarmupTime * 60f)) ?? 0f;

        public List<Projectile_MagicMissileBase> activeProjectiles = new();

        public Mote_Incantation activeLine;

        public List<Mote_MagicCircle> activeMotes = new();

        protected bool isWarmingUp = false;
        protected bool wasWarmingUp = false;
        public bool IsWarmingUp => isWarmingUp;
        protected int lineIndex = 0;

        protected float actualMana;
        protected float actualEnt;

        public bool stunning = false;

        //咏唱音效：低频持续音句柄与吐息节流
        private Sustainer humSustainer;
        private int lastBreathTick = -1;

        private const int BreathThrottleTicks = 18;

        public bool CanConcurrentCast => Def.canConcurrentCast;

        private List<MagicCircleLayer> magicCircles;
        public List<MagicCircleLayer> MagicCircles
        {
            get
            {
                if (!Def.magicCircles.NullOrEmpty())
                    return Def.magicCircles;
                if (!magicCircles.NullOrEmpty()) 
                    return magicCircles;

                //尝试随机生成魔法阵样式
                magicCircles = new List<MagicCircleLayer>();
                int cnt = (int)Def.rarity + 1;
                for (int i = 0; i < cnt; i++)
                {
                    var layer = new MagicCircleLayer
                    {
                        texPath = $"Familia_Myth/Things/Mote/MagicCircles/{Rand.Range(1, 4)}",
                        drawColor = Def.themeColor,
                        scaleFrom = 0.5f,
                        size = new((i + 1) * 5, (i + 1) * 5),
                        altitude = i * 0.01f,
                        rotate = Rand.Range(-20f, 20f),
                        appearProgress = i * 0.25f,
                        initialRotation = Rand.Range(0f, 360f)
                    };
                    magicCircles.Add(layer);
                }
                return magicCircles;
            }
        }

        public virtual void RewardInitialize()
        {
            def = DefDatabase<SpellDef>.GetNamed(GetType().Name, false);
            if (Def == null) return;
        }

        public override AcceptanceReport CanCast
        {
            get
            {
                AcceptanceReport canCast = base.CanCast;
                if (!canCast) return canCast;

                var check = pawn?.GetPharna()?.CheckStunning ?? true;
                if (!check)
                    return "FamiliaMyth.Ability.StunnedCannotCast".Translate(check.Reason);

                var tracker = pawn?.psychicEntropy;
                if (tracker == null) return true;

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
                //咏唱已结束但持续音未收，兜底停掉
                humSustainer?.End();
                humSustainer = null;

                //开始咏唱
                if (verb.WarmingUp && !wasWarmingUp)
                {
                    lineIndex = 1;
                    isWarmingUp = true;
                    ThrowIncantation(0);
                    warmupTicksLeft = WarmupTicks - 1;
                    stunning = true;
                    actualEnt = 0;
                    actualMana = 0;
                    WarmUpStart();
                    MagicCircleStart();
                    StartCastHum();
                }
                wasWarmingUp = verb.WarmingUp;
                return;
            }

            wasWarmingUp = verb.WarmingUp;

            //逐 tick 维持低频音；漏维持会被 Sustainer 自动结束，避免残留
            humSustainer?.Maintain();

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
                //Log.Message($"[Familia Myth] Cast Interrupted With Left Ticks:{warmupTicksLeft}");
                CastComplete(false);
                return;
            }

            //根据进度生成咏唱词
            int targetLine = Mathf.Min((int)(WarmupProgress * LineCount + 0.01f), LineCount - 1);
            while (lineIndex <= targetLine)
                ThrowIncantation(lineIndex++);

            TickWarmUp();
            TickMagicCircle();

            var tracker = pawn?.psychicEntropy;
            var pharna = pawn?.GetPharna();
            if (tracker == null || pharna == null) return;

            //检查魔力储量
            if (pharna.manaPool < ManaCostPerTick)
            {
                Messages.Message("FamiliaMyth.Ability.ManaExhaust".Translate(pawn.Name?.ToStringShort ?? "Unknown"), MessageTypeDefOf.CautionInput);
                CastComplete(false);
                return;
            }
            //检查心灵熵
            if (tracker.WouldOverflowEntropy(EntropyGainPerTick)) //WouldOverflowEntropy中有检查是否启用心灵熵限制
            {
                Messages.Message("FamiliaMyth.Ability.EntropyOverlimit".Translate(pawn.Name?.ToStringShort ?? "Unknown"), MessageTypeDefOf.CautionInput);
                CastComplete(false);
                return;
            }

            //咏唱时每tick消耗魔力并增加心灵熵
            float preEnt = tracker.EntropyValue;
            actualMana += pharna.SpendMana(ManaCostPerTick);
            tracker.TryAddEntropy(EntropyGainPerTick, overLimit: true); //是否超限已检查过，强制增加

            //记录实际值
            actualEnt += Mathf.Max(0, tracker.EntropyValue - preEnt);
        }

        private void CastComplete(bool castDone)
        {
            //结尾
            isWarmingUp = false;
            stunning = false;

            //完整咏唱结束时记录施法日志
            if (castDone)
                CombatLogHelper.WriteCastLog(pawn, Def);

            //保证最后一句生成；此时 isWarmingUp 已为 false，不带吐息
            if (lineIndex <= LineCount)
                ThrowIncantation(LineCount);

            //Log.Message($"castDone: {castDone}");
            StopCastHum(castDone);
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
            if (MagicCircles.NullOrEmpty()) return;
            activeMotes.Clear();
            TickMagicCircle();
        }

        private void TickMagicCircle()
        {
            if (MagicCircles.NullOrEmpty() || pawn?.Map == null) return;

            //生成
            var layers = MagicCircles;
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

        //生成第 a 句咏唱词，咏唱进行中才带吐息
        private void ThrowIncantation(int a)
        {
            if (a >= Def.incantationLines.Count) return;
            ThrowIncantation(Def.incantationLines[a]);
            PlayCastBreath();
        }

        private void StartCastHum()
        {
            humSustainer?.End();
            humSustainer = FamiliaMythSounds.CastHum?.TrySpawnSustainer(
                SoundInfo.InMap(new TargetInfo(pawn.Position, pawn.Map), MaintenanceType.PerTick));
        }

        private void StopCastHum(bool castDone)
        {
            humSustainer?.End();
            humSustainer = null;

            if (pawn == null) return;

            SoundDef sound = castDone ? FamiliaMythSounds.CastRelease : FamiliaMythSounds.CastAbort;
            sound?.PlayOneShot(SoundInfo.InMap(pawn));
        }

        //每句咏唱词一次吐息；非咏唱状态（如完成时补最后一句）不吹气，卡顿补句按 tick 节流
        private void PlayCastBreath()
        {
            if (pawn == null || !isWarmingUp) return;
            if (CurrentTick - lastBreathTick < BreathThrottleTicks) return;

            lastBreathTick = CurrentTick;
            FamiliaMythSounds.CastBreath?.PlayOneShot(SoundInfo.InMap(pawn));
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
            if (verb is Verb_CastSpell castSpell && this is ISpellWithTargetRange spellWithTargetRange)
                castSpell.spell = spellWithTargetRange;
            pharna.spells.Add(this);

            TutorialLetterHelper.TrySend(TutorialLetterHelper.FirstSpell, pharna.pawn);
        }

        private static readonly Dictionary<SpellType, StatDef> ManaControlStatDefs = new();

        private static StatDef GetManaControlStat(SpellType type)
        {
            if (type == SpellType.None || type == SpellType.Special)
                return ManaControlDef;

            if (!ManaControlStatDefs.TryGetValue(type, out var stat))
            {
                stat = DefDatabase<StatDef>.GetNamed("ManaControl_" + type.ToString());
                ManaControlStatDefs[type] = stat;
            }
            return stat;
        }

        public virtual float GetCurrentManaControl()
        {
            if (pawn == null) return 1f;
            return pawn.GetStatValue(GetManaControlStat(Def.spellType));
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
                    defaultLabel = "FamiliaMyth.Ability.CancelCastLabel".Translate(Def.label),
                    defaultDesc = "FamiliaMyth.Ability.CancelCastDesc".Translate(Def.label),
                    icon = CanceKeyTexture,
                    action = OnCanceled
                };
            }

            if (Def.hasSpellKey)
            {
                var a = pawn?.GetPharna()?.CheckStunning ?? true;
                Command_Action spellKey = new()
                {
                    defaultLabel = "FamiliaMyth.Ability.SpellKeyLabel".Translate(Def.label),
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
            Scribe_Values.Look(ref actualMana, "actualMana");
            Scribe_Values.Look(ref wasWarmingUp, "wasWarmingUp");
            Scribe_Collections.Look(ref magicCircles, "magicCircles", LookMode.Deep);
            Scribe_Collections.Look(ref activeProjectiles, "activeProjectiles", LookMode.Reference);
            Scribe_Collections.Look(ref activeMotes, "activeMotes", LookMode.Reference);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                humSustainer = null;
                foreach (var mote in activeMotes)
                    mote?.Maintain();
            }
        }
    }

    public interface IManaControlModifier
    {
        bool ShouldApplyManaControl(SpellType type) => true;
        void ApplyManaControlModifier(ref float power);
    }
}