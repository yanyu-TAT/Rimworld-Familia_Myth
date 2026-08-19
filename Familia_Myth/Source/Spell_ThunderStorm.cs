using FamiliaMyth.Core;
using RimWorld;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class Spell_ThunderStorm : Spell
    {
        private const int BasicStrikeCount = 8;
        private const float BasicStrikeRadius = 5f;
        private const int TicksPerStrike = 60;

        private float strikeRadius;
        private int strikeCount = 0;
        public int StrikeCount
        {
            get => strikeCount;
            set
            {
                strikeCount = value;
                if (strikeCount <= 0)
                    StrikeDone();
            }
        }

        private static int thunderStormingCount = 0;
        public static bool ThunderStorming => thunderStormingCount > 0;

        private static WeatherDef thunderstormDef;
        private static WeatherDef ThunderstormDef =>
                thunderstormDef ??= DefDatabase<WeatherDef>.GetNamed("RainyThunderstorm");

        private int TotalStrikeCount => (int)(BasicStrikeCount * GetCurrentSpellPower());
        private float StrikeRadius => BasicStrikeRadius * GetCurrentSpellPower();

        protected override void WarmUpStart()
        {
            if (pawn?.Map == null) return;
            pawn.Map.weatherManager.TransitionTo(ThunderstormDef);
            thunderStormingCount++;
            //Log.Message("It will rain.");
        }

        protected override void WarmUpFinish(bool castDone)
        {
            if (pawn?.Map == null || !pawn.Spawned || pawn.DeadOrDowned)
                return;
            if (!castDone)
            {
                //恢复天气
                pawn.Map.weatherManager.TransitionTo(pawn.Map.weatherManager.lastWeather);
                thunderStormingCount--;
                return;
            }

            strikeCount = TotalStrikeCount;
            strikeRadius = StrikeRadius;
            stunning = true;
            int totalTicks = TicksPerStrike * strikeCount;

            for (int i = 0; i < strikeCount; i++)
                GameComponent_FamiliaMyth.Schedule(Rand.Range(60, 60 + totalTicks), DoStrike, pawn);
        }

        private void DoStrike()
        {
            //Log.Message($"{StrikeCount} Try Strike");
            StrikeCount--;
            if (pawn?.Map == null) return;
            var enemies = pawn.Map.mapPawns.AllPawnsSpawned
                .Where(p => (p.Faction?.HostileTo(pawn.Faction) ?? false) && p.Spawned && !p.DeadOrDowned)
                .ToList();

            IntVec3 loc;
            if (!enemies.NullOrEmpty())
            {
                var target = enemies.RandomElement();
                loc = target.Position + (Rand.UnitVector3 * strikeRadius / 2f).ToIntVec3();
            }
            else
            {
                loc = CellFinderLoose.RandomCellWith(
                    c => c.Standable(pawn.Map) && !pawn.Map.roofGrid.Roofed(c),
                    pawn.Map, 1000);
            }

            //雷击
            pawn.Map.weatherManager.eventHandler.AddEvent(new WeatherEvent_LightningStrike(pawn.Map, loc));
            //爆炸
            GenExplosion.DoExplosion(loc, pawn.Map, strikeRadius, DamageDefOf.Flame, pawn,
                damAmount: 25, damageFalloff: true, doSoundEffects: false, screenShakeFactor: 0.1f);
            //EMP效果
            GenExplosion.DoExplosion(loc, pawn.Map, strikeRadius, DamageDefOf.EMP, pawn,
                doSoundEffects: false);
        }

        private void StrikeDone()
        {
            stunning = false;
            if (pawn?.Map != null)
                pawn.Map.weatherManager.TransitionTo(pawn.Map.weatherManager.lastWeather);
            thunderStormingCount--;
        }
    }
}
