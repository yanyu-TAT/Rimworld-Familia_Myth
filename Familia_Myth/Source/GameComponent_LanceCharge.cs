using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class GameComponent_LanceCharge : GameComponent
    {
        public static GameComponent_LanceCharge Instance => Current.Game?.GetComponent<GameComponent_LanceCharge>();

        public GameComponent_LanceCharge(Game game) { }
        public GameComponent_LanceCharge() { }

        private class MovingPawn : IExposable
        {
            public Pawn pawn;
            public Vector3 velocity;
            public Vector3 exactPos;
            public int ticks;

            public void ExposeData()
            {
                Scribe_Values.Look(ref velocity, "velocity");
                Scribe_Values.Look(ref ticks, "ticks");
                Scribe_Values.Look(ref exactPos, "exactPos");
                Scribe_References.Look(ref pawn, "pawn");
            }
        }

        private List<MovingPawn> movingPawns = new();

        public static void StartMoving(Pawn pawn, Vector3 dir, int ticks, float dist)
        {
            if (Instance?.movingPawns.Any(k => k.pawn == pawn) ?? false)
                return;

            var moving = new MovingPawn()
            {
                pawn = pawn,
                velocity = dir.normalized * (dist / ticks),
                exactPos = pawn.Position.ToVector3Shifted(),
                ticks = ticks
            };
            Instance?.movingPawns.Add(moving);
        }

        public override void GameComponentTick()
        {
            foreach (var moving in movingPawns.ToList())
            {
                var pawn = moving.pawn;
                //对象消失/死亡/击退结束
                if (pawn?.Map == null || !pawn.Spawned || pawn.Dead || moving.ticks <= 0)
                {
                    movingPawns.Remove(moving);
                    continue;
                }

                if (Flying(moving))
                {
                    //更新位置
                    moving.exactPos += moving.velocity;
                    moving.ticks--;
                    pawn.Position = moving.exactPos.ToIntVec3();
                    pawn.Notify_Teleported(true, false);
                    pawn.stances?.stunner.StunFor(2, null, false, false, true);
                }
                else
                {
                    movingPawns.Remove(moving); 
                }
            }
        }

        private bool Flying(MovingPawn knocked)
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
                pawn.stances?.stagger?.StaggerFor(30);
                FleckMaker.ThrowSmoke(next.ToVector3(), pawn.Map, 1f);
                return false;
            }

            return true;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref movingPawns, "movingPawns", LookMode.Deep);
        }
    }
}
