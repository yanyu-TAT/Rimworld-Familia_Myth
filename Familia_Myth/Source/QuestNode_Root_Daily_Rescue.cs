using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Quest
{
    //日常任务「紧急救援」根节点：统一点数 → 威胁 part(点数×0.5) → 遇难者 worldPawn → slate
    public class QuestNode_Root_Daily_Rescue : QuestNode
    {
        public const float ThreatFactor = 0.5f;      //威胁点数系数
        public const float VictimPerPoints = 1000f;  //遇难者人数映射
        public const int MaxVictims = 5;
        public const float AnomalyChance = 0.5f;

        protected override void RunInt()
        {
            var slate = QuestGen.slate;

            QuestUtilities.CalculateTaskParams(isRankUp: false, out float points);
            float threat = points * ThreatFactor;
            int count = Mathf.Clamp(1 + Mathf.FloorToInt(points / VictimPerPoints), 1, MaxVictims);

            //遇难者所属派系：随机中立/友好可见派系
            var friendly = Find.FactionManager.AllFactionsVisible
                .Where(f => !f.IsPlayer && !f.def.isPlayer
                    && f.RelationKindWith(Faction.OfPlayer) != FactionRelationKind.Hostile)
                .ToList();
            if (friendly.Count == 0) return;
            Faction rescueFaction = friendly.RandomElement();

            //遇难者：worldPawn（暂不 spawn，由 QuestPart_RescueSpawn 放置）
            var victims = new List<Pawn>();
            for (int i = 0; i < count; i++)
                victims.Add(PawnGenerator.GeneratePawn(
                    new PawnGenerationRequest(rescueFaction.RandomPawnKind(), rescueFaction)));

            //威胁 part：狂暴兽群；Anomaly 激活时概率切换腐化营地整套
            bool useAnomaly = ModsConfig.AnomalyActive && Rand.Chance(AnomalyChance)
                && DefDatabase<SitePartDef>.GetNamedSilentFail("DistressCall_Fleshbeasts") != null;
            string threatDefName = useAnomaly ? "DistressCall_Fleshbeasts" : "Manhunters";
            var partDef = DefDatabase<SitePartDef>.GetNamedSilentFail(threatDefName);
            if (partDef == null) return;

            //狂暴兽群需足够点数选出狂暴动物（候选 combatPower×2 ≤ threat 才可入选）；
            //早期低点数（threat=points×0.5 常 <70）会使站点拿不到 animalKind 而报错，
            //故把兽群威胁点抬升到"可生成兽群"的下限
            if (!useAnomaly)
                threat = MinManhunterThreat(threat);

            var sitePartsParams = new List<SitePartDefWithParams>
            {
                new(partDef, new SitePartParams { threatPoints = threat })
            };
            var enemy = Find.FactionManager.RandomEnemyFaction(
                allowHidden: false, allowNonHumanlike: true, allowDefeated: false);

            slate.Set("difficulty", points);
            slate.Set("sitePartsParams", sitePartsParams);
            slate.Set("siteFaction", enemy);
            slate.Set("rescueFaction", rescueFaction);
            slate.Set("rescueCount", count);
            slate.Set("victims", victims);
        }

        //把兽群威胁点抬升到足以选出狂暴动物的下限
        //（AggressiveAnimalIncidentUtility 要求至少一只候选 combatPower×2 ≤ threat，
        // 否则站点生成时拿不到 animalKind；用原版全局兜底路径 PlanetTile.Invalid 预检）
        private static float MinManhunterThreat(float threat)
        {
            float t = Mathf.Max(threat, 100f);
            while (t < 20000f)
            {
                if (AggressiveAnimalIncidentUtility.TryFindAggressiveAnimalKind(t, PlanetTile.Invalid, out _))
                    return t;
                t *= 1.5f;
            }
            return t;
        }

        protected override bool TestRunInt(Slate slate) => true;
    }
}
