using FamiliaMyth.Feat;
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
        //调试用：一次性展示全部奖励
        private const int DebugRewardCount = 100;

        private static IEnumerable<Thing> GetThingsAtMouse() => 
            Find.CurrentMap.thingGrid.ThingsAt(Verse.UI.MouseCell()).ToList();

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
                        foreach (var pawn in GetThingsAtMouse().OfType<Pawn>())
                        {
                            var pharna = pawn.GetPharna();
                            pharna?.GainStatus(type, DefaultAmount);
                            MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, $"{Hediff_Pharna.GetStatusName(type)}:{pharna?.GetCurrentStatus(type)}");
                        }
                    }));
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
            if (pawn.GetPharna() != null) return;
            pawn.health.AddHediff(HediffDef.Named("Hediff_Pharna"));
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
            foreach (var entry in AscendHelper.Entries)
            {
                var reward = AscendHelper.CreateReward(entry.Type);
                if (reward == null) continue;

                options.Add(new DebugMenuOption(
                    $"{reward.Label} [{reward.RewardType}]",
                    DebugMenuOptionMode.Tool,
                    () =>
                    {
                        foreach (var pawn in GetThingsAtMouse().OfType<Pawn>())
                        {
                            var pharna = pawn.GetPharna();
                            if (pharna == null) continue;

                            var r = AscendHelper.CreateReward(entry.Type);
                            if (r == null || pharna.HasReward(r)) continue;
                            r.Apply(pharna);
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
            var pharna = pawn.GetPharna(); 
            if (pharna == null) return;
            var session = new AscendRewardSession(pharna, DebugRewardCount, 0);
            if (session.Rewards.Count > 0)
                Find.WindowStack.Add(new Dialog_AscendReward(session));
        }

        [DebugAction("Familia Myth", "All Developable Ability Gain +100 Proficiency",
            actionType = DebugActionType.ToolMapForPawns,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void GainDevelopableAbilityProficiency(Pawn pawn)
        {
            var pharna = pawn.GetPharna();
            if (pharna?.developableAbilities == null) return;
            foreach (var ability in pharna.developableAbilities)
            {
                if (ability == null) continue;
                ability.GainProficiency(DefaultAmount);
                MoteMaker.ThrowText(pawn.Position.ToVector3(), pawn.Map, $"{ability.Proficiency}");
            }
        }

        [DebugAction("Familia Myth", "Trigger Virtue",
            actionType = DebugActionType.Action,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void TriggerVirtue()
        {
            var options = new List<DebugMenuOption>();
            foreach (VirtueType type in Enum.GetValues(typeof(VirtueType)))
            {
                options.Add(new DebugMenuOption(
                    type.ToString(),
                    DebugMenuOptionMode.Tool,
                    () =>
                    {
                        foreach (var pawn in GetThingsAtMouse().OfType<Pawn>())
                        {
                            var pharna = pawn.GetPharna();
                            if (pharna != null)
                            {
                                pharna.currentVirtue = type;
                                var def = DefDatabase<MentalStateDef>.GetNamed($"MentalState_Virtue_{type}");
                                pawn.mindState.mentalStateHandler.TryStartMentalState(def);
                            }
                        }
                    }));
            }

            Find.WindowStack.Add(new Dialog_DebugOptionListLister(options));
        }

        /// <summary>
        /// 调试指令：设置或清除铭刻
        /// </summary>
        [DebugAction("Familia Myth", "Set Inscription",
            actionType = DebugActionType.Action,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void SetInscription()
        {
            var options = new List<DebugMenuOption>();
            foreach (PharnaStatusType type in Enum.GetValues(typeof(PharnaStatusType)))
            {
                options.Add(new DebugMenuOption(
                    Hediff_Pharna.GetStatusName(type),
                    DebugMenuOptionMode.Tool,
                    () => ApplyInscription(type)));
            }
            options.Add(new DebugMenuOption("Clear", DebugMenuOptionMode.Tool, () => ApplyInscription(null)));

            Find.WindowStack.Add(new Dialog_DebugOptionListLister(options));
        }

        //先置可用再走统一入口，避免绕过次数校验
        private static void ApplyInscription(PharnaStatusType? type)
        {
            foreach (var pawn in GetThingsAtMouse().OfType<Pawn>())
            {
                var pharna = pawn.GetPharna();
                if (pharna == null) continue;

                pharna.inscriptionAvailable = true;
                pharna.TrySetInscription(type);
                MoteMaker.ThrowText(pawn.DrawPos, pawn.Map,
                    type.HasValue ? Hediff_Pharna.GetStatusName(type.Value) : "None");
            }
        }

        [DebugAction("Familia Myth", "Give Feat",
            actionType = DebugActionType.ToolMap,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void GiveFeat()
        {
            var options = new List<DebugMenuOption>();
            foreach(var pawn in GetThingsAtMouse().OfType<Pawn>())
            {
                var pharna = pawn.GetPharna();
                if (pharna == null) continue;

                var feat = new FeatRecord
                {
                    tier = 9
                };
                pharna.feats.Add(feat);
                MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, "Feat Added");
            }
        }
    }
}