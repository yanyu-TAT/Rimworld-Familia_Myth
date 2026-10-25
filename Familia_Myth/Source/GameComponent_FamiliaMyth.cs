using FamiliaMyth.Abilities;
using FamiliaMyth.Pharna;
using RimWorld;
using Verse;

namespace FamiliaMyth.Core
{
    /// <summary>
    /// 用于添加模组相关物品的游戏组件类
    /// </summary>
    public class GameComponent_FamiliaMyth : GameComponent
    {
        public static GameComponent_FamiliaMyth Instance => Current.Game?.GetComponent<GameComponent_FamiliaMyth>();

        public static List<(int tick, Action action, Thing giver)> pendingActions = new();

        public readonly Dictionary<Pawn, PharnaRecord> records = new();

        //已发送的教程信件 id
        public List<string> sentTutorials = new();

        public void CommitRecord(Hediff_Pharna pharna)
        {
            var pawn = pharna?.pawn;
            if (pawn == null) return;
            if (!records.TryGetValue(pawn, out var record))
            {
                record = new();
                records[pawn] = record;
            }
            record.Record(pharna);
        }

        public void Discard(Pawn pawn)
            => records.Remove(pawn);

        public PharnaRecord GetRecord(Pawn pawn)
        {
            if (pawn == null) return null;
            return records.TryGetValue(pawn, out var record) ? record : null;
        }

        public bool HasTutorialBeenSent(string id) => sentTutorials != null && sentTutorials.Contains(id);

        public void MarkTutorialSent(string id)
        {
            sentTutorials ??= new List<string>();
            if (!sentTutorials.Contains(id))
                sentTutorials.Add(id);
        }

        public static void Schedule(int tick, Action action, Thing giver)
        {
            pendingActions.Add((tick, action, giver));
        }

        public GameComponent_FamiliaMyth(Game game) : base() { }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref sentTutorials, "sentTutorials", LookMode.Value);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && sentTutorials == null)
                sentTutorials = new List<string>();
        }

        public override void FinalizeInit()
        {
            if (!ModsConfig.IsActive("Ludeon.RimWorld.Ideology"))
            {
                Log.Error("[FamiliaMyth] The Ideology dlc is not active. The main part of the mod will not work!");
            }
        }

        public override void GameComponentTick()
        {
            if (Find.TickManager.TicksGame % TutorialLetterHelper.GrantHintCheckInterval == 0)
                TutorialLetterHelper.CheckGrantHint();

            if (pendingActions.NullOrEmpty()) return;
            for (int i = 0; i < pendingActions.Count; i++)
            {
                var a = pendingActions[i];
                if (Spell_TheWorld.ShouldTick(a.giver))
                {
                    a.tick--;
                    pendingActions[i] = a;
                }
            }

            var list = pendingActions.Where(a => a.tick <= 0).ToList();
            foreach(var act in list)
            {
                act.action();
                pendingActions.Remove(act);
            }
        }

        public void NotifyPharnaLost(Hediff_Pharna pharna)
        {
            if (Find.LetterStack == null) return;
            CommitRecord(pharna);
            var pawn = pharna?.pawn;
            if (pawn == null || pawn.Destroyed || pawn.Dead || pawn.health?.hediffSet == null) return;
            if (GetRecord(pawn) is PharnaRecord record)
                SendLetterPharnaLost(pawn, record);
        }

        private void SendLetterPharnaLost(Pawn pawn, PharnaRecord record)
        {
            TaggedString label = "FamiliaMyth.PharnaLost.LetterLabel".Translate();
            TaggedString text = "FamiliaMyth.PharnaLost.LetterText".Translate(pawn.Name?.ToStringShort ?? "Unknown");
            LetterDef letterDef = DefDatabase<LetterDef>.GetNamed("FamiliaMyth_PharnaLost", false);
            if (letterDef == null)
            {
                Log.Error("[FamiliaMyth] Can't find LetterDef FamiliaMyth_PharnaLost, using NeutralEvent instead.");
                letterDef = LetterDefOf.NeutralEvent;
            }
            if (LetterMaker.MakeLetter(label, text, letterDef) is not ChoiceLetter_PharnaLost letter) return;
            letter.title = label;
            letter.Text = text;
            letter.pawn = pawn;
            Find.LetterStack.ReceiveLetter(letter);
        }
    }
}