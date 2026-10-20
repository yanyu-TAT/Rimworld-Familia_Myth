using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    /// <summary>
    /// 布都御魂：咏唱结束后以自身为中心展开领域法阵，周期性地对范围内所有单位施加减速与打断，期间施法者自身无法行动。
    /// </summary>
    public class Spell_FutsuNoMitama : Spell, ISpellWithState
    {
        //基础持续时间与法阵半径（均乘法强）
        private const int BasicDurationTicks = 900;
        private const float BasicRadius = 8f;
        //效果循环：减速 / 打断 / 续定身共用同一分支
        private const int EffectIntervalTicks = 60;
        private const int StaggerTicks = 75;
        private const float StaggerMoveSpeedFactor = 0.4f;
        //巨剑入场时长（与 Mote_FutsuNoMitamaSword 一致）
        private const int SwordEntryTicks = 36;

        private static ThingDef circleDef;
        private static ThingDef CircleDef =>
            circleDef ??= DefDatabase<ThingDef>.GetNamed("Mote_FutsuNoMitama");

        private static ThingDef swordDef;
        private static ThingDef SwordDef =>
            swordDef ??= DefDatabase<ThingDef>.GetNamed("Mote_FutsuNoMitamaSword");

        private int remainingTicks = -1;
        private int totalTicks;
        private int elapsedTicks;
        private IntVec3 center;
        private float radius;
        private int effectCooldown;
        private Mote_FutsuNoMitama circleMote;
        private Mote_FutsuNoMitamaSword swordMote;
        private Hediff_SpellState state;

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

        public string EffectName => "FamiliaMyth.Ability.FutsuNoMitama".Translate();

        public Hediff_SpellState State { get => state; set => state = value; }

        protected override void WarmUpFinish(bool castDone)
        {
            if (!castDone || pawn?.Map == null || !pawn.Spawned || pawn.DeadOrDowned) return;

            float power = GetCurrentManaControl();
            radius = BasicRadius * power;
            totalTicks = remainingTicks = Mathf.Max(1, (int)(BasicDurationTicks * power));
            elapsedTicks = 0;
            effectCooldown = 0;
            center = pawn.Position;

            Map map = pawn.Map;
            if (ThingMaker.MakeThing(CircleDef) is Mote_FutsuNoMitama circle)
            {
                circle.Init(radius, Def.themeColor * 0.9f);
                GenSpawn.Spawn(circle, center, map);
                circle.Maintain();
                circleMote = circle;
            }
            if (ThingMaker.MakeThing(SwordDef) is Mote_FutsuNoMitamaSword sword)
            {
                GenSpawn.Spawn(sword, center, map);
                sword.Init(Def.themeColor);
                sword.Maintain();
                swordMote = sword;
            }

            ((ISpellWithState)this).NotifyStateStart(pawn);
            pawn.stances.stunner.StunFor(remainingTicks, pawn, addBattleLog: false, showMote: false);
        }

        public override void AbilityTick()
        {
            base.AbilityTick();
            if (!IsActive) return;

            if (pawn?.Map == null || !pawn.Spawned || pawn.DeadOrDowned)
            {
                EndEffect();
                return;
            }

            remainingTicks--;
            elapsedTicks++;
            circleMote?.Maintain();
            swordMote?.Maintain();

            if (swordMote != null)
            {
                int span = Mathf.Max(1, totalTicks - SwordEntryTicks);
                swordMote.Progress = Mathf.Clamp01((float)(elapsedTicks - SwordEntryTicks) / span);
            }

            if (effectCooldown > 0) effectCooldown--;
            if (effectCooldown <= 0)
            {
                ApplyFieldEffect();
                effectCooldown = EffectIntervalTicks;
            }

            if (!IsActive) EndEffect();
        }

        private void ApplyFieldEffect()
        {
            if (pawn?.Map is not Map map) return;

            float sqr = radius * radius;
            foreach (Pawn other in map.mapPawns.AllPawnsSpawned)
            {
                if (other == null || other.Position.DistanceToSquared(center) > sqr) continue;
                other.stances.stagger.StaggerFor(StaggerTicks, StaggerMoveSpeedFactor);
                other.stances.CancelBusyStanceSoft();
            }

            circleMote?.Pulse();
            pawn.stances.stunner.StunFor(Mathf.Max(remainingTicks, 1), pawn, addBattleLog: false, showMote: false);
        }

        private void EndEffect()
        {
            remainingTicks = 0;
            ((ISpellWithState)this).NotifyStateEnd(pawn);
            pawn?.stances?.stunner.StopStun();
            //停止维护后，两个 Mote 按 fadeOutUnmaintained 自行淡出并销毁
            circleMote = null;
            swordMote = null;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref remainingTicks, "remainingTicks", -1);
            Scribe_Values.Look(ref totalTicks, "totalTicks");
            Scribe_Values.Look(ref elapsedTicks, "elapsedTicks");
            Scribe_Values.Look(ref center, "center");
            Scribe_Values.Look(ref radius, "radius");
            Scribe_Values.Look(ref effectCooldown, "effectCooldown");
            Scribe_References.Look(ref circleMote, "circleMote");
            Scribe_References.Look(ref swordMote, "swordMote");
            Scribe_References.Look(ref state, "state");

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                circleMote?.Maintain();
                swordMote?.Maintain();
            }
        }
    }
}
