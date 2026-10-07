using FamiliaMyth.Pharna;
using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class Spell_ZeoGullveig : Spell, ISpellWithState, ISpellWithTargetRange
    {
        private const int ParticleIntervalTicks = 10; // 上浮粒子生成间隔

        //基础持续时间
        private const int BasicDuration = 600;
        //基础单次治疗量
        private const int BasicHealBudget = 250;
        //治疗间隔
        private const int HealIntervalTicks = 90;
        //自动治愈治疗量
        private const int AutoHealBudget = 8;
        //自动治愈单次魔力消耗
        private const float AutoHealManaCost = 40f;
        //法阵范围
        public const float Radius = 10f;
        //离开法阵的效果持续时间
        private const int HediffDuationTicks = 150;

        private static ThingDef circleDef;
        private static ThingDef CircleDef =>
            circleDef ??= DefDatabase<ThingDef>.GetNamed("Mote_ZeoGullveigCircle");

        private static HediffDef hediffDef;
        private static HediffDef HediffDef =>
            hediffDef ??= DefDatabase<HediffDef>.GetNamed("Hediff_ZeoGullveig");

        private static FleckDef healSpark;
        private static FleckDef HealSpark =>
            healSpark ??= DefDatabase<FleckDef>.GetNamed("HealSpark");


        private int remainingTicks = -1;
        private IntVec3 targetPos;
        private Map targetMap;
        private float healBudget;
        private Mote_ZeoGullveig circle;
        private bool isAutoHealing = false;
        private int circleHealCooldown = -1;
        private int autoHealCooldown = -1;

        public bool IsActive => remainingTicks > 0;
        public string RemainingTimeText
        {
            get
            {
                if (!IsActive) return "";
                if (remainingTicks < 2500)
                    return remainingTicks.ToStringSecondsFromTicks("F0");
                return remainingTicks.ToStringTicksToPeriod();
            }
        }

        public string EffectName => "FamiliaMyth.Ability.ZeoGullveig".Translate();

        private Hediff_SpellState state;
        public Hediff_SpellState State { get => state; set => state = value; }

        public Color HighlightColor => Color.green;

        public bool CheckLOS => true;

        public bool CanHitTargetFrom(IntVec3 root, LocalTargetInfo localTarget) => true;

        public IEnumerable<IntVec3> GetCells(LocalTargetInfo localTarget)
        {
            if (!localTarget.IsValid) return null;
            return GenRadial.RadialCellsAround(localTarget.Cell, Radius, true);
        }

        protected override void WarmUpStart()
        {
            targetPos = verb.CurrentTarget.Cell;
            targetMap = pawn?.Map;
        }

        protected override void WarmUpFinish(bool castDone)
        {
            if (!castDone || targetMap == null || !targetPos.InBounds(targetMap))
                return;

            float power = GetCurrentManaControl();
            healBudget = BasicHealBudget * power;
            remainingTicks = (int)(BasicDuration * power);

            circle = ThingMaker.MakeThing(CircleDef) as Mote_ZeoGullveig;
            if (circle == null) return;

            circle.Init();
            GenSpawn.Spawn(circle, targetPos, targetMap);
            circle.Maintain();

            ((ISpellWithState)this).NotifyStateStart(pawn);
        }

        public override void AbilityTick()
        {
            base.AbilityTick();

            if (isAutoHealing)
                TickAutoHeal();

            if (IsActive)
                TickHealCircle();
        }

        private void TickHealCircle()
        {
            remainingTicks--;
            if (IsActive)
                circle?.Maintain();
            else
                ((ISpellWithState)this).NotifyStateEnd(pawn);

            if (Find.TickManager.TicksGame % ParticleIntervalTicks == 0)
                SpawnParticles();

            if (circleHealCooldown > 0)
            {
                circleHealCooldown--;
                return;
            }

            HealArea();
        }

        private void HealArea()
        {
            var pawns = targetMap?.mapPawns.AllPawnsSpawned
                .Where(p => !pawn.Dead && p.Faction == pawn.Faction && p.Position.DistanceToSquared(targetPos) <= Radius * Radius)
                .ToList();
            if (pawns.NullOrEmpty()) return;

            foreach (var p in pawns)
            {
                p.RestoreParts(2, pawn);
                p.TendInjuries(5, 1f, pawn);
            }

            float budget = healBudget;
            Log.Message($"Trying To Heal {pawns.Count} Targets With Budget: {budget}");
            foreach (var p in pawns)
            {
                budget = p.HealInjuries(budget, pawn);
                if (budget <= 0) break;
            }
            Log.Message($"Heal Done. Budget Left: {budget}");

            foreach (var p in pawns)
            {
                var hediff = p.health.GetOrAddHediff(HediffDef);
                var timer = hediff.TryGetComp<HediffComp_Disappears>();
                timer?.SetDuration(HediffDuationTicks);
                //疼痛抑制的伟业贡献由 HediffComp_FeatSupport 按实际生效时间累计
                hediff?.TryGetComp<HediffComp_FeatSupport>()?.SetCaster(pawn);
            }

            circleHealCooldown = HealIntervalTicks;

            if (budget < healBudget)
                circle?.Flash();
        }

        private void SpawnParticles()
        {
            if (targetMap == null || Radius <= 0f) return;
            for (int i = 0; i < 4; i++)
            {
                var center = targetPos.ToVector3Shifted();
                var ang = Rand.Range(0f, 360f);
                var start = center + Rand.InsideUnitCircleVec3 * Radius;
                var data = FleckMaker.GetDataStatic(start, targetMap, HealSpark, 1f);
                data.velocity = Vector3.forward * Rand.Range(0.4f, 1.0f);
                data.scale = Rand.Range(0.5f, 1.5f);
                data.rotation = Rand.Range(0f, 360f);
                data.instanceColor = Def.themeColor;
                targetMap.flecks.CreateFleck(data);
            }
        }

        private void TickAutoHeal()
        {
            if (autoHealCooldown > 0)
            {
                autoHealCooldown--;
                return;
            }

            var pharna = pawn?.GetPharna();
            if (pharna == null) return;

            if (pharna.manaPool < AutoHealManaCost)
            {
                isAutoHealing = false;
                Messages.Message("FamiliaMyth.Ability.ZeoGullveigNoMana".Translate(pawn.Name.ToStringShort), MessageTypeDefOf.NeutralEvent);
                return;
            }

            pharna.SpendMana(AutoHealManaCost);
            pawn.RestoreParts(1);
            pawn.TendInjuries(3);
            pawn.HealInjuries(AutoHealBudget);

            autoHealCooldown = HealIntervalTicks;
            SpawnSelfParticles();
        }

        private void SpawnSelfParticles()
        {
            if (pawn?.Map == null) return;
            var loc = pawn.Position.ToVector3Shifted();
            for (int i = 0; i < 4; i++)
            {
                var start = loc + Rand.InsideUnitCircleVec3 * 0.5f;
                var data = FleckMaker.GetDataStatic(start, pawn.Map, HealSpark, 1f);
                data.velocity = Vector3.forward * Rand.Range(0.2f, 0.5f);
                data.scale = Rand.Range(0.8f, 1.2f);
                data.rotation = Rand.Range(0f, 360f);
                data.instanceColor = Def.themeColor.WithAlpha(0.7f);
                pawn.Map.flecks.CreateFleck(data);
            }
        }

        private static readonly Texture2D ToggleIcon = ContentFinder<Texture2D>.Get("Familia_Myth/UI/Commands/ZeoGullveig", true);
        private Command_Toggle command;
        private Command_Toggle Command =>
            command ??= new Command_Toggle
            {
                icon = ToggleIcon,
                isActive = () => isAutoHealing,
                toggleAction = () => isAutoHealing = !isAutoHealing,
                defaultLabel = "FamiliaMyth.Ability.ZeoGullveigAutoLabel".Translate(),
                defaultDesc = "FamiliaMyth.Ability.ZeoGullveigAutoDesc".Translate(),
                disabledReason = "FamiliaMyth.Ability.PawnDead".Translate(pawn.Name.ToStringShort)
            };

        public override IEnumerable<Command> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
                yield return gizmo;

            Command.Disabled = pawn.Dead;
            yield return Command;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref remainingTicks, "remainingTicks", -1);
            Scribe_Values.Look(ref targetPos, "targetPos");
            Scribe_Values.Look(ref healBudget, "healBudget");
            Scribe_Values.Look(ref isAutoHealing, "isAutoHealing");
            Scribe_Values.Look(ref autoHealCooldown, "autoHealCooldown");
            Scribe_Values.Look(ref circleHealCooldown, "circleHealCooldown");
            Scribe_References.Look(ref targetMap, "targetMap");
            Scribe_References.Look(ref circle, "circle");
            Scribe_References.Look(ref state, "state");
        }
    }
}
