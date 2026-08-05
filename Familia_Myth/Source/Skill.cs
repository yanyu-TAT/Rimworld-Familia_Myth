using FamiliaMyth.Pharna;
using RimWorld;
using Verse;
using static UnityEngine.GraphicsBuffer;

namespace FamiliaMyth.Abilities
{
    public abstract class Skill : IExposable, IAscendReward
    {
        public SkillDef def;

        protected int lastCastTick = -9999;
        public int CooldownTicks => (int)(def.cooldown * 60f);
        protected int CurrentTick => Find.TickManager.TicksGame;
        public bool IsCooldown => CurrentTick - lastCastTick < CooldownTicks;
        public float RemainingCooldown => Math.Max(0, CooldownTicks - (CurrentTick - lastCastTick)) / 60f;

        public RewardType RewardType => RewardType.Skill;

        public RewardRarity Rarity => def.rarity;

        public string Label => def.label;

        public string Description => def.description;

        public void RewardInitialize()
        {
            def = DefDatabase<SkillDef>.GetNamed(GetType().Name, false);
        }

        protected virtual void SetCooldown()
        {
            lastCastTick = CurrentTick;
        }

        
        /// <summary>
        /// 主动技能尝试释放，如果可释放则调用ApplyEffct，并根据返回的结果进入冷却
        /// </summary>
        /// <param name="caster">释放者</param>
        /// <param name="target">目标</param>
        /// <returns>释放结果</returns>
        public virtual bool TryCast(Pawn caster, LocalTargetInfo target)
        {
            if (caster == null) return false;
            if (IsCooldown) return false;
            if (!CheckCondition(caster)) return false;

            if (def.skillType != SkillType.Active)
            {
                Log.Warning("[FamiliaMyth] Trying to cast passive or triggered skill!");
                return false;
            }

            if (ApplyEffect(caster, target)) 
            { 
                lastCastTick = CurrentTick; 
                return true;
            }

            return false;
        }

        /// <summary>
        /// 每tick检查被动效果是否适用，如果可适用则调用ApplyEffect，并根据返回的结果进入冷却
        /// </summary>
        /// <param name="pawn">效果持有者</param>
        public virtual void Tick(Pawn pawn)
        {
            if (pawn == null) return;
            if (def.skillType == SkillType.Active) return;
            if (Find.TickManager.TicksGame % (int)def.checkInterval != 0) return;  
            if (IsCooldown) return;
            if (!CheckCondition(pawn)) return;

            if (ApplyEffect(pawn, pawn))
                lastCastTick = CurrentTick;
        }

        /// <summary>
        /// 检查释放/触发条件
        /// </summary>
        /// <param name="caster">使用者</param>
        /// <returns>检查结果，默认为true（即无条件触发被动效果）</returns>
        public virtual bool CheckCondition(Pawn caster) { return true; }

        /// <summary>
        /// 应用技能效果
        /// </summary>
        /// <param name="pawn">使用者</param>
        /// <param name="target">目标</param>
        /// <returns>效果结果</returns>
        public abstract bool ApplyEffect(Pawn pawn, LocalTargetInfo target);

        public virtual IEnumerable<Gizmo> GetGizmos() { return null; }

        public virtual void ExposeData()
        {
            Scribe_Defs.Look(ref def, "def");
            Scribe_Values.Look(ref lastCastTick, "lastCastTick");
        }

        public virtual void Apply(Hediff_Pharna pharna)
        {
            pharna.skills.Add(this);
            lastCastTick = CurrentTick - CooldownTicks; // 初始化时设置为可立即使用
        }
    }
}