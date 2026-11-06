using System.Collections.Generic;
using Verse;

namespace FamiliaMyth.Pharna
{
    /// <summary>
    /// 存档级的升华奖励收集记录：记录在升华窗口中展示过的奖励，读档保留
    /// </summary>
    public class GameComponent_AscendRewardCollection : GameComponent
    {
        private List<string> collectedDefNames = new();

        public static GameComponent_AscendRewardCollection Instance =>
            Current.Game?.GetComponent<GameComponent_AscendRewardCollection>();

        public GameComponent_AscendRewardCollection() { }

        public GameComponent_AscendRewardCollection(Game game) { }

        public bool IsCollected(string defName)
            => !defName.NullOrEmpty() && collectedDefNames != null && collectedDefNames.Contains(defName);

        public int CollectedCount => collectedDefNames?.Count ?? 0;

        /// <summary>
        /// 记录本次展示过的奖励，已在册的跳过
        /// </summary>
        public void MarkSeen(IEnumerable<IAscendReward> rewards)
        {
            if (rewards == null) return;
            if (collectedDefNames == null) collectedDefNames = new List<string>();

            foreach (IAscendReward reward in rewards)
            {
                string defName = reward?.DefName;
                if (defName.NullOrEmpty() || collectedDefNames.Contains(defName)) continue;
                collectedDefNames.Add(defName);
            }
        }

        public void ClearAll() => collectedDefNames?.Clear();

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref collectedDefNames, "collectedDefNames", LookMode.Value);
            if (collectedDefNames == null) collectedDefNames = new List<string>();
        }
    }
}
