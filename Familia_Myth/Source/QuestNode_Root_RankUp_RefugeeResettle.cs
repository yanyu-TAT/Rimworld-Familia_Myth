using FamiliaMyth.Familia;
using RimWorld;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Quest
{
    //强制任务「难民安置」根节点：统一点数 → 生成临时难民派系与袭击派系 → 生成难民 → 写入 slate。
    //到访、lodger 标记、劝加入与主状态机的装配在 QuestNode_RefugeeResettleSetup（需排在 util 之后）。
    public class QuestNode_Root_RankUp_RefugeeResettle : QuestNode
    {
        //难民人数映射：1 + 点数 / 该系数，上限 MaxRefugees
        public const float RefugeePerPoints = 1000f;
        public const int MaxRefugees = 10;

        //袭击点数相对任务点数的系数
        public const float RaidPointsFactor = 1.2f;

        //延迟回报子任务：概率与「每人每天」市场价值系数（对齐原版难民延迟回报的算法结构）
        public const float RewardSubQuestChance = 0.5f;
        public const float RewardValuePerRefugeeDay = 25f;

        public SlateRef<FamiliaCategoryDef> category;

        protected override void RunInt()
        {
            var slate = QuestGen.slate;

            QuestUtilities.CalculateTaskParams(isRankUp: true, out float points);

            var map = slate.Get<Map>("map") ?? Find.AnyPlayerHomeMap ?? Find.CurrentMap;
            if (map == null || !map.IsPlayerHome) return;

            var refugeeFaction = CreateRefugeeFaction();
            if (refugeeFaction == null) return;

            //袭击者：生成期选定（敌对派系），并与难民派系设为敌对——叙事上即"追杀难民的人"
            var raider = Find.FactionManager.RandomEnemyFaction(
                allowHidden: false, allowNonHumanlike: true, allowDefeated: false);
            if (raider != null && raider != refugeeFaction)
                raider.SetRelationDirect(refugeeFaction, FactionRelationKind.Hostile, canSendHostilityLetter: false);

            int count = Mathf.Clamp(1 + Mathf.FloorToInt(points / RefugeePerPoints), 1, MaxRefugees);
            var kindDef = PawnKindDefOf.Refugee ?? PawnKindDefOf.Villager;

            var victims = new List<Pawn>();
            for (int i = 0; i < count; i++)
                victims.Add(PawnGenerator.GeneratePawn(new PawnGenerationRequest(kindDef, refugeeFaction)));

            slate.Set("difficulty", points);
            slate.Set("refugeeCount", count);
            slate.Set("victims", victims);
            slate.Set("refugeeFaction", refugeeFaction);
            slate.Set("raiderFaction", raider);
            slate.Set("raidPoints", points * RaidPointsFactor);
            slate.Set("category", category.GetValue(slate));
        }

        protected override bool TestRunInt(Slate slate)
        {
            var map = slate.Get<Map>("map") ?? Find.AnyPlayerHomeMap ?? Find.CurrentMap;
            if (map == null || !map.IsPlayerHome)
            {
                Log.Error("[FamiliaMyth] 难民安置：找不到玩家据点地图，任务未发布。");
                return false;
            }
            return true;
        }

        //现生成临时难民派系（对齐原版难民事件）：OutlanderRefugee（缺 DLC 时逐级兜底），hidden + temporary
        private static Faction CreateRefugeeFaction()
        {
            var def = FactionDefOf.OutlanderRefugee ?? FactionDefOf.Beggars ?? FactionDefOf.OutlanderCivil;
            if (def == null)
            {
                Log.Error("[FamiliaMyth] 难民安置：找不到可用的难民派系 Def，任务未发布。");
                return null;
            }

            var relations = new List<FactionRelation>();
            foreach (var f in Find.FactionManager.AllFactionsListForReading)
                if (f.def != null && !f.def.PermanentlyHostileTo(def))
                    relations.Add(new FactionRelation { other = f, kind = FactionRelationKind.Neutral });

            var parms = new FactionGeneratorParms(def, default(IdeoGenerationParms), hidden: true);
            if (ModsConfig.IdeologyActive)
                parms.ideoGenerationParms = new IdeoGenerationParms(def, forceNoExpansionIdeo: false,
                    DefDatabase<PreceptDef>.AllDefs.Where(p => p.proselytizes || p.approvesOfCharity).ToList());

            var faction = FactionGenerator.NewGeneratedFactionWithRelations(parms, relations);
            faction.temporary = true;
            Find.FactionManager.Add(faction);
            return faction;
        }
    }
}
