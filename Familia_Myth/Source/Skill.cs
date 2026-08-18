using FamiliaMyth.Pharna;
using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    public abstract class Skill : IExposable, IAscendReward
    {
        public SkillDef def;

        protected int lastCastTick = -9999;
        public int CooldownTicks => (int)(def.cooldown * 60f);
        protected int CurrentTick => Find.TickManager.TicksGame;
        public bool IsCooldown => CurrentTick - lastCastTick < CooldownTicks;
        public float RemainingCooldown => Mathf.Max(0, CooldownTicks - (CurrentTick - lastCastTick)) / 60f;
        public float CooldownProgress => Mathf.Clamp01(RemainingCooldown / def.cooldown);

        public RewardType RewardType => RewardType.Skill;

        public RewardRarity Rarity => def.rarity;

        public string Label => def.label;

        public string Description => def.description;

        public Hediff_Pharna pharna;

        public Pawn Owner => pharna?.pawn;

        /// <summary>
        /// 对于有延迟效果的主动技能效果，应该在延迟过程中禁用其他主动效果
        /// </summary>
        public bool stunning = false;

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
        public virtual void TryCast()
        {
            if (Owner == null) return;

            if (def.skillType != SkillType.Active)
            {
                Log.Warning("[FamiliaMyth] Trying to cast passive or triggered skill!");
                return;
            }

            if (!def.needTarget)
            {
                if (ApplyEffect(null))
                    lastCastTick = CurrentTick;
                return;
            }

            Find.Targeter.BeginTargeting(
                TargetingParms,
                t => 
                { 
                    if (t.IsValid && ApplyEffect(t)) 
                        lastCastTick = CurrentTick; 
                },
                null,                 //高亮（默认）
                null,                 //过滤器
                Owner,                //施法者
                null,                 //选择状态结束时行为
                null,                 //鼠标额外贴图
                true,                 //是否播放确认音效
                null,                 //选择时GUI层每帧行为
                OnTargeting);   //选择时世界层每帧行为
        }

        public virtual TargetingParameters TargetingParms => TargetingParameters.ForThing();

        /// <summary>
        /// 每tick检查被动效果是否适用，如果可适用则调用ApplyEffect，并根据返回的结果进入冷却
        /// </summary>
        public virtual void Tick()
        {
            if (Owner == null) return;
            //非被动效果不处理
            if (def.skillType == SkillType.Active) return;
            if (Find.TickManager.TicksGame % (int)def.checkInterval != 0) return;
            if (!CanCast) return;

            if (ApplyEffect(null))
                lastCastTick = CurrentTick;
        }

        public virtual AcceptanceReport CanCast
        {
            get{
                if (IsCooldown)
                    return $"冷却中，剩余{RemainingCooldown:F1}秒";

                if (def.skillType != SkillType.Active)
                    return true;

                var check = pharna?.CheckStunning ?? true;
                if (!check)
                    return $"{check.Reason}效果结束前不可重复释放";

                return true;
            }
        }

        /// <summary>
        /// 应用技能效果
        /// </summary>
        /// <param name="target">目标</param>
        /// <returns>效果结果</returns>
        public abstract bool ApplyEffect(LocalTargetInfo target);

        public virtual IEnumerable<Gizmo> GetGizmos()
        {
            if (def?.skillType == SkillType.Active)
            {
                yield return new Command_Skill(this)
                {
                    action = TryCast
                };
            }

            if (DebugSettings.ShowDevGizmos && IsCooldown)
            {
                yield return new Command_Action
                {
                    defaultLabel = $"Dev : {Label} Reset Cooldown",
                    action = delegate
                    {
                        lastCastTick = CurrentTick - CooldownTicks;
                    }
                };
            }
        }

        public virtual void ExposeData()
        {
            Scribe_Defs.Look(ref def, "def");
            Scribe_Values.Look(ref lastCastTick, "lastCastTick");
            Scribe_References.Look(ref pharna, "pharna");
        }

        public virtual void Apply(Hediff_Pharna pharna)
        {
            this.pharna = pharna;
            pharna.skills.Add(this);
            lastCastTick = CurrentTick - CooldownTicks; //初始化时设置为可立即使用
        }

        /// <summary>
        /// 鼠标悬停在按钮上时的行为
        /// </summary>
        public virtual void OnHovering() { }

        /// <summary>
        /// 每个渲染帧调用的绘制行为
        /// </summary>
        public virtual void Update() { }

        /// <summary>
        /// 目标选择时的每帧渲染
        /// </summary>
        /// <param name="targetInfo">Targeter返回的目标</param>
        protected virtual void OnTargeting(LocalTargetInfo targetInfo) { }
    }
}