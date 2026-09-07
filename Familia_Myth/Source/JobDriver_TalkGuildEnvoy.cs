using FamiliaMyth.Quest;
using Verse;
using Verse.AI;

namespace FamiliaMyth.Incidents
{
    public class JobDriver_TalkGuildEnvoy : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(TargetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);

            //走到使者面前
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

            //对话
            yield return Toils_General.Do(delegate
            {
                //发送信号
                Pawn envoy = TargetPawnA;
                if (envoy == null || !envoy.Spawned) return;
                //发送信件
                SendChoiceLetter(envoy);
            });
        }

        private void SendChoiceLetter(Pawn envoy)
        {
            if (!GameComponent_GuildEnvoyTracker.HasEnvoy)
                return;

            var gen = GameComponent_GuildEnvoyTracker.QuestGenerator;
            var letter = LetterMaker.MakeLetter(
                label: gen.def?.label ?? "公会使者",
                text: gen.LetterText ?? "使者向你介绍了委托的详情。",
                def: DefDatabase<LetterDef>.GetNamed(
                    gen.Type == QuestType.Daily 
                    ? "AcceptDailyQuestFromEnvoy" 
                    : "AcceptRankUpQuestFromEnvoy"),
                lookTargets: envoy);
            if (letter != null && letter is IQuestProvider provider)
            {
                provider.Envoy = envoy;
                provider.QuestDef = gen.def;
                Find.LetterStack.ReceiveLetter(letter);
                if (!Find.WindowStack.IsOpen<Dialog_NodeTree>())
                    letter.OpenLetter();
            }
        }
    }

    public interface IQuestProvider
    {
        Pawn Envoy { get; set; }
        GuildQuestDef QuestDef { get; set; }
    }
}