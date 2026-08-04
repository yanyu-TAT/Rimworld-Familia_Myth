using Verse;

namespace FamiliaMyth.Familia
{
    public class FamiliaRankState : IExposable
    {
        public FamiliaRankDef curRankDef;
        public FamiliaCategoryDef def;

        public int curRankUpPoints;

        public int rankUpEnvoyID = -1;
        public int rankUpQuestID = -1;

        public bool RankUpEnvoyPending => rankUpEnvoyID != -1;

        public void ExposeData()
        {
            Scribe_Defs.Look(ref def, "def");
            Scribe_Defs.Look(ref curRankDef, "curRankDef");
            Scribe_Values.Look(ref rankUpQuestID, "rankUpQuestID");
            Scribe_Values.Look(ref rankUpEnvoyID, "rankUpEnvoyID");
            Scribe_Values.Look(ref curRankUpPoints, "curRankUpPoints");
        }

        public void Init(FamiliaCategoryDef def)
        {
            this.def = def;
            if (def != null)
                curRankDef = def.rankDefs.FirstOrDefault();
        }

        public void OnRankUpQuestStart(int questID)
        {
            rankUpEnvoyID = -1;
            rankUpQuestID = questID;
        }


    }
}
