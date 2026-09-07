using Verse;

namespace FamiliaMyth.Familia
{
    public class FamiliaRankState : IExposable
    {
        public FamiliaRankDef curRankDef;
        public FamiliaCategoryDef def;

        public int curRankUpPoints;

        public int rankUpQuestID = -1;

        public int rankUpQuestRetryTick = -1;

        public bool InRetryCooldown => rankUpQuestRetryTick > Find.TickManager.TicksGame;

        public void Init(FamiliaCategoryDef def)
        {
            this.def = def;
            if (def != null)
                curRankDef = def.rankDefs.FirstOrDefault();
        }

        public void OnRankUpQuestStart(int questID)
        {
            rankUpQuestID = questID;
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref def, "def");
            Scribe_Defs.Look(ref curRankDef, "curRankDef");
            Scribe_Values.Look(ref rankUpQuestRetryTick, "rankUpQuestRetryTick");
            Scribe_Values.Look(ref rankUpQuestID, "rankUpQuestID");
            Scribe_Values.Look(ref curRankUpPoints, "curRankUpPoints");
        }
    }
}
