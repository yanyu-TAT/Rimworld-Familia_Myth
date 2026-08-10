using FamiliaMyth.Abilities;
using RimWorld;
using System.Text;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Pharna
{
    public enum PharnaStatusType{
        Strength,
        Endurance,
        Dexterity,
        Agility,
        Mana,
        Will
    }

    public class Hediff_Pharna : Hediff_Level
    {
        private static readonly float MaxExploit = 999f;
        private static readonly List<string> ranks = new();

        public float strength;      //力量
        public float endurance;     //耐久
        public float dexterity;     //灵巧
        public float agility;       //敏捷
        public float mana;         //魔力
        public float will;          //意志

        //潜在能力值
        public float strengthPotential;
        public float endurancePotential;
        public float dexterityPotential;
        public float agilityPotential;
        public float manaPotential;
        public float willPotential;

        //发展能力
        public List<DevelopableAbility> developableAbilities = new();
        //技能
        public List<Skill> skills = new();
        //魔法
        public List<Spell> spells = new();

        public float experience;    //经验值
        public float exploits;      //伟业值

        //能力值上限
        public float maxStatus = 999f;

        //美德触发间隔（10-0.001x）天
        protected int VirtueTriggerInterval => (int)((10f - 0.001 * GetFullStatus(PharnaStatusType.Will)) * GenDate.TicksPerDay);
        protected int lastVirtueTriggeredTick = 99999;
        public bool IsVirtueCooldown => Find.TickManager.TicksGame - lastVirtueTriggeredTick <= VirtueTriggerInterval;
        public VirtueType? currentVirtue = null;

        static Hediff_Pharna(){
            for (char c = 'I'; c >= 'A'; c--)
            {
                ranks.Add(c.ToString());
            }
            ranks.Add("S");
            ranks.Add("SS");
            ranks.Add("SSS");
        }

        public override void PostAdd(DamageInfo? dinfo)
        {
            // Hediff_Level.PostAdd要求Part != null，自动设置为躯干
            if (Part == null && pawn != null)
            {
                Part = pawn.RaceProps.body.corePart;
            }
            base.PostAdd(dinfo);
        }

        public override string TipStringExtra
        {
            get
            {
                StringBuilder sb = new();

                for (int i = 0; i < 6; i++)
                {
                    PharnaStatusType type = (PharnaStatusType)i;
                    float cur = GetCurrentStatus(type);
                    string rank = GetRank(cur);
                    string typeName = GetStatusName(type);
                    string line = $"{typeName} {rank}{(int)cur}";
                    sb.AppendLine(line);
                }

                sb.AppendLine();
                sb.AppendLine("FamiliaMyth.Experience".Translate() + ": " + ((int)experience).ToString());
                sb.AppendLine("FamiliaMyth.Exploits".Translate() + ": " + ((int)exploits).ToString() + "/" + ((int)MaxExploit).ToString());

                return sb.ToString().TrimEnd();
            }
        }

        public float GetCurrentStatus(PharnaStatusType type)
        {
            return type switch
            {
                PharnaStatusType.Strength => strength,
                PharnaStatusType.Endurance => endurance,
                PharnaStatusType.Dexterity => dexterity,
                PharnaStatusType.Agility => agility,
                PharnaStatusType.Mana => mana,
                PharnaStatusType.Will => will,
                _ => 0f
            };
        }

        public static string GetStatusName(PharnaStatusType type)
        {
            return type switch
            {
                PharnaStatusType.Strength => "FamiliaMyth.Strength".Translate(),
                PharnaStatusType.Endurance => "FamiliaMyth.Endurance".Translate(),
                PharnaStatusType.Dexterity => "FamiliaMyth.Dexterity".Translate(),
                PharnaStatusType.Agility => "FamiliaMyth.Agility".Translate(),
                PharnaStatusType.Mana => "FamiliaMyth.Mana".Translate(),
                PharnaStatusType.Will => "FamiliaMyth.Will".Translate(),
                _ => null
            };
        }

        public float GetFullStatus(PharnaStatusType type)
        {
            return type switch
            {
                PharnaStatusType.Strength => strength + strengthPotential,
                PharnaStatusType.Endurance => endurance + endurancePotential,
                PharnaStatusType.Dexterity => dexterity + dexterityPotential,
                PharnaStatusType.Agility => agility + agilityPotential,
                PharnaStatusType.Mana => mana + manaPotential,
                PharnaStatusType.Will => will + willPotential,
                _ => 0f
            };
        }

        public bool CanAscend()
        {
            bool status = false;
            for (int i = 0; i < 6; i++)
            {
                PharnaStatusType type = (PharnaStatusType)i;
                status = status || GetCurrentStatus(type) >= 500;
            }
            return status && exploits >= MaxExploit;
        }

        public override void PostTick()
        {
            //Log.Message($"Ticking Hediff_Pharna for pawn {pawn?.LabelShort ?? "null"}");
            base.PostTick();

            foreach(var skill in skills)
            {
                skill?.Tick();
            }

            foreach (var spell in spells)
            {
                spell?.AbilityTick();
            }

            foreach (var developableAbility in developableAbilities)
            {
                developableAbility?.Tick();
            }

            //疼痛百分比 * 0.002/tick
            if (pawn?.health?.hediffSet != null)
            {
                GainStatus(PharnaStatusType.Will, 0.002f * pawn.health.hediffSet.PainTotal);
            }
        }

        public void Ascend()
        {
            //保留潜在值
            strengthPotential += strength;
            endurancePotential += endurance;
            dexterityPotential += dexterity;
            agilityPotential += agility;
            manaPotential += mana;
            willPotential += will;

            //清零能力值
            strength = 0f; 
            endurance = 0f;
            dexterity = 0f;
            agility = 0f;
            mana = 0f;
            will = 0f;

            //消耗伟业值
            exploits -= MaxExploit;

            //升级奖励在仪式奖励中给予
            //升级
            ChangeLevel(1);
        }

        public bool GainStatus(PharnaStatusType type, float amount)
        {
            if (amount <= 0f) return false;
            float capped = Math.Min(amount, maxStatus - GetCurrentStatus(type));
            switch(type)
            {
                case PharnaStatusType.Strength: strength += capped; break;
                case PharnaStatusType.Endurance: endurance += capped; break;
                case PharnaStatusType.Dexterity: dexterity += capped; break;
                case PharnaStatusType.Agility: agility += capped; break;
                case PharnaStatusType.Mana: mana += capped; break;
                case PharnaStatusType.Will: will += capped; break;
                default: return false;
            }
            experience += capped;
            return true;
        }

        /// <summary>
        /// 伟业值增量与等级成反比
        /// </summary>
        /// <param name="amount">预定的伟业值</param>
        /// <returns>实际增加的伟业值</returns>
        public float GainExploits(float amount)
        {
            if (amount <= 0f) return 0;
            float capped = Math.Min(amount, (MaxExploit - exploits) * level);
            exploits += capped / level;
            return capped;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            base.GetGizmos();
            foreach (var spell in spells)
            {
                var gizmos = spell?.GetGizmos();
                if (gizmos != null)
                {
                    foreach (var gizmo in gizmos)
                    {
                        yield return gizmo;
                    }
                }
            }

            foreach (var skill in skills)
            {
                var gizmos = skill?.GetGizmos();
                if (gizmos != null)
                {
                    foreach (var gizmo in gizmos)
                    {
                        yield return gizmo;
                    }
                }
            }

            foreach (var developableAbility in developableAbilities)
            {
                var gizmos = developableAbility?.GetGizmos();
                if (gizmos != null)
                {
                    foreach (var gizmo in gizmos)
                    {
                        yield return gizmo;
                    }
                }
            }

            //调试指令
            if (DebugSettings.ShowDevGizmos && IsVirtueCooldown)
            {
                //重置美德冷却
                yield return new Command_Action
                {
                    defaultLabel = "DEV: Reset virtue cooldown",
                    action = delegate
                    {
                        lastVirtueTriggeredTick = Find.TickManager.TicksGame - VirtueTriggerInterval;
                        currentVirtue = null;
                    }
                };
            }
        }

        public bool HasReward(IAscendReward reward)
        {
            if (reward == null) return false;
            if (spells.Any(t => t.Label == reward.Label)) return true;
            if (skills.Any(t => t.Label == reward.Label)) return true;
            if (developableAbilities.Any(t => t.Label == reward.Label)) return true;
            return false;
        }

        public IEnumerable<string> GetRewardNames()
        {
            foreach (var spell in spells)
                yield return spell.GetType().Name;

            foreach (var skill in skills)
                yield return skill.GetType().Name;

            foreach (var developAbleability in developableAbilities)
                yield return developAbleability.GetType().Name;
        }

        public static string GetRank(float value)
        {
            int t = ((int)value) / 100;
            t = t >= 12 ? 11 : t;
            return ranks[t];
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref strength, "strength", 0f);
            Scribe_Values.Look(ref endurance, "endurance", 0f);
            Scribe_Values.Look(ref dexterity, "dexterity", 0f);
            Scribe_Values.Look(ref agility, "agility", 0f);
            Scribe_Values.Look(ref mana, "mana", 0f);
            Scribe_Values.Look(ref will, "will", 0f);

            Scribe_Values.Look(ref strengthPotential, "strengthPotential", 0f);
            Scribe_Values.Look(ref endurancePotential, "endurancePotential", 0f);
            Scribe_Values.Look(ref dexterityPotential, "dexterityPotential", 0f);
            Scribe_Values.Look(ref agilityPotential, "agilityPotential", 0f);
            Scribe_Values.Look(ref manaPotential, "manaPotential", 0f);
            Scribe_Values.Look(ref willPotential, "willPotential", 0f);

            Scribe_Collections.Look(ref developableAbilities, "developAbilities", LookMode.Deep);
            Scribe_Collections.Look(ref skills, "skills", LookMode.Deep);
            Scribe_Collections.Look(ref spells, "spells", LookMode.Deep);

            Scribe_Values.Look(ref experience, "experience", 0f);
            Scribe_Values.Look(ref exploits, "exploits", 0f);

            Scribe_Values.Look(ref lastVirtueTriggeredTick, "lastVirtueTriggeredTick", 99999);
            Scribe_Values.Look(ref currentVirtue, "currentVirtue", null);
        }

        private static HediffDef cachedPharnaDef;
        private static HediffDef PharnaDef => cachedPharnaDef ??= DefDatabase<HediffDef>.GetNamed("Hediff_Pharna");

        public static Hediff_Pharna GetPharna(Pawn pawn)
        {
            if (pawn?.health?.hediffSet == null) return null;

            if (pawn.health.hediffSet.GetFirstHediffOfDef(PharnaDef) is Hediff_Pharna pharna)
                return pharna;

            return null;
        }

        public void SetVirtueCooldown()
        {
            lastVirtueTriggeredTick = Find.TickManager.TicksGame;
            currentVirtue = null;
        }

        public bool TryTriggerVirtue(float factor = 1f)
        {
            if (IsVirtueCooldown) return false;
            //0.75 * (1 - e ^ -0.00025 * x)
            float c = 0.75f * (1 - Mathf.Exp(-0.00025f * GetFullStatus(PharnaStatusType.Will))) * factor;
            if (!Rand.Chance(c)) return false;

            var values = Enum.GetValues(typeof(VirtueType));
            var type = (VirtueType)values.GetValue(Rand.Range(0, values.Length));
            var def = DefDatabase<MentalStateDef>.GetNamed($"MentalState_Virtue_{type}");

            if (def == null) return false;
            currentVirtue = type;
            return pawn.mindState.mentalStateHandler.TryStartMentalState(def);
        }

        public override void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.Notify_PawnPostApplyDamage(dinfo, totalDamageDealt);
            TryTriggerVirtue(0.1f);
        }
    }

    public static class PharnaUtilities
    {

    }
}