using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Quest
{
    //「物资押运」静态工具：定居点对筛选（A/B）、押运清单生成（源自 A 的交易清单）、清单校验与扣除
    public static class SupplyEscortUtility
    {
        //清单总重目标范围（生成清单时随机取值并凑重）
        public const float CargoMassMin = 300f;
        public const float CargoMassMax = 600f;

        //贵重上限：按 def 单价判（不看整堆总值）
        public const float MaxUnitMarketValue = 100f;

        //三角形约束：A/B 距玩家据点、A 与 B 之间任一边的最大格数
        public const int MaxDistanceToHome = 8;

        //可参与凑重的最小单件重量（排除近乎无重的杂物）
        public const float MinUnitMass = 0.01f;

        //A 的清单单轮生成不足时反复生成的轮数上限（保证能凑满 CargoMass）
        public const int MaxStockRolls = 20;

        //凑重时的收尾容差（视为已凑满）
        private const float MassEpsilon = 0.5f;

        //==================== 定居点对筛选 ====================

        //按「中立/友好派系 + Visitable + 距据点 ≤ MaxDistanceToHome」筛出候选定居点
        public static List<Settlement> CandidateSettlements(Map homeMap)
        {
            var result = new List<Settlement>();
            if (homeMap == null || !homeMap.Tile.Valid) return result;

            foreach (var s in Find.WorldObjects.Settlements)
            {
                if (s == null || s.Destroyed) continue;
                if (!IsUsableSettlement(s, homeMap)) continue;
                result.Add(s);
            }
            return result;
        }

        //随机取一组满足全部约束的 A/B（A=发放点、B=交付点）
        public static bool TryFindSettlementPair(Map homeMap, out Settlement a, out Settlement b)
        {
            a = null;
            b = null;

            var candidates = CandidateSettlements(homeMap);
            if (candidates.Count < 2) return false;

            var shuffled = candidates.InRandomOrder().ToList();
            foreach (var sa in shuffled)
            {
                foreach (var sb in shuffled)
                {
                    if (sa == sb) continue;
                    if (ApproxDistance(sa.Tile, sb.Tile) > MaxDistanceToHome) continue;

                    a = sa;
                    b = sb;
                    return true;
                }
            }
            return false;
        }

        private static bool IsUsableSettlement(Settlement s, Map homeMap)
        {
            //Visitable：非玩家、非敌对、非太空层
            if (!s.Visitable) return false;

            var faction = s.Faction;
            if (faction == null || faction.IsPlayer || faction == Faction.OfPlayer) return false;
            if (faction.HostileTo(Faction.OfPlayer)) return false;   //中立/友好

            if (ApproxDistance(s.Tile, homeMap.Tile) > MaxDistanceToHome) return false;

            //不与已有任务抢点（贸易请求 / 本任务自身）
            return !InUseByOtherQuest(s);
        }

        private static bool InUseByOtherQuest(Settlement s)
        {
            if (s.GetComponent<TradeRequestComp>()?.ActiveRequest == true) return true;

            foreach (var quest in Find.QuestManager.QuestsListForReading)
            {
                if (quest.Historical) continue;
                foreach (var part in quest.PartsListForReading)
                {
                    if (part is QuestPart_SupplyEscort escort
                        && (escort.settlementA == s || escort.settlementB == s))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static float ApproxDistance(PlanetTile from, PlanetTile to) =>
            Find.WorldGrid.ApproxDistanceInTiles(from, to);

        //==================== 押运清单生成 ====================

        //从 A 的交易清单现生成一批物资，凑足 totalMass，返回 def+count 清单（同 def 合并）
        public static List<ThingDefCountClass> BuildManifest(Settlement from, float totalMass)
        {
            var traderKind = from?.TraderKind;
            if (traderKind == null || traderKind.stockGenerators.NullOrEmpty()) return null;

            var counts = new Dictionary<ThingDef, int>();
            float remaining = totalMass;

            for (int roll = 0; roll < MaxStockRolls && remaining > MassEpsilon; roll++)
            {
                var pool = BuildPool(from, traderKind);
                if (pool.Count == 0) break;

                foreach (var candidate in pool.InRandomOrder())
                {
                    if (remaining <= MassEpsilon) break;

                    float unit = UnitMass(candidate);
                    if (unit < MinUnitMass) continue;

                    int need = Mathf.CeilToInt(remaining / unit);
                    int take = Mathf.Min(need, candidate.stackCount);
                    if (take <= 0) continue;

                    counts.TryGetValue(candidate.def, out int prev);
                    counts[candidate.def] = prev + take;
                    remaining -= unit * take;
                }
            }

            if (counts.Count == 0) return null;

            return counts.Select(kv => new ThingDefCountClass(kv.Key, kv.Value)).ToList();
        }

        private static List<Thing> BuildPool(Settlement from, TraderKindDef traderKind)
        {
            var pool = new List<Thing>();
            foreach (var generator in traderKind.stockGenerators)
            {
                if (generator == null) continue;
                foreach (var thing in generator.GenerateThings(from.Tile, from.Faction))
                {
                    if (IsEligibleCargo(thing)) pool.Add(thing);
                }
            }
            return pool;
        }

        //单个实例是否可入清单（def 合规 + 单件有重量）
        public static bool IsEligibleCargo(Thing thing) =>
            thing != null && IsEligibleDef(thing.def) && UnitMass(thing) >= MinUnitMass;

        //清单 def 合规性：交易清单可卖 + 非贵重 + 不腐烂 + 非建筑/块堆 + 无需材质/不带品质
        //（后两条保证「def + 数量」足以唯一描述清单项）
        public static bool IsEligibleDef(ThingDef def)
        {
            if (def == null) return false;
            if (def.category != ThingCategory.Item) return false;
            if (def.building != null) return false;                   //建筑
            if (!def.tradeability.TraderCanSell()) return false;      //块堆等不可交易物
            if (def.BaseMarketValue > MaxUnitMarketValue) return false;
            if (IsRottable(def)) return false;
            if (def.MadeFromStuff) return false;
            if (def.HasComp(typeof(CompQuality))) return false;
            if (def.stackLimit <= 0) return false;

            return true;
        }

        private static bool IsRottable(ThingDef def)
        {
            if (def.comps == null) return false;
            foreach (var comp in def.comps)
            {
                if (comp is CompProperties_Rottable) return true;
            }
            return false;
        }

        private static float UnitMass(Thing thing) => thing.GetStatValue(StatDefOf.Mass);

        //本次任务的清单总重目标（300~600kg 随机）
        public static float RandomCargoMass() => Rand.Range(CargoMassMin, CargoMassMax);

        //A 的交易清单异常时的兜底清单（等重白银）
        public static List<ThingDefCountClass> MakeSilverFallback(float totalMass)
        {
            float unit = ThingDefOf.Silver.GetStatValueAbstract(StatDefOf.Mass);
            int count = unit > 0f ? Mathf.CeilToInt(totalMass / unit) : 1;
            return new List<ThingDefCountClass> { new(ThingDefOf.Silver, count) };
        }

        //清单总重（超重判断与信件展示用）
        public static float ManifestMass(List<ThingDefCountClass> manifest)
        {
            if (manifest.NullOrEmpty()) return 0f;

            float sum = 0f;
            foreach (var req in manifest)
            {
                if (req?.thingDef == null || req.count <= 0) continue;
                sum += req.thingDef.GetStatValueAbstract(StatDefOf.Mass) * req.count;
            }
            return sum;
        }

        //按清单发放（按 stackLimit 拆堆）
        public static void GiveManifest(Caravan caravan, List<ThingDefCountClass> manifest)
        {
            if (caravan == null || manifest.NullOrEmpty()) return;

            foreach (var req in manifest)
            {
                if (req?.thingDef == null || req.count <= 0) continue;

                int stackLimit = Mathf.Max(1, req.thingDef.stackLimit);
                int left = req.count;
                while (left > 0)
                {
                    var thing = ThingMaker.MakeThing(req.thingDef);
                    thing.stackCount = Mathf.Min(left, stackLimit);
                    left -= thing.stackCount;
                    CaravanInventoryUtility.GiveThing(caravan, thing);
                }
            }
        }

        //==================== 清单校验与扣除 ====================

        //远行队背包中该 def 的可用总数（不含身上装备/武器）
        public static int CountInCaravan(Caravan caravan, ThingDef def)
        {
            if (caravan == null || def == null) return 0;

            int sum = 0;
            foreach (var thing in CaravanInventoryUtility.AllInventoryItems(caravan))
            {
                if (thing.Destroyed || thing.def != def) continue;
                sum += thing.stackCount;
            }
            return sum;
        }

        //深拷贝清单（待交清单与原清单各自独立）
        public static List<ThingDefCountClass> CloneManifest(List<ThingDefCountClass> manifest) =>
            manifest?.Where(m => m?.thingDef != null && m.count > 0)
                .Select(m => new ThingDefCountClass(m.thingDef, m.count))
                .ToList();

        //部分交付：对「待交清单」逐项扣除背包中现有数量（不要求齐全），返回本次实际交付项
        //全部项都交不出时返回 null
        public static List<ThingDefCountClass> TakePartial(Caravan caravan, List<ThingDefCountClass> pending)
        {
            if (caravan == null || pending.NullOrEmpty()) return null;

            List<ThingDefCountClass> delivered = null;
            foreach (var req in pending)
            {
                if (req?.thingDef == null || req.count <= 0) continue;

                int have = CountInCaravan(caravan, req.thingDef);
                int take = Mathf.Min(have, req.count);
                if (take <= 0) continue;
                if (!TakeFromCaravan(caravan, req.thingDef, take)) continue;

                delivered ??= new List<ThingDefCountClass>();
                delivered.Add(new ThingDefCountClass(req.thingDef, take));
            }
            return delivered;
        }

        //从待交清单中减去已交付部分（归零项移除）
        public static void SubtractPending(List<ThingDefCountClass> pending, List<ThingDefCountClass> delivered)
        {
            if (pending.NullOrEmpty() || delivered.NullOrEmpty()) return;

            foreach (var item in delivered)
            {
                if (item?.thingDef == null || item.count <= 0) continue;

                var req = pending.FirstOrDefault(p => p?.thingDef == item.thingDef);
                if (req == null) continue;
                req.count -= item.count;
            }
            pending.RemoveAll(p => p == null || p.count <= 0);
        }

        //从远行队背包扣除指定 def 的指定数量（跨多堆，其余物品一律不动）
        private static bool TakeFromCaravan(Caravan caravan, ThingDef def, int count)
        {
            int left = count;
            foreach (var thing in CaravanInventoryUtility.AllInventoryItems(caravan))
            {
                if (left <= 0) break;
                if (thing.Destroyed || thing.def != def) continue;

                int take = Mathf.Min(left, thing.stackCount);
                if (take >= thing.stackCount)
                    thing.Destroy(DestroyMode.Vanish);
                else
                    thing.SplitOff(take).Destroy(DestroyMode.Vanish);

                left -= take;
            }
            return left <= 0;
        }

        //清单摘要（信件与按钮提示用）
        public static string ManifestSummary(List<ThingDefCountClass> manifest)
        {
            if (manifest.NullOrEmpty()) return null;

            return string.Join("、", manifest
                .Where(m => m?.thingDef != null && m.count > 0)
                .Select(m => $"{m.thingDef.label} ×{m.count}"));
        }
    }
}
