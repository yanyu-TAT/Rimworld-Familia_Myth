namespace FamiliaMyth.Feat
{
    // 伟业性质判定器：每个对应一个 FeatTraitDef（见 Defs/FeatTraitDefs/FeatTraits.xml）。
    // 阈值集中在本文件便于调参；判定只看战斗事实，不看来源。

    //击退强袭：成规模的正面来袭
    public class Judger_RepelledAssault : FeatTraitJudger
    {
        private const float MinThreat = 400f;
        private const int MinEnemies = 3;

        public override bool Judge(FeatContext ctx)
            => ctx.Threat() >= MinThreat
            && ctx.enemies.Count >= MinEnemies;
    }

    //以寡敌众：少数人挡下远多于己方的敌人
    public class Judger_AgainstTheOdds : FeatTraitJudger
    {
        private const float MinThreat = 800f;
        private const int MinEnemies = 6;
        private const int MaxFriendlies = 3;
        private const int MinDefeated = 3;

        public override bool Judge(FeatContext ctx)
        {
            float threat = ctx.Threat();
            return threat >= MinThreat
                && ctx.enemies.Count >= MinEnemies
                && ctx.friendlies.Count <= MaxFriendlies
                && ctx.DefeatedCount() >= MinDefeated;
        }
    }

    //无伤克敌：我方无人倒下（按全部参战者计，非仅受恩惠者）
    public class Judger_FlawlessVictory : FeatTraitJudger
    {
        private const float MinThreat = 600f;
        private const int MinEnemies = 4;

        public override bool Judge(FeatContext ctx)
            => ctx.Threat() >= MinThreat
            && ctx.enemies.Count >= MinEnemies
            && ctx.DownedCount() == 0;
    }

    //绝境克强敌：敌方存在明显强者且战果扎实
    public class Judger_SlayTheMighty : FeatTraitJudger
    {
        private const float MinThreat = 1200f;
        private const float MinEnemyCombatPower = 85f;   //对齐原版 AncientSoldier / 部落首领
        private const int MinDefeated = 2;

        public override bool Judge(FeatContext ctx)
            => ctx.Threat() >= MinThreat
            && ctx.MaxCombatPower() >= MinEnemyCombatPower
            && ctx.DefeatedCount() >= MinDefeated;
    }
}
