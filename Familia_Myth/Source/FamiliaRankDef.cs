using JetBrains.Annotations;
using Verse;

namespace FamiliaMyth.Familia
{
    public class FamiliaRankDef : Def
    {
        public FamiliaCategoryDef categoryDef;

        public int seniority; //评级序号
        public int upgradePointsRequired; //晋升到本级所需的新增升级点数（晋升时从累计值中扣减）

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (var s in base.ConfigErrors())
                yield return s;

            if (categoryDef == null)
            {
                yield return $"Rank {defName} Belong To No Category.";
                yield break;
            }

            var list = categoryDef.rankDefs.ToList();
            if (list.Count(r => r.seniority == seniority) > 1)
                yield return $"Category {categoryDef.defName} Has More than One Ranks Of Seniority{seniority}.";
        }
    }
}
