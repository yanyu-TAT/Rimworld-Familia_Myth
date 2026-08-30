using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    public abstract class Projectile_MagicMissileBase : Projectile
    {
        protected Spell spell;

        protected static HashSet<IntVec3> checkedCells = new();

        private const int FadeInTicks = 60;
        private const int FadeOutTicks = 60;
        private int fadeInTimer = -999;
        private int fadeOutTimer = -999;

        protected static MaterialPropertyBlock matPropertyBlock;

        private int CurrentTick => Find.TickManager.TicksGame;

        private int pulseOffset;

        public BaseMagicMissileState currentState;

        private static FleckDef trailDef;
        private static FleckDef TrailDef =>
            trailDef ??= DefDatabase<FleckDef>.GetNamed("MagicMissileTrail");

        private Vector3 prevPosition;

        private bool hasPrevPosition;

        protected static Dictionary<string, Color> cachedEdgeColors = new();

        /// <summary>
        /// 当前魔法弹的贴图边缘的平均颜色
        /// </summary>
        protected Color EdgeColor
        {
            get
            {
                var path = def.graphicData.texPath;
                if (cachedEdgeColors.TryGetValue(path, out var color))
                    return color;

                var filePath = Path.Combine(def.modContentPack.RootDir, "Textures", path + ".png");
                if (!File.Exists(filePath))
                {
                    Log.Warning($"[FamiliaMyth]Texture file not found!: {filePath}");
                    return Color.white;
                }

                var tex = new Texture2D(2, 2);
                ImageConversion.LoadImage(tex, File.ReadAllBytes(filePath));

                var colors = GetColorList(tex);
                var avgColor = colors.Count > 0
                    ? new Color(
                        colors.Sum(c => c.r) / colors.Count,
                        colors.Sum(c => c.g) / colors.Count,
                        colors.Sum(c => c.b) / colors.Count,
                        1)
                    : Color.white;

                //销毁
                UnityEngine.Object.Destroy(tex);
                cachedEdgeColors[path] = avgColor;
                return avgColor;
            }
        }

        /// <summary>
        /// 获取贴图最边缘的像素的颜色列表
        /// </summary>
        /// <param name="tex"></param>
        /// <returns></returns>
        private List<Color> GetColorList(Texture2D tex)
        {
            int w = tex.width, h = tex.height;
            var pixels = tex.GetPixels();

            var isTransparent = new bool[w, h];
            Vector2Int start = new(-1, -1);
            //找出最左上不透明像素
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    isTransparent[x, y] = pixels[y * w + x].a <= 0.01f;

            for (int y = 0; y < h && start.x == -1; y++)
                for (int x = 0; x < w && start.x == -1; x++)
                {
                    if (isTransparent[x, y]) continue;
                    // 检查8邻域是否有透明像素
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            if (dx == 0 && dy == 0) continue;
                            int nx = x + dx, ny = y + dy;
                            if (nx < 0 || nx >= w || ny < 0 || ny >= h || isTransparent[nx, ny])
                            {
                                start = new Vector2Int(x, y);
                                break;
                            }
                        }
                        if (start.x != -1) break;
                    }
                }

            List<Color> result = new();
            if (start.x == -1) return result;

            //顺时针遍历
            int[] dxx = { 1, 1, 0, -1, -1, -1, 0, 1 };
            int[] dyy = { 0, -1, -1, -1, 0, 1, 1, 1 };

            //从起始点左侧进入
            Vector2Int prevP = new(start.x - 1, start.y);
            if (prevP.x >= 0 && prevP.x < w && !isTransparent[prevP.x, prevP.y])
            {
                // start 左侧不是背景，需要从 start 的 8 邻域中找一个透明的作为 prevP
                bool foundBg = false;
                for (int i = 0; i < 8; i++)
                {
                    int nx = start.x + dxx[i];
                    int ny = start.y + dyy[i];
                    if (nx < 0 || nx >= w || ny < 0 || ny >= h || isTransparent[nx, ny])
                    {
                        prevP = new Vector2Int(nx, ny);
                        foundBg = true;
                        break;
                    }
                }
                if (!foundBg) return result; // 孤立点
            }

            Vector2Int curP = start;
            result.Add(pixels[curP.y * w + curP.x]);

            while (true)
            {
                //上个像素索引
                var lastDir = -1;
                for (int i = 0; i < 8; i++)
                    if (curP + new Vector2Int(dxx[i], dyy[i]) == prevP)
                    {
                        lastDir = i;
                        break;
                    }

                //顺时针搜索不透明像素
                var startDir = (lastDir + 1) % 8;
                var foundDir = -1;
                for (int i = 0; i < 8; i++)
                {
                    var dir = (startDir + i) % 8;
                    var next = curP + new Vector2Int(dxx[dir], dyy[dir]);
                    if (next.x >= 0 && next.x < w && next.y >= 0 && next.y < h && !isTransparent[next.x, next.y])
                    {
                        foundDir = dir;
                        break;
                    }
                }

                //孤立点
                if (foundDir == -1) break;

                var nextP = curP + new Vector2Int(dxx[foundDir], dyy[foundDir]);
                result.Add(pixels[curP.y * w + curP.x]);

                //轮廓闭合
                if (nextP == start) break;

                prevP = curP;
                curP = nextP;
            }

            return result;
        }

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
            fadeInTimer = FadeInTicks;
            pulseOffset = Rand.Int % 60; //随机闪烁

            //仅占位
            destination = origin = spell.pawn.Position.ToVector3Shifted();
        }

        /// <summary>
        /// 获取对应信号的方法委托
        /// </summary>
        /// <param name="message"></param>
        public virtual Action NotifyBy(string message)
        {
            return null;
        }

        protected override void TickInterval(int delta)
        {
            currentState.UpdateInterval(delta);
            var cur = ExactPosition;

            //首帧初始化
            if (!hasPrevPosition)
            {
                prevPosition = cur;
                hasPrevPosition = true;
            }
            var prev = prevPosition;

            //地图外销毁
            if (!cur.InBounds(Map))
            {
                Position = prev.ToIntVec3();
                Destroy();
                return;
            }

            if (NeedTrail)
            {
                //沿 prev→cur 移动线段生成流线曳光（等距补点，保证连续）
                var moveDir = cur - prev;
                var dist = moveDir.magnitude;
                if (dist > 1e-5f)
                {
                    var dir = moveDir / dist;
                    const float step = 0.22f;
                    for (float d = 0f; d < dist; d += step)
                    {
                        var pos = prev + dir * d;
                        var data = FleckMaker.GetDataStatic(pos, Map, TrailDef, 1f);
                        data.velocity = dir * 0.6f; //沿轨迹方向小速度，形成流线
                        data.instanceColor = EdgeColor.WithAlpha(0.85f);
                        Map.flecks.CreateFleck(data);
                    }
                }
            }

            prevPosition = cur;

            //淡入计时
            if (fadeInTimer > 0)
            {
                fadeInTimer -= delta;
                if (fadeInTimer < 0) fadeInTimer = 0;
            }

            //淡出计时
            if (fadeOutTimer > 0)
            {
                fadeOutTimer -= delta;
                if (fadeOutTimer <= 0)
                {
                    Destroy();
                    return;
                }
            }

            //位置未改变，返回
            if (cur == prev) return;

            //判定碰撞
            if (CheckHitLine(prev, cur))
                return;

            //移动
            Position = cur.ToIntVec3();
        }

        public void FadeOut()
        {
            if (fadeOutTimer < 0)
                fadeOutTimer = FadeOutTicks;
        }

        public override Color DrawColor
        {
            get
            {
                var color = base.DrawColor;

                //淡入效果
                if (fadeInTimer > 0)
                    color = color.WithAlpha(Mathf.Clamp01(1f - (float)fadeInTimer / FadeInTicks));
                //淡出效果
                if (fadeOutTimer > 0)
                    color = color.WithAlpha(Mathf.Clamp01((float)fadeOutTimer / FadeOutTicks));

                //闪烁效果
                var k = 0.9f + 0.1f * Mathf.Sin((CurrentTick + pulseOffset) * 0.05f);
                color *= k;

                return color;
            }
        }

        /// <summary>
        /// 覆写绘制方法，使用MaterialPropertyBlock设置颜色
        /// </summary>
        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            matPropertyBlock ??= new MaterialPropertyBlock();
            matPropertyBlock.SetColor(ShaderPropertyIDs.Color, DrawColor);

            var mesh = MeshPool.GridPlane(def.graphicData.drawSize);
            Graphics.DrawMesh(mesh, drawLoc, ExactRotation, DrawMat, 0, null, 0, matPropertyBlock, false, false, false);
            Comps_PostDraw();
        }

        public override int UpdateRateTicks => 1;

        public override Vector3 ExactPosition => currentState.ExactPosition + Vector3.up * def.Altitude;

        public override Quaternion ExactRotation
        {
            get
            {
                var dir = currentState.Velocity;
                if (dir.magnitude < 1e-5f) dir = currentState.Acceleration;
                if (dir.magnitude < 1e-5f) dir = Vector3.forward;
                return Quaternion.LookRotation(dir);
            }
        }

        public virtual bool CanShieldBlock => true;

        protected virtual bool NeedTrail => true;

        protected bool CheckHitLine(Vector3 A, Vector3 B)
        {
            //坐标不变不检测
            if (A == B) return false;

            //检测护盾拦截
            if (CanShieldBlock)
            {
                var list = Map.listerThings.ThingsInGroup(ThingRequestGroup.ProjectileInterceptor).ToList();
                foreach (var thing in list)
                    if (thing.TryGetComp<CompProjectileInterceptor>().CheckIntercept(this, A, B))
                    {
                        Impact(null, true);
                        return true;
                    }
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
            checkedCells.Clear(); //清空检测表
            do 
            { 
                z += step;  //前进
                cell = z.ToIntVec3();
                if (!checkedCells.Contains(cell))
                {
                    if (CheckHitCell(cell)) return true;
                    //防止重复检测
                    checkedCells.Add(cell);
                }
            } while (cell != b);

            return false;
        }

        protected bool CheckHitCell(IntVec3 cell)
        {
            var thingList = cell.GetThingList(Map).ToList();
            foreach(var thing in thingList)
            {
                if (CanHitAt(thing))
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

        protected virtual bool CanHitAt(Thing thing)
        {
            return currentState.CanHit && CanHit(thing);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref spell, "spell");
            Scribe_Deep.Look(ref currentState, "currentState");
        }
    }
}