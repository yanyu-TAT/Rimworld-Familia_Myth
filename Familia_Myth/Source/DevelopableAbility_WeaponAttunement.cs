using FamiliaMyth.Pharna;
using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    /// <summary>
    /// 武器同调发展能力：提升近战命中率，按武器 def 积累武器熟练度，
    /// 熟练度提供概率重置近战攻击冷却与暴击概率。
    /// </summary>
    public class DevelopableAbility_WeaponAttunement : DevelopableAbility
    {
        //近战命中率乘算系数（每点发展能力熟练度）
        private const float HitChancePerProf = 0.001f;
        //装备时间间隔：每多少 tick 提高一点武器熟练度
        private const int WeaponProfIntervalTicks = 1250;
        //每个间隔提高的武器熟练度
        private const float WeaponProfPerInterval = 1f;
        //每点武器熟练度提供的概率
        private const float ChancePerWeaponProf = 0.0005f;
        //两类概率的封顶
        private const float MaxChance = 0.5f;
        //近战技能经验 -> 发展能力熟练度
        public const float ProfGainPerXp = 0.005f;

        private Dictionary<ThingDef, float> weaponProficiency = new();

        //武器熟练度上限由发展能力熟练度决定
        public float MaxWeaponProficiency => proficiency;

        public override bool ApplyModifiers(StatDef stat, ref float value)
        {
            if (pharna == null || stat == null) return false;
            if (stat == StatDefOf.MeleeHitChance)
            {
                value *= 1f + proficiency * HitChancePerProf;
                return true;
            }
            return false;
        }

        public override string GetExplanation(StatDef stat)
        {
            if (pharna == null || stat == null) return "";
            if (stat == StatDefOf.MeleeHitChance)
            {
                string label = "FamiliaMyth.DevelopableAbility.WeaponAttunementLabel".Translate();
                return $"Lv.{pharna.level} {label}{Rank}: x{(1f + proficiency * HitChancePerProf).ToStringPercent()}";
            }
            return "";
        }

        public float GetWeaponProficiency(ThingDef weaponDef)
        {
            if (weaponDef == null || weaponProficiency == null) return 0f;
            return weaponProficiency.TryGetValue(weaponDef, out float value) ? value : 0f;
        }

        //概率与武器熟练度成正比，封顶 MaxChance
        public float GetChance(ThingDef weaponDef)
            => Mathf.Min(MaxChance, GetWeaponProficiency(weaponDef) * ChancePerWeaponProf);

        public float GetCooldownResetChance(ThingDef weaponDef) => GetChance(weaponDef);

        public float GetCritChance(ThingDef weaponDef) => GetChance(weaponDef);

        public string GetRankText(ThingDef weaponDef)
            => Hediff_Pharna.GetDegreeRank((int) GetWeaponProficiency(weaponDef) / 100);

        public void AddWeaponProficiency(ThingDef weaponDef, float amount)
        {
            if (weaponDef == null || amount <= 0f) return;
            float max = MaxWeaponProficiency;
            if (max <= 0f) return;
            float current = GetWeaponProficiency(weaponDef);
            if (current >= max) return;
            weaponProficiency[weaponDef] = Mathf.Min(max, current + amount);
        }

        public override void Tick()
        {
            Pawn pawn = pharna?.pawn;
            if (pawn == null || !pawn.Spawned) return;

            SyncDisplayHediff();

            if (!pawn.IsHashIntervalTick(WeaponProfIntervalTicks)) return;
            Thing weapon = CurrentMeleeWeapon(pawn);
            if (weapon == null) return;
            AddWeaponProficiency(weapon.def, WeaponProfPerInterval);
        }

        //展示 Hediff 只保留当前装备近战武器的一条
        public void SyncDisplayHediff()
        {
            Pawn pawn = pharna?.pawn;
            if (pawn?.health?.hediffSet == null) return;

            Thing weapon = CurrentMeleeWeapon(pawn);
            bool exists = false;
            foreach (Hediff hediff in pawn.health.hediffSet.hediffs.ToList())
            {
                if (hediff is not Hediff_WeaponAttunement display) continue;
                if (weapon != null && display.weaponDef == weapon.def)
                {
                    exists = true;
                    continue;
                }
                pawn.health.RemoveHediff(display);
            }

            if (weapon == null || exists) return;
            if (pawn.health.AddHediff(Hediff_WeaponAttunement.HediffDef) is Hediff_WeaponAttunement added)
                added.weaponDef = weapon.def;
        }

        private static Thing CurrentMeleeWeapon(Pawn pawn)
        {
            Thing weapon = pawn?.equipment?.Primary;
            return weapon?.def != null && weapon.def.IsMeleeWeapon ? weapon : null;
        }

        public override void Apply(Hediff_Pharna p)
        {
            base.Apply(p);
            SyncDisplayHediff();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref weaponProficiency, "weaponProficiency", LookMode.Def, LookMode.Value);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
                weaponProficiency ??= new Dictionary<ThingDef, float>();
        }
    }

    public static class WeaponAttunementUtility
    {
        public static DevelopableAbility_WeaponAttunement GetWeaponAttunement(this Pawn pawn)
        {
            Hediff_Pharna pharna = pawn?.GetPharna();
            if (pharna?.developableAbilities == null) return null;
            foreach (DevelopableAbility ability in pharna.developableAbilities)
                if (ability is DevelopableAbility_WeaponAttunement attunement)
                    return attunement;
            return null;
        }
    }
}
