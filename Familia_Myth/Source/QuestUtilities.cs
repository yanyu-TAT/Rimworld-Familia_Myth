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

        //伟业值奖励（固定量，不随难度点数变化）
        public const float DailyExploits = 500f;
        public const float RankUpExploits = 900f;

        //计算任务威胁点数与伟业值奖励
        public static void CalculateTaskParams(bool isRankUp, out float threatPoints, out float totalExploits)
        {
            float points = StorytellerUtility.DefaultSiteThreatPointsNow() * PharnaGrowthFactor();
            if (isRankUp)
                points *= RankUpMultiplier;

            threatPoints = points;
            totalExploits = isRankUp ? RankUpExploits : DailyExploits;
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

        //按参与者名单分配伟业值并发送结算信。
        //规则：acceptor 在名单且多人 → acceptor 得 50%，名单其余均分；否则名单全员均分（acceptor 可为 null）
        //参与者过滤（存活且有恩惠）在方法内进行；成功发放返回 true。
        public static bool DistributeExploits(List<Pawn> participants, Pawn acceptor,
            float totalExploits, string letterLabel, string letterHeader)
        {
            if (totalExploits <= 0f || participants == null) return false;

            var list = participants.Where(p => p != null && !p.Dead && p.GetPharna() != null).ToList();
            if (list.Count == 0) return false;

            bool acceptorPresent = acceptor != null && list.Contains(acceptor);
            List<(Pawn pawn, float amount)> gains = new();
            foreach (var p in list)
            {
                float share;
                if (acceptorPresent && list.Count > 1)
                    share = p == acceptor
                        ? totalExploits * 0.5f
                        : totalExploits * 0.5f / (list.Count - 1);
                else
                    share = totalExploits / list.Count;

                var pharna = p.GetPharna();
                var a = pharna?.GainExploits(share);
                if (a is float f && f > 0)
                    gains.Add((p, f));
            }
            if (gains.Count == 0) return false;

            TaggedString text = letterHeader;
            foreach (var (pawn, amount) in gains)
                text += "FamiliaMyth.QuestMisc.ExploitsGainedLine".Translate(
                    pawn.Name.ToStringShort, amount.ToString("F1"));

            Letter letter = acceptor != null
                ? LetterMaker.MakeLetter(letterLabel, text, LetterDefOf.PositiveEvent, acceptor)
                : LetterMaker.MakeLetter(letterLabel, text, LetterDefOf.PositiveEvent);
            Find.LetterStack.ReceiveLetter(letter);
            return true;
        }
    }
}
