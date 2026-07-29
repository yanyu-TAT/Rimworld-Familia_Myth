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
                //移除未对话
                IncidentWorker_GuildEnvoy.NotSpoken.Remove(envoy);
                //发送信件
                SendChoiceLetter(envoy);
            });
        }
        private void SendChoiceLetter(Pawn envoy)
        {
            if (LetterMaker.MakeLetter(
                label: "公会使者",
                text: "使者向你介绍了委托的详情。\n\n" +
                      "公会在附近发现了一处邪恶派阀据点，\n" +
                      "放任不管会严重危害周边治安。\n" +
                      "现请求派出冒险者前去肃清。\n\n" +
                      "是否接受？",
                def: DefDatabase<LetterDef>.GetNamed("AcceptMissionFromEnvoy"),
                lookTargets: envoy) is ChoiceLetter_MissionOffer letter)
            {
                letter.envoy = envoy;
                Find.LetterStack.ReceiveLetter(letter);
                if (Find.WindowStack.IsOpen<Dialog_NodeTree>() == false)
                    letter.OpenLetter();
            }
        }
    }
}