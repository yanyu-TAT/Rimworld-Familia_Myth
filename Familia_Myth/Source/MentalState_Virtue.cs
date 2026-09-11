using FamiliaMyth.Core;
using RimWorld;
using Verse;
using Verse.AI;

namespace FamiliaMyth.Pharna
{
    public class MentalState_Virtue : MentalState
    {
        public static ThingDef moteDef;
        public static ThingDef MoteDef =>
            moteDef ??= DefDatabase<ThingDef>.GetNamed("Mote_VirtueHalo");

        public VirtueType type;
        public Hediff buff;
        public Mote_Floating mote;

        public override void PostStart(string reason)
        {
            //设定持续时间
            int duration = Rand.Range(GenDate.TicksPerDay, GenDate.TicksPerDay * 2);
            forceRecoverAfterTicks = duration;

            //获取美德种类并设定
            var pharna = pawn.GetPharna();
            if (pharna == null)
            {
                Log.Warning($"Pawn {pawn.Name.ToStringShort} Hasn't Got Pharna But Triggered Virtue");
                return;
            }
            if (pharna.currentVirtue.HasValue){
                type = pharna.currentVirtue.Value;
            }
            else
            {
                Log.Warning($"Pawn {pawn.Name.ToStringShort} Triggered A Non-exist Virtue");
                return;
            }

            //添加buff
            var hediffDef = DefDatabase<HediffDef>.GetNamed($"Hediff_VirtueBuff_{type}");
            if (hediffDef == null)
            {
                Log.Error($"Can't Get Buff Hediff For Virtue {type}");
                return;
            }
            buff = pawn.health.AddHediff(hediffDef);
            if (buff == null)
            {
                Log.Error($"Can't Add Buff Hediff To Pawn {pawn.Name.ToStringShort}");
                return;
            }

            //添加光环特效
            mote = (Mote_Floating)ThingMaker.MakeThing(MoteDef);
            mote.Attach(pawn);
            GenSpawn.Spawn(mote, pawn.Position, pawn.Map);

            //美德触发时的飘字，二者择一
            if (pawn.Spawned && pawn.Map != null)
            {
                string voiceKey = Rand.Bool ? "FamiliaMyth.Virtue.Whisper." : "FamiliaMyth.Virtue.Prayer.";
                MoteMaker.ThrowText(pawn.DrawPos, pawn.Map,
                    TranslateHelper.Translate(voiceKey + type, ("god", FamiliaMythMod.GodAddress)).Resolve());
            }
        }

        public override void PostEnd()
        {
            //设置冷却
            var pharna = pawn.GetPharna();
            if (pharna == null)
            {
                Log.Warning($"Pawn {pawn.Name.ToStringShort} Hasn't Got Pharna But Triggered Virtue");
                return;
            }

            //移除buff
            if (pawn?.health != null && buff != null)
                pawn.health.RemoveHediff(buff);
            pharna.SetVirtueCooldown();

            //移除光环特效
            mote?.Destroy();
        }

        public override RandomSocialMode SocialModeMax() => RandomSocialMode.SuperActive;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref type, "type");
            Scribe_References.Look(ref buff, "buff");
            Scribe_References.Look(ref mote, "mote");
        }
    }

    public enum VirtueType
    {
        Stalwart,
        Courageous,
        Focused,
        Powerful,
        Vigorous
    }
}
