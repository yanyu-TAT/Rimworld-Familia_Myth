using FamiliaMyth.Abilities;
using FamiliaMyth.Feat;
using Verse;

namespace FamiliaMyth.Pharna
{
    public class PharnaRecord
    {
        public int level = 1;
        public float strength;
        public float endurance;
        public float dexterity;
        public float agility;
        public float mana;
        public float will;
        public float strengthPotential;
        public float endurancePotential;
        public float dexterityPotential;
        public float agilityPotential;
        public float manaPotential;
        public float willPotential;
        public float experience;
        public float manaPool;
        public int ascendsWithoutRare;
        public PharnaStatusType? inscriptionType;
        public bool inscriptionAvailable;

        public List<FeatRecord> feats = new();
        public List<string> skills = new();
        public List<string> spells = new();
        public List<(string, float)> developableAbilities = new();

        public void Record(Hediff_Pharna pharna)
        {
            level = pharna.level;
            strength = pharna.strength;
            endurance = pharna.endurance;
            dexterity = pharna.dexterity;
            agility = pharna.agility;
            mana = pharna.mana;
            will = pharna.will;
            strengthPotential = pharna.strengthPotential;
            endurancePotential = pharna.endurancePotential;
            dexterityPotential = pharna.dexterityPotential;
            agilityPotential = pharna.agilityPotential;
            manaPotential = pharna.manaPotential;
            willPotential = pharna.willPotential;
            experience = pharna.experience;
            manaPool = pharna.manaPool;
            ascendsWithoutRare = pharna.ascendsWithoutRare;
            inscriptionType = pharna.inscriptionType;
            inscriptionAvailable = pharna.inscriptionAvailable;

            feats.Clear();
            if (pharna.feats != null)
            {
                foreach (FeatRecord record in pharna.feats)
                {
                    if (record == null) continue;
                    feats.Add(new FeatRecord
                    {
                        featTraits = record.featTraits == null ? new List<FeatTraitDef>() : new List<FeatTraitDef>(record.featTraits),
                        tier = record.tier,
                        share = record.share,
                        battleMagnitude = record.battleMagnitude,
                        battleName = record.battleName,
                        tick = record.tick,
                        tileId = record.tileId,
                        usedForLevel = record.usedForLevel
                    });
                }
            }

            skills.Clear();
            if (pharna.skills != null)
            {
                foreach (Skill skill in pharna.skills)
                {
                    if (skill != null && !string.IsNullOrEmpty(skill.DefName)) 
                        skills.Add(skill.DefName);
                }
            }

            spells.Clear();
            if (pharna.spells != null)
            {
                foreach (Spell spell in pharna.spells)
                {
                    if (spell != null && !string.IsNullOrEmpty(spell.DefName)) 
                        spells.Add(spell.DefName);
                }
            }

            developableAbilities.Clear();
            if (pharna.developableAbilities != null)
            {
                foreach (DevelopableAbility ability in pharna.developableAbilities)
                {
                    if (ability != null && !string.IsNullOrEmpty(ability.DefName))
                        developableAbilities.Add((ability.DefName, ability.Proficiency));
                }
            }
        }

        public AcceptanceReport Restore(Pawn target)
        {
            if (target == null || target.Dead || target.health?.hediffSet == null)
                return "FamiliaMyth.PharnaLost.RestoreFailed".Translate();
            if (target.GetPharna() != null)
                return "FamiliaMyth.PharnaLost.AlreadyRestored".Translate();

            HediffDef hediffDef = DefDatabase<HediffDef>.GetNamed("Hediff_Pharna", false);
            if (hediffDef == null)
            {
                Log.Error("[FamiliaMyth] Can't find HediffDef Hediff_Pharna, can't restore lost pharna.");
                return "FamiliaMyth.PharnaLost.RestoreFailed".Translate();
            }

            // AddHediff 会走 PostAdd：置 Part 为躯干核心部位，并把殖民者登记进恩惠特效列表
            if (target.health.AddHediff(hediffDef) is not Hediff_Pharna pharna)
            {
                Log.Error("[FamiliaMyth] AddHediff returned an unexpected hediff, can't restore lost pharna.");
                return "FamiliaMyth.PharnaLost.RestoreFailed".Translate();
            }

            ApplyValues(pharna);
            RestoreAbilities(pharna);
            ReconcileMapRegistry(target);
            return true;
        }

        private void ApplyValues(Hediff_Pharna pharna)
        {
            pharna.SetLevelTo(level);
            pharna.strength = strength;
            pharna.endurance = endurance;
            pharna.dexterity = dexterity;
            pharna.agility = agility;
            pharna.mana = mana;
            pharna.will = will;
            pharna.strengthPotential = strengthPotential;
            pharna.endurancePotential = endurancePotential;
            pharna.dexterityPotential = dexterityPotential;
            pharna.agilityPotential = agilityPotential;
            pharna.manaPotential = manaPotential;
            pharna.willPotential = willPotential;
            pharna.experience = experience;
            pharna.manaPool = manaPool;
            pharna.ascendsWithoutRare = ascendsWithoutRare;
            pharna.inscriptionType = inscriptionType;
            pharna.inscriptionAvailable = inscriptionAvailable;
            pharna.feats.AddRange(feats);
        }

        private void RestoreAbilities(Hediff_Pharna pharna)
        {
            foreach (string defName in skills)
            {
                SkillDef def = DefDatabase<SkillDef>.GetNamed(defName, false);
                if (def == null)
                {
                    Log.Warning($"[FamiliaMyth] SkillDef {defName} not found, skipped when restoring lost pharna.");
                    continue;
                }
                def.CreateSkill().Apply(pharna);
            }

            foreach (string defName in spells)
            {
                SpellDef def = DefDatabase<SpellDef>.GetNamed(defName, false);
                if (def == null)
                {
                    Log.Warning($"[FamiliaMyth] SpellDef {defName} not found, skipped when restoring lost pharna.");
                    continue;
                }
                Spell spell = def.CreateSpell();
                spell.Apply(pharna);
                spell.pawn = pharna.pawn;
                if (spell.verb != null) spell.verb.caster = pharna.pawn;
            }

            foreach ((string defName, float proficiency) in developableAbilities)
            {
                DevelopableAbilityDef def = DefDatabase<DevelopableAbilityDef>.GetNamed(defName, false);
                DevelopableAbility ability = def?.CreateAbility();
                if (ability == null)
                {
                    Log.Warning($"[FamiliaMyth] DevelopableAbilityDef {defName} not found, skipped when restoring lost pharna.");
                    continue;
                }
                ability.Apply(pharna, proficiency);
            }
        }

        /// <summary>
        /// 让恩惠特效列表与实际所在地图一致（PostAdd 只会登记到当前地图）
        /// </summary>
        private static void ReconcileMapRegistry(Pawn target)
        {
            if (Find.Maps == null) return;
            foreach (Map map in Find.Maps)
            {
                MapComponent_Pharna component = map.GetComponent<MapComponent_Pharna>();
                if (component == null) continue;
                bool shouldHave = target.MapHeld == map;
                bool has = component.pawnsWithPharna.Contains(target);
                if (shouldHave && !has) component.pawnsWithPharna.Add(target);
                else if (!shouldHave && has) component.pawnsWithPharna.Remove(target);
            }
        }
    }
}
