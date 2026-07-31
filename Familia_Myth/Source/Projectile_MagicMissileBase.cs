using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    public abstract class Projectile_MagicMissileBase : Projectile
    {
        protected Spell spell;

        protected static HashSet<IntVec3> checkCells = new();

        public Spell Spell => spell;

        public BaseMagicMissileState currentState;

        /// <summary>
        /// 初始化，设置spell引用
        /// 必须先初始化再使用missile引用初始化状态
        /// </summary>
        /// <param name="spell"></param>
        public virtual void Init(Spell spell)
        {
            this.spell = spell;
            launcher = spell.pawn;
            HitFlags = ProjectileHitFlags.All;
        }

        /// <summary>
        /// 获取对应对应信号的方法委托
        /// </summary>
        /// <param name="message"></param>
        public virtual Action NotifyBy(string message)
        {
            return null;
        }

        protected override void TickInterval(int delta)
        {
            var prevPosition = ExactPosition;
            currentState.UpdateInterval(delta);

            //地图外销毁
            if (!ExactPosition.InBounds(Map))
            {
                Position = prevPosition.ToIntVec3();
                Destroy();
                return;
            }
            if (ExactPosition == prevPosition) return;

            //判定碰撞
            if (CheckHitLine(prevPosition, ExactPosition))
                return;

            //移动
            Position = ExactPosition.ToIntVec3();
        }

        public override Vector3 ExactPosition => currentState.ExactPosition + Vector3.up * def.Altitude;

        protected bool CheckHitLine(Vector3 A, Vector3 B)
        {
            //坐标不变不检测
            if (A == B) return false;

            //检测护盾拦截
            var list = Map.listerThings.ThingsInGroup(ThingRequestGroup.ProjectileInterceptor);
            foreach (var thing in list)
                if (thing.TryGetComp<CompProjectileInterceptor>().CheckIntercept(this, A, B))
                {
                    Impact(null, true);
                    return true;
                }

            var a = A.ToIntVec3(); var b = B.ToIntVec3();
            //不变仅检测当前格
            if (a == b) return CheckHitCell(b);
            //超出边缘不检测
            if (!a.InBounds(Map) || !b.InBounds(Map)) return false;
            //坐标格相邻则只检测目标格
            if (a.AdjacentToCardinal(b)) return CheckHitCell(b);

            //每0.2个单位距离判定一次
            var z = A;  //起点
            var d = B - A;  //总路程
            var step = d.normalized * 0.2f; //步长
            IntVec3 cell; //当前格
            checkCells.Clear(); //清空检测表
            do 
            { 
                z += step;  //前进
                cell = z.ToIntVec3();
                if (!checkCells.Contains(cell))
                {
                    if (CheckHitCell(cell)) return true;
                    //防止重复检测
                    checkCells.Add(cell);
                }
            } while (cell != b);

            return false;
        }

        protected bool CheckHitCell(IntVec3 cell)
        {
            var thingList = cell.GetThingList(Map);
            foreach(var thing in thingList)
            {
                if (CanHit(thing))
                {
                    //碰撞墙体
                    if (thing.def.Fillage == FillCategory.Full)
                    {
                        if (thing is not Building_Door door || !door.Open)
                        {
                            Impact(thing);
                            return true;
                        }
                    }

                    //碰撞敌对目标
                    if (thing is Pawn pawn)
                    {
                        if (launcher?.Faction != null&& pawn.Faction != null && launcher.Faction.HostileTo(pawn.Faction))
                        {
                            Impact(pawn);
                            return true;
                        }
                    }

                    //其余目标不做碰撞
                }
            }

            return false;
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            //Log.Message("Missile Destroyed");
            spell?.activeProjectiles?.Remove(this);
            base.Destroy(mode);
        }

        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            base.Impact(hitThing, blockedByShield);
        }

        protected virtual void ExtraImpact() { }

        protected new bool CanHit(Thing thing)
        {
            return currentState.CanHit && base.CanHit(thing);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref spell, "spell");
            Scribe_Deep.Look(ref currentState, "currentState");
        }
    }
}