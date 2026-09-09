using System.Collections.Generic;
using System.Linq;
using FamiliaMyth.Familia;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Quest
{
    //日常任务「遗迹探索」根节点：统一点数 → 在殖民地附近(2~10 格)生成一座
    //原版 AncientComplex（复杂体：房间/威胁/房间奖励箱全套，同「文化仪式发现遗迹」类别）→ slate 参数。
    //站点生成完全复刻原版 QuestNode_Root_Loot_AncientComplex 的生成法。
    public class QuestNode_Root_Daily_AncientRelic : QuestNode
    {
        public const string RelicDefName = "FM_AncientRelicArtifact";

        public const string CrateOpenedSignal = "relicCrateOpened";      //任务机关箱被打开(=发现)
        public const string RaidArriveSignal = "relicRaidArrive";        //守家袭击(0.5×)
        public const string ShuttleCallSignal = "relicShuttleCall";      //召唤公会穿梭机(3~5 天)

        //原版 threat 曲线与尺寸曲线（复刻自 QuestNode_Root_AncientComplex）
        private static readonly SimpleCurve ThreatPointsOverPoints = new SimpleCurve
        {
            new CurvePoint(35f, 20),
            new CurvePoint(400f, 100f),
            new CurvePoint(10000f, 4000f)
        };

        private static readonly SimpleCurve ComplexSizeOverPoints = new SimpleCurve
        {
            new CurvePoint(0f, 30f),
            new CurvePoint(10000f, 50f)
        };

        protected override void RunInt()
        {
            if (!ModsConfig.IdeologyActive)
                return;

            var slate = QuestGen.slate;
            var quest = QuestGen.quest;

            QuestUtilities.CalculateTaskParams(isRankUp: false, out float points, out float baseExploits);

            var sitePartDef = DefDatabase<SitePartDef>.GetNamedSilentFail("AncientComplex");
            var layoutDef = DefDatabase<LayoutDef>.GetNamedSilentFail("AncientComplex_Loot");
            var rewardMaker = DefDatabase<ThingSetMakerDef>.GetNamedSilentFail("MapGen_AncientComplexRoomLoot_Better");
            var relicDef = DefDatabase<ThingDef>.GetNamedSilentFail(RelicDefName);
            if (sitePartDef == null || layoutDef == null || rewardMaker == null || relicDef == null)
                return;

            //1. 殖民地附近选址（2~10 格）
            if (!TileFinder.TryFindNewSiteTile(out PlanetTile siteTile, 2, 10,
                    allowCaravans: false, null, 0.5f, canSelectComboLandmarks: true,
                    TileFinderMode.Near))
                return;

            //2. 生成复杂体布局图纸（AncientComplex_Loot 房间体系；loot 版不带 hack 终端）
            int size = Mathf.RoundToInt(ComplexSizeOverPoints.Evaluate(points));
            var sketch = layoutDef.Worker.GenerateStructureSketch(
                new StructureGenParams { size = new IntVec2(size, size) });

            //3. 装配并生成 site（完整原版威胁 + 房间奖励箱），属主 Faction.OfAncients
            float threatPoints = ThreatPointsOverPoints.Evaluate(points);
            var site = QuestGen_Sites.GenerateSite(
                Gen.YieldSingle(new SitePartDefWithParams(
                    parms: new SitePartParams
                    {
                        threatPoints = Find.Storyteller.difficulty.allowViolentQuests ? threatPoints : 0f,
                        ancientLayoutStructureSketch = sketch,
                        ancientComplexRewardMaker = rewardMaker
                    },
                    def: sitePartDef)),
                siteTile, Faction.OfAncients);
            if (site == null)
                return;
            quest.SpawnWorldObject(site);

            //4. 袭击方派系（守家 0.5× 袭击）与公会押送方
            var raidFaction = Find.FactionManager.RandomEnemyFaction(
                allowHidden: false, allowNonHumanlike: false, allowDefeated: false);
            if (raidFaction == null)
                return;

            Faction guild = Find.FactionManager.FirstFactionOfDef(GameComponent_FamiliaRankTracker.GuildDef);
            if (guild == null)
            {
                foreach (var f in Find.FactionManager.AllFactionsVisible)
                    if (!f.IsPlayer && !f.def.isPlayer
                        && f.RelationKindWith(Faction.OfPlayer) != FactionRelationKind.Hostile)
                    {
                        guild = f;
                        break;
                    }
            }

            //5. slate 参数（XML 与装配节点消费）
            slate.Set("difficulty", points);
            slate.Set("totalExploits", baseExploits);
            slate.Set("site", site);
            slate.Set("siteFaction", raidFaction);
            slate.Set("guildFaction", guild);
            //穿梭机交付所需的任务物品（一件圣物匣；IsRequired 按 def+count 匹配）
            slate.Set("requiredItems", new List<ThingDefCount> { new ThingDefCount(relicDef, 1) });
        }

        protected override bool TestRunInt(Slate slate)
        {
            if (!ModsConfig.IdeologyActive)
                return false;
            return DefDatabase<ThingDef>.GetNamedSilentFail(RelicDefName) != null
                   && TileFinder.TryFindNewSiteTile(out _, 2, 10, allowCaravans: false, null, 0.5f,
                       canSelectComboLandmarks: true, TileFinderMode.Near);
        }
    }
}
