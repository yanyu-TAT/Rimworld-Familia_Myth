using FamiliaMyth.Pharna;
using Verse;

namespace Familia_Myth.Source
{
    /// <summary>
    /// 用于提供渲染帧的绘制入口
    /// </summary>
    public class MapComponent_Pharna : MapComponent
    {
        public static MapComponent_Pharna Instance => Find.CurrentMap?.GetComponent<MapComponent_Pharna>();

        public List<Pawn> pawnsWithPharna = new();

        public MapComponent_Pharna(Map map) : base(map) { }

        public override void MapComponentUpdate()
        {
            base.MapComponentUpdate();
            foreach(var pawn in pawnsWithPharna.ToList())
            {
                if (Hediff_Pharna.GetPharna(pawn) is not Hediff_Pharna pharna)
                {
                    pawnsWithPharna.Remove(pawn);
                    continue;
                }
                pharna.Update();
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref pawnsWithPharna, "pawnsWithPharna", LookMode.Reference);
        }
    }
}
