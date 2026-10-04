using FamiliaMyth.Familia;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace FamiliaMyth.Quest
{
    //日常任务「罪犯追捕」根节点：统一点数 → 守卫威胁参数(Outpost × GuardFactor) →
    //生成 1 名通缉犯(敌派系 worldPawn) → slate 参数
    public class QuestNode_Root_Daily_CriminalHunt : QuestNode
    {
        //守卫威胁点数占任务总点数的比例（通缉犯战力另计；供调参）
        public const float GuardFactor = 0.8f;

        protected override void RunInt()
        {
            var slate = QuestGen.slate;

            QuestUtilities.CalculateTaskParams(isRankUp: false, out float points);
            float guardPoints = points * GuardFactor;

            //守卫敌人：罪犯窝点本体（原版 Outpost 营地）
            var outpostDef = DefDatabase<SitePartDef>.GetNamedSilentFail("Outpost");
            if (outpostDef == null) return;
            var sitePartsParams = new List<SitePartDefWithParams>
            {
                new(outpostDef, new SitePartParams { threatPoints = guardPoints })
            };

            //罪犯派系：随机敌对派系（守卫与通缉犯同派系）
            var criminalFaction = Find.FactionManager.RandomEnemyFaction(
                allowHidden: false, allowNonHumanlike: false, allowDefeated: false);
            if (criminalFaction == null) return;

            //通缉犯：敌派系成员（worldPawn，由 QuestPart_CriminalHuntSpawn 于进图时放置）
            Pawn wanted = PawnGenerator.GeneratePawn(
                new PawnGenerationRequest(criminalFaction.RandomPawnKind(), criminalFaction));
            if (wanted == null) return;

            //押运方：公会（运行时补建实例；缺失时兜底随机中立/友好可见派系）
            Faction guild = Find.FactionManager.FirstFactionOfDef(GameComponent_FamiliaRankTracker.GuildDef);
            if (guild == null)
            {
                foreach (var f in Find.FactionManager.AllFactionsVisible)
                    if (!f.IsPlayer && !f.def.isPlayer
                        && f.RelationKindWith(Faction.OfPlayer) != FactionRelationKind.Hostile)
                    {
                        guild = f;
                        break;
                    }
            }

            slate.Set("difficulty", points);
            slate.Set("sitePartsParams", sitePartsParams);
            slate.Set("siteFaction", criminalFaction);
            slate.Set("wanted", wanted);
            slate.Set("wantedPawns", new List<Pawn> { wanted });
            slate.Set("guildFaction", guild);
        }

        protected override bool TestRunInt(Slate slate) => true;
    }
}
