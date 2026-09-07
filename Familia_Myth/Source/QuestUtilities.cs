using FamiliaMyth.Pharna;
using RimWorld;
using Verse;

namespace FamiliaMyth.Quest
{
    public static class QuestUtilities
    {
        //强制任务点数倍数
        public const float RankUpMultiplier = 1.5f;

        //恩惠成长因子：总等级/总经验对加成的贡献权重
        public const float LevelWeight = 1f / 80f;
        public const float ExpWeight = 1f / 20000f;

        //伟业值奖励系数：点数 × 该系数
        public const float ExploitsFactor = 0.08f;

        //计算任务威胁点数与伟业值奖励
        public static void CalculateTaskParams(bool isRankUp, out float threatPoints, out float totalExploits)
        {
            float points = StorytellerUtility.DefaultSiteThreatPointsNow() * PharnaGrowthFactor();
            if (isRankUp)
                points *= RankUpMultiplier;

            threatPoints = points;
            totalExploits = points * ExploitsFactor;
        }

        //全殖民地恩惠成长因子：随所有受恩惠殖民者的总等级与总经验平滑增长
        private static float PharnaGrowthFactor()
        {
            float levelSum = 0f;
            float expSum = 0f;

            //家园/任务地图上的殖民者
            foreach (var map in Find.Maps)
                foreach (var pawn in map.mapPawns.FreeColonists)
                    AddPharna(pawn, ref levelSum, ref expSum);

            //远征车队中的殖民者
            foreach (var caravan in Find.WorldObjects.Caravans)
                if (caravan.Faction == Faction.OfPlayer)
                    foreach (var pawn in caravan.PawnsListForReading)
                        AddPharna(pawn, ref levelSum, ref expSum);

            float bonus = levelSum * LevelWeight + expSum * ExpWeight;
            return 1f + bonus;
        }

        private static void AddPharna(Pawn pawn, ref float levelSum, ref float expSum)
        {
            if (pawn.GetPharna() is Hediff_Pharna pharna)
            {
                levelSum += pharna.level;
                expSum += pharna.experience;
            }
        }
    }
}
