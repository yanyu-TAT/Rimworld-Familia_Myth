using RimWorld;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class Spell_KingsSword : Spell, ISpellWithState
    {
        //基础持续时间
        private const int BasicSwordDuration = 3600;
        //基础攻击范围
        private const float BasicAttackRange = 2.5f;
        //基础环绕半径
        private const float BasicOrbitRadius = 4f;
        //环绕角速度
        public const float OrbitAngularSpeed = 0.5f;
        //悬浮环绕时的移速
        public const float HoveringSpeed = 0.08f;

        private static ThingDef swordDef;
        private static ThingDef SwordDef =>
            swordDef ??= ThingDef.Named("Projectile_KingsSword");

        public Projectile_KingsSword Sword
        {
            get => activeProjectiles.FirstOrDefault() as Projectile_KingsSword;
            set
            {
                if (value == null) activeProjectiles.Clear();
                if (activeProjectiles?.Count >= 1)
                {
                    activeProjectiles[0] = value;
                    return;
                }

                activeProjectiles.Add(value);
            }
        }
        //重复释放次数
        public int usedTimes = 0;
        private int remainingTicks = 0;
        public bool IsActive => remainingTicks > 0;

        public float AttackRange => BasicAttackRange * usedTimes;
        public float OrbitRadius => BasicOrbitRadius * usedTimes;
        public float DamageMultiplier => usedTimes;
        public float SwordScale => usedTimes;

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

        public string EffectName => "君王之剑";

        private Hediff_SpellState state;
        public Hediff_SpellState State { get => state; set => state = value; }

        protected override void WarmUpFinish(bool castDone)
        {
            if (!castDone || pawn?.Map == null || !pawn.Spawned || pawn.DeadOrDowned)
                return;

            int totalTicks = (int)(BasicSwordDuration * GetCurrentSpellPower());
            //大剑已在场
            if (IsActive)
            {
                usedTimes++;
                remainingTicks += totalTicks;
            }
            //初次召唤
            else
            {
                usedTimes = 1;
                remainingTicks = totalTicks;
                SpawnSword();
            }
        }

        private void SpawnSword()
        {
            if (Sword != null && Sword.Spawned) return;

            Sword = (Projectile_KingsSword)GenSpawn.Spawn(SwordDef, pawn.Position, pawn.Map);
            if (Sword == null) return;

            Sword.Init(this);
            Sword.currentState = new HoveringState_Orbit(Sword, OrbitAngularSpeed, OrbitRadius);
            Sword.currentState.SetMaxV(HoveringSpeed);

            ((ISpellWithState)this).NotifyStateStart(pawn);
            //Log.Message("Sword Spawned");
        }

        public override void AbilityTick()
        {
            base.AbilityTick();
            if (!IsActive) return;
            remainingTicks--;
            if (pawn?.Map == null || !pawn.Spawned || pawn.Dead || Sword == null || !Sword.Spawned || !IsActive)
                EndSword();
        }

        private void EndSword()
        {
            remainingTicks = 0;
            usedTimes = 0;
            Log.Message("Sword End");
            if (Sword != null && Sword.Spawned)
                Sword.FadeOut();
            activeProjectiles.Clear();

            ((ISpellWithState)this).NotifyStateEnd(pawn);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref remainingTicks, "remainingTicks");
            Scribe_Values.Look(ref usedTimes, "usedTimes");
            Scribe_References.Look(ref state, "state");
        }
    }
}
