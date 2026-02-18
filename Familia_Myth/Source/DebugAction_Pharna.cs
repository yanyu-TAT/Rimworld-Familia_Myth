using FamiliaMyth.Pharna;
using LudeonTK;
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
                        foreach (var pawn in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).OfType<Pawn>())
                        {
                            Hediff_Pharna.GetPharna(pawn)?.GainStatus(type, DefaultAmount);
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
    }
}