using FamiliaMyth.Pharna;
using UnityEngine;
using Verse;

namespace FamiliaMyth.UI
{
    public class Dialog_AscendReward : Window
    {
        private readonly Hediff_Pharna pharna;
        private readonly List<IAscendReward> rewards;

        private Vector2 scrollPosition;
        private static readonly Vector2 ButtonSize = new(400f, 60f);
        private const float RowSpacing = 8f;
        private const float TitleHeight = 40f;

        public Dialog_AscendReward(Hediff_Pharna pharna, List<IAscendReward> rewards)
        {
            this.pharna = pharna;
            this.rewards = rewards;

            forcePause = true;          // 暂停游戏
            doCloseX = false;           // 不可手动关闭（必须做出选择）
            absorbInputAroundWindow = true; // 阻止点击窗口外区域
            closeOnAccept = false;
            closeOnCancel = false;
        }

        public override Vector2 InitialSize => new(460f, TitleHeight + (ButtonSize.y + RowSpacing) * rewards.Count + 60f);

        public override void DoWindowContents(Rect inRect)
        {
            // 标题
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.UpperCenter;
            Widgets.Label(inRect.TopPartPixels(TitleHeight), "选择一项升级奖励");
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;

            // 奖励列表
            Rect listRect = new(inRect.x, inRect.y + TitleHeight, inRect.width,
                (ButtonSize.y + RowSpacing) * rewards.Count);
            float curY = listRect.y;

            foreach (var reward in rewards)
            {
                Rect buttonRect = new(listRect.x, curY, ButtonSize.x, ButtonSize.y);

                if (Widgets.ButtonText(buttonRect, GetRewardLabel(reward)))
                {
                    reward.RewardInitialize();
                    reward.Apply(pharna);
                    Close();
                }

                // 描述文字（自定义绘制在按钮下方）
                Rect descRect = new(buttonRect.x + 10f, buttonRect.y + 35f,
                    buttonRect.width - 20f, 20f);
                GUI.color = Color.grey;
                Widgets.Label(descRect, reward.Description);
                GUI.color = Color.white;

                curY += ButtonSize.y + RowSpacing;
            }
        }

        private static string GetRewardLabel(IAscendReward reward)
        {
            string typeLabel = reward.RewardType switch
            {
                RewardType.Spell => "魔法",
                RewardType.Skill => "技能",
                RewardType.DevelopableAbility => "发展能力",
                _ => "?"
            };

            string rarityTag = reward.Rarity switch
            {
                RewardRarity.Unique => "★独有",
                RewardRarity.Rare => "★特殊",
                RewardRarity.Uncommon => "★稀有",
                RewardRarity.Common => "★通常",
                _ => ""
            };

            return $"[{typeLabel}] {reward.Label}  {rarityTag}";
        }
    }
}
