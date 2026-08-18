using FamiliaMyth.Pharna;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;
using Random = UnityEngine.Random;

namespace FamiliaMyth.Quest
{
    public class QuestNode_DailySetDifficulty : QuestNode
    {
        public SlateRef<string> storeDifficultyAs;
        public SlateRef<string> storeExploitsAs;

        //发布的伟业值的基础值
        public float baseExploits = 800f;
        public float difficultyMultiplier = 1f;

        protected override void RunInt()
        {
            var slate = QuestGen.slate;

            float difficulty = CalculateDifficulty();

            //计算总伟业值
            float totalExploits = baseExploits * difficulty;
            float randomFactor = Random.Range(0.8f, 1.2f);
            totalExploits *= randomFactor;
            totalExploits = Mathf.Clamp(totalExploits, 10f, 500f);

            //站点威胁点数：基础威胁 × 难度系数 × 恩惠加成
            float threatPoints = StorytellerUtility.DefaultSiteThreatPointsNow() * difficulty * GetPharnaMultiplier();

            //构建站点部件参数
            var outpostDef = DefDatabase<SitePartDef>.GetNamed("Outpost");
            if (outpostDef != null)
            {
                var sitePartsParams = new List<SitePartDefWithParams>
                {
                    new(outpostDef, new SitePartParams { threatPoints = threatPoints })
                };
                slate.Set("sitePartsParams", sitePartsParams);
            }

            // 存储结果到Slate
            string difficultyVarName = storeDifficultyAs.GetValue(slate) ?? "difficulty";
            string exploitsVarName = storeExploitsAs.GetValue(slate) ?? "totalExploits";
            slate.Set(difficultyVarName, difficulty);
            slate.Set(exploitsVarName, totalExploits);
        }

        public float CalculateDifficulty()
        {
            Map map = Find.CurrentMap;
            if (map == null) return 1.0f;

            //获取财富值
            float wealth = map.PlayerWealthForStoryteller;

            //财富系数（参考原版曲线）
            float wealthFactor;
            if (wealth < 14000f) wealthFactor = 0.5f;
            else if (wealth < 400000f) wealthFactor = Mathf.Lerp(0.5f, 2.0f, (wealth - 14000f) / 386000f);
            else if (wealth < 1000000f) wealthFactor = Mathf.Lerp(2.0f, 3.0f, (wealth - 400000f) / 600000f);
            else wealthFactor = 3.0f;

            //综合难度
            float difficulty = wealthFactor * difficultyMultiplier;
            return Mathf.Clamp(difficulty, 0.5f, 5.0f);
        }

        //恩惠等级加成：越高等级的受恩惠冒险者越多，难度越高
        private float GetPharnaMultiplier()
        {
            Map map = Find.CurrentMap;
            if (map == null) return 1f;

            float m = 1f;
            foreach (var pawn in map.mapPawns.AllPawnsSpawned
                .Where(p => p.Faction == Faction.OfPlayer && p.IsColonist))
                if (pawn.GetPharna() is Hediff_Pharna pharna)
                    m += (pharna.level - 1) * (pharna.level - 1) / 10f;
            return m;
        }

        protected override bool TestRunInt(Slate slate) => true;
    }
}