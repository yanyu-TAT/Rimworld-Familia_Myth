using Verse;

namespace FamiliaMyth
{
    //本模组自建音效的 SoundDef 缓存；缺失时告警并退化为静默
    [StaticConstructorOnStartup]
    public static class FamiliaMythSounds
    {
        public static readonly SoundDef Ascend;
        public static readonly SoundDef CastHum;
        public static readonly SoundDef CastBreath;
        public static readonly SoundDef CastRelease;
        public static readonly SoundDef CastAbort;

        static FamiliaMythSounds()
        {
            Ascend = Get("FamiliaMyth_Sound_Ascend");
            CastHum = Get("FamiliaMyth_Sound_CastHum");
            CastBreath = Get("FamiliaMyth_Sound_CastBreath");
            CastRelease = Get("FamiliaMyth_Sound_CastRelease");
            CastAbort = Get("FamiliaMyth_Sound_CastAbort");
        }

        private static SoundDef Get(string defName)
        {
            SoundDef def = DefDatabase<SoundDef>.GetNamed(defName, false);
            if (def == null)
                Log.Warning($"[FamiliaMyth] SoundDef not found: {defName}");
            return def;
        }
    }
}
