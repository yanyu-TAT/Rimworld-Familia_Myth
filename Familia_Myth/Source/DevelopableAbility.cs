using Verse;

namespace FamiliaMyth.Pharna
{
    /// <summary>
    /// 发展能力数据结构，暂未实现
    /// </summary>
    public class DevelopableAbility : IExposable
    {
        public DevelopableAbilityDef def;
        public float proficiency; //熟练度
        public void ExposeData()
        {
            Scribe_Defs.Look(ref def, "def");
            Scribe_Values.Look(ref proficiency, "proficiency", 0f);
        }
    }
}