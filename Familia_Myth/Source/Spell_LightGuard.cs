using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class Spell_LightGuard : Spell, ISpellWithRemainingTime
    {
        //基础光球数量
        private const int BasicOrbCount = 4;
        //基础持续时间
        private const int BasicDurationTicks = 3600;
        //悬浮环绕半径
        private const float HoveringRadius = 2f;
        //悬浮角速度
        private const float HoveringAngularSpeed = 3f;

        private int remainingTicks = -1;
        public int RemainingTicks => remainingTicks;

        public bool IsActive => remainingTicks > 0;

        private static ThingDef orbDef;
        private static ThingDef OrbDef =>
            orbDef ??= DefDatabase<ThingDef>.GetNamed("Projectile_LightOrb");

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

        public string EffectName => "守护光灵";

        private Hediff_SpellState state;
        public Hediff_SpellState State { get => state; set => state = value; }

        protected override void WarmUpFinish(bool castDone)
        {
            if (!castDone || pawn?.Map == null || !pawn.Spawned || pawn.DeadOrDowned)
                return;

            int totalTicks = (int)(BasicDurationTicks * GetCurrentSpellPower());
            int cnt = Mathf.RoundToInt(BasicOrbCount * GetCurrentSpellPower());
            if (IsActive)
            {
                //叠加时间并补齐光球
                remainingTicks += totalTicks;
                SpawnOrbs(cnt);
            }
            else
            {
                remainingTicks = totalTicks;
                SpawnOrbs(cnt);
                ((ISpellWithRemainingTime)this).NotifyStateStart(pawn);
            }
        }

        private void SpawnOrbs(int cnt)
        {
            int retryCnt = 0;
            if (pawn?.Map == null || cnt <= 0) return;

            //补齐数量
            while (activeProjectiles.Count < cnt)
            {
                var orb = (Projectile_LightOrb)GenSpawn.Spawn(OrbDef, pawn.Position, pawn.Map);
                if (orb == null)
                {
                    retryCnt++;
                    if (retryCnt <= 10)
                        continue;
                    else
                    {
                        Log.Error($"[FamiliaMyth] Retried Over 10 Times When Creating Light Orb For {pawn.Name.ToStringShort}");
                        return;
                    }
                }

                orb.Init(this);
                orb.currentState = new HoveringState_Orbit(
                    orb, HoveringAngularSpeed, HoveringRadius,
                    stiffness: 50, damping: 15);
                activeProjectiles.Add(orb);
            }
            if (activeProjectiles.NullOrEmpty()) return;

            //重新分配位置
            float start = ((HoveringState_Orbit)activeProjectiles.First().currentState).angle;
            float step = 360f / activeProjectiles.Count;
            for (int i = 0; i < activeProjectiles.Count; i++)
                ((HoveringState_Orbit)activeProjectiles[i].currentState).angle = (start + i * step) % 360f;
        }

        public override void AbilityTick()
        {
            base.AbilityTick();
            if (!IsActive) return;
            remainingTicks--;

            if (!IsActive || pawn?.Map == null || !pawn.Spawned || pawn.Dead)
                EndLightOrb();
        }

        protected override void OnSpellKeyCasted()
        {
            base.OnSpellKeyCasted();
            EndLightOrb();
        }

        private void EndLightOrb()
        {
            remainingTicks = -1;
            foreach (var orb in activeProjectiles.ToList())
                if (orb != null && orb.Spawned)
                    orb.FadeOut();
            activeProjectiles.Clear();

            ((ISpellWithRemainingTime)this).NotifyStateEnd(pawn);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref remainingTicks, "remainingTicks");
            Scribe_References.Look(ref state, "state");
        }
    }
}
