using FamiliaMyth.Pharna;
using FamiliaMyth.UI;
using LudeonTK;
using RimWorld;
using Verse;

namespace FamiliaMyth.Debug
{
    public static class DebugAction_Pharna
    {
        private const float DefaultAmount = 100f;

        /// <summary>
        /// 调试指令：增加属性值
        /// </summary>
        [DebugAction("Familia Myth", "Pharna +100 Status",
            actionType = DebugActionType.Action,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void AddStatus()
        {
            var options = new List<DebugMenuOption>();
            foreach (PharnaStatusType type in Enum.GetValues(typeof(PharnaStatusType)))
            {
                options.Add(new DebugMenuOption(
                    Hediff_Pharna.GetStatusName(type),
                    DebugMenuOptionMode.Tool,
                    () =>
                    {
                        foreach (var pawn in Find.CurrentMap.thingGrid.ThingsAt(Verse.UI.MouseCell()).OfType<Pawn>())
                        {
                            var pharna = Hediff_Pharna.GetPharna(pawn);
                            pharna?.GainStatus(type, DefaultAmount);
                            MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, $"{Hediff_Pharna.GetStatusName(type)}:{pharna?.GetCurrentStatus(type)}");
                        }
                    }
                ));
            }

            Find.WindowStack.Add(new Dialog_DebugOptionListLister(options));
        }

        /// <summary>
        /// 调试指令：赋予恩惠
        /// </summary>
        /// <param name="pawn">选中的殖民者</param>
        [DebugAction("Familia Myth", "Grant Pharna",
            actionType = DebugActionType.ToolMapForPawns,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void GrantPharna(Pawn pawn)
        {
            if (Hediff_Pharna.GetPharna(pawn) != null) return;
            pawn.health.AddHediff(HediffDef.Named("Hediff_Pharna"));
        }

        /// <summary>
        /// 调试指令：增加伟业值
        /// </summary>
        /// <param name="pawn">选中的殖民者</param>
        [DebugAction("Familia Myth", "Pharna +100 Eploits",
            actionType = DebugActionType.ToolMapForPawns,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void AddExploits(Pawn pawn)
        {
            var pharna = Hediff_Pharna.GetPharna(pawn);
            pharna?.GainExploits(DefaultAmount);
            MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, $"{pharna?.exploits}");
        }

        /// <summary>
        /// 调试指令：添加升华奖励
        /// </summary>
        [DebugAction("Familia Myth", "Add Acsend Reward",
            actionType = DebugActionType.Action,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void AddAscendReward()
        {
            var options = new List<DebugMenuOption>();
            foreach (var (Factory, _) in AscendHelper.rewards)
            {
                var reward = Factory();
                reward.RewardInitialize();

                options.Add(new DebugMenuOption(
                    $"{reward.Label} [{reward.RewardType}]",
                    DebugMenuOptionMode.Tool,
                    () =>
                    {
                        foreach (var pawn in Find.CurrentMap.thingGrid.ThingsAt(Verse.UI.MouseCell()).OfType<Pawn>())
                        {
                            var pharna = Hediff_Pharna.GetPharna(pawn);
                            if (pharna == null) continue;
                            List<string> rewardNames = pharna.GetRewards().ToList();
                            var r = Factory();
                            if (!rewardNames.Contains(r.GetType().Name))
                            {
                                r.RewardInitialize();
                                r.Apply(pharna);
                            }
                        }
                    }
                ));
            }
            Find.WindowStack.Add(new Dialog_DebugOptionListLister(options));
        }

        /// <summary>
        /// 调试指令：显示升华奖励窗口
        /// </summary>
        /// <param name="pawn">选中的殖民者</param>
        [DebugAction("Familia Myth", "Show Ascend Window",
            actionType = DebugActionType.ToolMapForPawns,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void ShowAscendWindow(Pawn pawn)
        {
            var pharna = Hediff_Pharna.GetPharna(pawn); 
            if (pharna == null) return;
            var rewards = AscendHelper.GetRandomRewards(10).ToList();
            if (rewards.Count > 0)
            {
                foreach (var reward in rewards)
                    reward.RewardInitialize();
                Find.WindowStack.Add(new Dialog_AscendReward(pharna, rewards));
            }
        }

        [DebugAction("Familia Myth", "All Developable Ability Gain +100 Proficiency",
            actionType = DebugActionType.ToolMapForPawns,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void GainDevelopableAbilityProficiency(Pawn pawn)
        {
            var pharna = Hediff_Pharna.GetPharna(pawn);
            if (pharna?.developableAbilities == null) return;
            foreach (var ability in pharna.developableAbilities)
            {
                if (ability == null) continue;
                ability.GainProficiency(DefaultAmount);
                MoteMaker.ThrowText(pawn.Position.ToVector3(), pawn.Map, $"{ability.Proficiency}");
            }
        }
    }
}