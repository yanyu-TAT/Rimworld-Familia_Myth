using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class GameComponent_RoundhouseKick : GameComponent
    {
        public static GameComponent_RoundhouseKick Instance => Current.Game?.GetComponent<GameComponent_RoundhouseKick>();

        public GameComponent_RoundhouseKick(Game game) { }
        public GameComponent_RoundhouseKick() { }

        private class KnockedPawn : IExposable
        {
            public Pawn pawn;
            public Vector3 velocity;
            public Vector3 exactPos;
            public Verb verb;
            public int ticks;

            public void ExposeData()
            {
                Scribe_Values.Look(ref velocity, "velocity");
                Scribe_Values.Look(ref ticks, "ticks");
                Scribe_Values.Look(ref exactPos, "exactPos");
                Scribe_References.Look(ref pawn, "pawn");
                Scribe_References.Look(ref verb, "verb");
            }
        }

        private List<KnockedPawn> knockedPawns = new();

        public static void Kick(Pawn pawn, Verb verb, Vector3 dir)
        {
            if (Instance?.knockedPawns.Any(k => k.pawn == pawn) ?? false)
                return;

            var knocked = new KnockedPawn()
            {
                pawn = pawn,
                verb = verb,
                velocity = dir.normalized * (Skill_RoundhouseKick.KnockbackDist / Skill_RoundhouseKick.KnckbackTicks),
                exactPos = pawn.Position.ToVector3Shifted(),
                ticks = Skill_RoundhouseKick.KnckbackTicks
            };
            Instance?.knockedPawns.Add(knocked);
        }

        public override void GameComponentTick()
        {
            foreach (var knocked in knockedPawns.ToList())
            {
                var pawn = knocked.pawn;
                //对象消失/死亡/击退结束
                if (pawn?.Map == null || !pawn.Spawned || pawn.Dead || knocked.ticks <= 0)
                {
                    knockedPawns.Remove(knocked);
                    continue;
                }

                if (Flying(knocked))
                {
                    //更新位置
                    knocked.exactPos += knocked.velocity;
                    knocked.ticks--;
                    pawn.Position = knocked.exactPos.ToIntVec3();
                    pawn.Notify_Teleported(true, false);
                    pawn.stances?.stunner.StunFor(2, knocked.verb.CasterPawn, false, false, true);
                }
                else
                {
                    knockedPawns.Remove(knocked); 
                }
            }
        }

        private bool Flying(KnockedPawn knocked)
        {
            var next = (knocked.exactPos + knocked.velocity).ToIntVec3();
            var pawn = knocked.pawn;

            //位置未变化
            if (knocked.exactPos.ToIntVec3() == next) return true;

            //超出地图边界
            if (!next.InBounds(pawn.Map))
                return false;

            //碰到障碍物
            if (!next.Walkable(pawn.Map) || next.GetThingList(pawn.Map).Any(t => t != pawn && t.def.passability == Traversability.Impassable))
            {
                var verb = knocked.verb;
                var dinfo = new DamageInfo(
                    verb.GetDamageDef(),
                    verb.verbProps.AdjustedMeleeDamageAmount(verb, verb.CasterPawn) * Skill_RoundhouseKick.BlockBonus,
                    verb.verbProps.AdjustedArmorPenetration(verb, verb.CasterPawn),
                    (-knocked.velocity).ToAngleFlat(),
                    verb.CasterPawn);
                dinfo.SetTool(verb.tool);
                pawn.TakeDamage(dinfo);
                pawn.stances?.stagger?.StaggerFor(30);

                FleckMaker.ThrowSmoke(next.ToVector3(), pawn.Map, 1f);
                return false;
            }

            return true;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref knockedPawns, "knockedPawns", LookMode.Deep);
        }
    }
}
