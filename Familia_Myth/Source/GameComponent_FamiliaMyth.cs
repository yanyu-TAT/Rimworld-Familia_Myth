using FamiliaMyth.Abilities;
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

        public static void Schedule(int tick, Action action, Thing giver)
        {
            pendingActions.Add((tick, action, giver));
        }

        public GameComponent_FamiliaMyth(Game game) : base() { }

        public override void FinalizeInit()
        {
            if (!ModsConfig.IsActive("Ludeon.RimWorld.Ideology"))
            {
                Log.Error("[FamiliaMyth] The Ideology dlc is not active. The main part of the mod will not work!");
                return;
            }

            Ideo ideo = Find.FactionManager.OfPlayer?.ideos?.PrimaryIdeo;
            if (ideo == null)
            {
                Log.Error("[FamiliaMyth] Player's ideo not found!");
                return;
            }

            //添加仪式
            TryAddRitual(ideo, "Ritual_GrantPharna", "RitualPattern_GrantPharna");
            TryAddRitual(ideo, "Ritual_AscendPharna", "RitualPattern_AscendPharna");

            Log.Message("[FamiliaMyth] Rituals Added");
        }

        public override void GameComponentTick()
        {
            if (pendingActions.NullOrEmpty()) return;
            pendingActions.ForEach(a =>
            {
                if (Spell_TheWorld.ShouldTick(a.giver))
                    a.tick--;
            });

            var list = pendingActions.Where(a => a.tick <= 0).ToList();
            foreach(var act in list)
            {
                act.action();
                pendingActions.Remove(act);
            }
        }

        private void TryAddRitual(Ideo ideo, string preceptDefName, string patternDefName)
        {
            if (ideo.PreceptsListForReading.Any(p => p.def.defName == preceptDefName))
                return;

            PreceptDef preceptDef = DefDatabase<PreceptDef>.GetNamed(preceptDefName);
            RitualPatternDef ritualPatternDef = DefDatabase<RitualPatternDef>.GetNamed(patternDefName);
            if (preceptDef == null || ritualPatternDef == null)
            {
                Log.Error($"[FamiliaMyth] {preceptDefName} or {patternDefName} def not found");
                return;
            }

            //创建实例
            Precept_Ritual ritual = (Precept_Ritual)PreceptMaker.MakePrecept(preceptDef);
            ritualPatternDef.Fill(ritual);  //填充实例
            ritual.Init(ideo);              //初始化实例
            ideo.AddPrecept(ritual);        //添加到文化
        }
    }
}