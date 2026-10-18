using RimWorld;
using Verse;

namespace FamiliaMyth.Feat
{
    // 伟业性质判定器：每个对应一个 FeatTraitDef（见 Defs/FeatTraitDefs/FeatTraits.xml）。
    // 阈值集中在本文件便于调参；判定只看战斗事实，不看来源。
    // 作用域由 FeatTraitDef.scope 声明：Battle 实现 Judge(ctx)，Pawn 实现 Judge(ctx, subject)。

    //以寡敌众：少数人挡下远多于己方的敌人，且伤害主要由我方打出
    public class Judger_AgainstTheOdds : FeatTraitJudger
    {
        private const float MinThreat = 1000f;
        private const int MinFraction = 10;
        private const int MinDefeated = 10;
        private const float MinTotalFraction = 0.5f;

        public override bool Judge(FeatContext ctx)
        {
            float enemyDamage = ctx.EnemyDamageTotal();
            return ctx.Threat() >= MinThreat
                && enemyDamage > 0f
                && ctx.FriendliesDamageTotal() >= enemyDamage * MinTotalFraction
                && (float)ctx.enemies.Count / ctx.friendlies.Count >= MinFraction
                && ctx.DefeatedCount() >= MinDefeated;
        }
    }

    //大获全胜：我方无人倒下，且伤害主要由我方打出（全部参战者计，非仅受恩惠者）
    public class Judger_CompleteVictory : FeatTraitJudger
    {
        private const float MinThreat = 600f;
        private const float MinTotalFraction = 0.5f;

        public override bool Judge(FeatContext ctx)
        {
            float enemyDamage = ctx.EnemyDamageTotal();
            return ctx.Threat() >= MinThreat
                && enemyDamage > 0f
                && ctx.FriendliesDamageTotal() >= enemyDamage * MinTotalFraction
                && ctx.DownedCount() == 0;
        }
    }

    //一夫当关：独自击退了绝大多数敌人
    public class Judger_SingleDefender : FeatTraitJudger
    {
        private const float MinFraction = 0.6f;
        private const float MinThreat = 600f;

        public override bool Judge(FeatContext ctx, Pawn subject)
        {
            if (subject == null || ctx.Threat() < MinThreat) return false;
            float total = ctx.TotalContribution();
            if (total <= 0f) return false;
            return ctx.ContributionOf(subject) / total > MinFraction;
        }
    }

    //摧枯拉朽：战斗在短时间内结束，且伤害主要由我方打出
    public class Judger_EasilyOvercome : FeatTraitJudger
    {
        private const float MaxTime = GenDate.TicksPerHour * 2;
        private const float MinTotalFraction = 0.75f;
        private const float MinThreat = 600f;

        public override bool Judge(FeatContext ctx)
        {
            float enemyDamage = ctx.EnemyDamageTotal();
            return ctx.Threat() >= MinThreat
                && ctx.BattleDuration() < MaxTime
                && enemyDamage > 0f
                && ctx.FriendliesDamageTotal() > enemyDamage * MinTotalFraction;
        }
    }
}
