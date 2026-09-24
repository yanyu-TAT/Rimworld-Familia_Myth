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
            }
        }

        public override void GameComponentTick()
        {
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
    }
}