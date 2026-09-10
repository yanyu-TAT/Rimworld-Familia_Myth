using RimWorld;

namespace FamiliaMyth.Quest
{
    //「物资押运」定居点交互 comp 的属性类（经 Patch 注入 SettlementDef.comps）
    public class WorldObjectCompProperties_SupplyEscort : WorldObjectCompProperties
    {
        public WorldObjectCompProperties_SupplyEscort()
        {
            compClass = typeof(WorldObjectComp_SupplyEscort);
        }
    }
}
