using FamiliaMyth.Feat;
using FamiliaMyth.Pharna;
using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class Spell_TheWorld : Spell, ISpellWithState
    {
        private const int BasicDurationTicks = 1800;
        private const float FieldRadius = 50f;

        private int totalTicks;
        private int remainingTicks;
        private IntVec3? fieldCenter;
        private static Dictionary<Pawn, IntVec3> Fields => GameComponent_TheWorld.Instance?.fields;

        public bool IsActive => remainingTicks > 0;
        public static bool AnyActiveField => Fields.Count > 0;

        private Mote_Clock clockMote;
        private static ThingDef clockMoteDef;
        private static ThingDef ClockMoteDef =>
            clockMoteDef ??= DefDatabase<ThingDef>.GetNamed("Mote_Clock");

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

        public string EffectName => "FamiliaMyth.Ability.TimeStop".Translate();

        private Hediff_SpellState state;
        public Hediff_SpellState State { get => state; set => state = value; }

        public override AcceptanceReport CanCast
        {
            get
            {
                var canCast = base.CanCast;
                if (!canCast) return canCast;

                if (!AnyActiveField) 
                    return true;
                if (Fields.Keys.Any(p => p == pawn))
                    return "FamiliaMyth.Ability.TimeStopActive".Translate();

                return true;
            }
        }

        protected override void WarmUpFinish(bool castDone)
        {
            if (!castDone || pawn?.Map == null || !pawn.Spawned || pawn.DeadOrDowned) return;
            totalTicks = (int)(BasicDurationTicks * GetCurrentManaControl());
            remainingTicks = totalTicks;
            //时停无 Hediff 载体，按生效时长计入支援贡献
            GameComponent_FeatTracker.Instance?.RecordSupport(pawn, totalTicks / 60f * FeatUtility.ControlPerSecond);
            fieldCenter = pawn.Position;
            Fields[pawn] = pawn.Position;
            
            var mote = (Mote_Clock)MoteMaker.MakeStaticMote(
                fieldCenter.Value.ToVector3ShiftedWithAltitude(AltitudeLayer.MoteLow),
                pawn.Map, ClockMoteDef, 1f, true);
            if (mote != null)
            {
                mote.Init(pawn.Position, FieldRadius);
                clockMote = mote;
                clockMote.Maintain();
                clockMote.ratio = 1f;
            }

            ((ISpellWithState)this).NotifyStateStart(pawn);
        }

        public override void AbilityTick()
        {
            base.AbilityTick();
            if (!IsActive) return;

            remainingTicks--;
            if (pawn?.Map == null || !pawn.Spawned || pawn.DeadOrDowned || !IsActive)
            {
                EndTimeFreeze();
                return;
            }

            if (clockMote != null && IsActive)
            {
                clockMote.ratio = (float)remainingTicks / totalTicks;
                clockMote.Maintain();
            }
        }

        protected override void OnSpellKeyCasted()
        {
            base.OnSpellKeyCasted();
            EndTimeFreeze(true);
        }

        private void EndTimeFreeze(bool bySpellKey = false)
        {
            if (bySpellKey && remainingTicks > 0)
            {
                float r = Mathf.Clamp01((float)remainingTicks / totalTicks);
                var tracker = pawn?.psychicEntropy;
                if (tracker != null)
                {
                    //按比例恢复/消除
                    pawn.GetPharna()?.GainMana(r * actualMana);
                    tracker.TryAddEntropy(-r * actualEnt);
                }
            }

            //移除效果
            remainingTicks = 0;
            fieldCenter = null;
            Fields.Remove(pawn);

            ((ISpellWithState)this).NotifyStateEnd(pawn);
        }

        public override void OnGizmoUpdate()
        {
            base.OnGizmoUpdate();
            if (CanCast)
                GenDraw.DrawRadiusRing(pawn.Position, FieldRadius);
        }

        public static bool ShouldTick(Thing thing)
        {
            if (!thing.Spawned) 
                return true;

            if (thing is Mote_Clock || thing is Mote_ClockHand)
                return true;

            if (thing is Pawn pawn && Fields.ContainsKey(pawn))
                return true;

            foreach(var center in Fields.Values)
                if (thing.Position.DistanceToSquared(center) <= FieldRadius * FieldRadius)
                    return false;

            return true;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref totalTicks, "totalTicks");
            Scribe_Values.Look(ref remainingTicks, "remainingTicks");
            Scribe_Values.Look(ref fieldCenter, "fieldCenter");
            Scribe_References.Look(ref clockMote, "clockMote");
            Scribe_References.Look(ref state, "state");

            if (Scribe.mode == LoadSaveMode.PostLoadInit && fieldCenter.HasValue && pawn != null)
            {
                Fields.Add(pawn, fieldCenter.Value);
            }
        }
    }
}
