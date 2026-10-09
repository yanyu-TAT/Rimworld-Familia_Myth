using FamiliaMyth.Pharna;
using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    public abstract class Skill : IExposable, IAscendReward
    {
        public SkillDef def;

        protected int cooldownTimer = -1;
        private int charge = 0;
        protected int Charge
        {
            get => charge;
            set => charge = Mathf.Clamp(value, 0, MaxCharge);
        }
        protected int MaxCharge => def.maxCharge;
        public int CooldownTicks => (int)(def.cooldown * 60f);
        public bool IsReady => Charge > 0;
        public bool MaxCharged => Charge >= MaxCharge;
        public bool IsCooldown => cooldownTimer > 0;
        public float CooldownProgress => Mathf.Clamp01((float)cooldownTimer / CooldownTicks);

        public string TopRightLabel => MaxCharge > 1 ? $"{Charge}/{MaxCharge}" : null;

        public string StateText
        {
            get
            {
                if (active) return "FamiliaMyth.Ability.SkillActive".Translate();
                string s = "";
                //可多次充能的技能
                if (MaxCharge > 1)
                {
                    //显示充能次数
                    s += "FamiliaMyth.Ability.SkillCharge".Translate(Charge);
                    //未在冷却中或充能已满则不显示冷却文本
                    if (!IsCooldown || MaxCharged) return s;
                    //冷却文本和充能文本间的逗号
                    s += "，";
                }
                else if (!IsCooldown)
                {
                    //不可多次充能且已冷却完毕
                    s += "FamiliaMyth.Ability.SkillReady".Translate();
                    return s;
                }

                //冷却文本
                if (cooldownTimer < 2500)
                    s += "FamiliaMyth.Ability.SkillCooldown".Translate(cooldownTimer.ToStringSecondsFromTicks("F0"));
                else
                    s += "FamiliaMyth.Ability.SkillCooldown".Translate(cooldownTimer.ToStringTicksToPeriod());

                return s;
            }
        }

        public RewardType RewardType => RewardType.Skill;

        public RewardRarity Rarity => def.rarity;

        public string DefName => def?.defName;

        public string Label => def.label;

        public string Description => def.description;

        public Hediff_Pharna pharna;

        public Pawn Owner => pharna?.pawn;

        /// <summary>
        /// 僵直
        /// 对于有延迟效果的主动技能效果，应该在延迟过程中禁用其他主动效果以及自身的冷却
        /// </summary>
        public bool stunning = false;
        /// <summary>
        /// 正在生效
        /// 对于有持续效果的技能，效果持续过程中不进行冷却
        /// </summary>
        public bool active = false;

        public void RewardInitialize()
        {
            def = DefDatabase<SkillDef>.GetNamed(GetType().Name, false);
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

            Owner.jobs?.StopAll();
            if (!def.needTarget)
            {
                if (ApplyEffect(null))
                    Charge--;
                return;
            }

            Find.Targeter.BeginTargeting(
                TargetingParms,
                t => 
                {
                    if (t.IsValid && ApplyEffect(t))
                        Charge--;
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

            //常态效果类无需处理冷却
            if (def.skillType != SkillType.Passive && ShouldCooldown)
                TickCooldown();

            //非被动效果不处理
            if (def.skillType == SkillType.Active) return;
            if (Find.TickManager.TicksGame % (int)def.checkInterval != 0) return;

            if (!CanCast) return;
            if (ApplyEffect(null) && def.skillType == SkillType.Triggered)
                Charge--;
        }

        protected virtual bool ShouldCooldown => !stunning && !active;

        private void TickCooldown()
        {
            //充能未满但未在冷却中
            if (!MaxCharged && !IsCooldown)
                cooldownTimer = CooldownTicks;
            //效果过程中不处理冷却
            if (IsCooldown)
            {
                cooldownTimer--;
                //冷却完毕且充能未满
                if (cooldownTimer <= 0 && !MaxCharged)
                    Charge++;
            }
        }

        public virtual AcceptanceReport CanCast
        {
            get{

                if (def.skillType != SkillType.Active)
                    return true;

                if (!IsReady)
                    return StateText;

                var check = pharna?.CheckStunning ?? true;
                if (!check)
                    return "FamiliaMyth.Ability.StunnedCannotCast".Translate(check.Reason);

                return true;
            }
        }

        /// <summary>
        /// 应用技能效果
        /// </summary>
        /// <param name="target">目标</param>
        /// <returns>是否成功应用效果</returns>
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
                    defaultLabel = $"Dev : {Label} Full Charge",
                    action = delegate
                    {
                        Charge = MaxCharge;
                    }
                };
            }
        }

        public virtual void ExposeData()
        {
            Scribe_Defs.Look(ref def, "def");
            Scribe_Values.Look(ref cooldownTimer, "cooldownTimer");
            Scribe_Values.Look(ref charge, "charge");
            Scribe_Values.Look(ref active, "active");
            Scribe_References.Look(ref pharna, "pharna");
        }

        public virtual void Apply(Hediff_Pharna pharna)
        {
            this.pharna = pharna;
            pharna.skills.Add(this);
            Charge = MaxCharge; //初始化时设置为满充能
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

        public virtual void NotifyEnffectStart()
        {
            active = true;
        }

        public virtual void NotifyEffectEnd()
        {
            active = false;
        }
    }
}