using RimWorld;
using Verse;

namespace FamiliaMyth.Abilities
{
    /// <summary>
    /// 恩惠类 StatDef 专用 worker：恩惠数值只属于角色。
    /// StatCategoryDef.Pharna 开了 displayAllByDefault，若沿用原版 StatWorker，
    /// 非 Pawn 的 Thing（物品/建筑/地形）会绕过 showOnPawns 与 showIfHediffsPresent 检查，
    /// 从兜底分支判定为「应显示」，导致信息卡出现恩惠栏，故在此提前拦掉。
    /// </summary>
    public class StatWorker_Pharna : StatWorker
    {
        public override bool ShouldShowFor(StatRequest req)
        {
            return req.Thing is Pawn && base.ShouldShowFor(req);
        }
    }
}
