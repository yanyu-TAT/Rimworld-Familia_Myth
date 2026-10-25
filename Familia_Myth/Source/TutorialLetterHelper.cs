using FamiliaMyth.Pharna;
using RimWorld;
using Verse;

namespace FamiliaMyth.Core
{
    /// <summary>
    /// 教程信件统一出口：一次性标记、开关判断与发送
    /// </summary>
    public static class TutorialLetterHelper
    {
        public const string GrantHint = "grant_hint";
        public const string GrantHintNoMeme = "grant_hint_nomeme";
        public const string IntroGrant = "intro_grant";
        public const string Bottleneck = "bottleneck";
        public const string Ascendable = "ascendable";
        public const string FirstSpell = "first_spell";
        public const string FirstDevAbility = "first_devability";

        //开场信检查间隔（约 41 秒）与开局缓冲（1 天）
        public const int GrantHintCheckInterval = 2500;
        private const int GrantHintDelayTicks = 60000;

        public static void TrySend(string id, Pawn pawn, LookTargets targets = null)
        {
            if (!FamiliaMythSettings.TutorialLettersEnabled) return;

            GameComponent_FamiliaMyth component = GameComponent_FamiliaMyth.Instance;
            if (component == null || component.HasTutorialBeenSent(id)) return;
            if (Find.LetterStack == null) return;

            LetterDef def = DefDatabase<LetterDef>.GetNamed("FamiliaMyth_Tutorial", false) ?? LetterDefOf.NeutralEvent;

            TaggedString label = $"FamiliaMyth.Tutorial.{id}.Label".Translate();
            TaggedString text = $"FamiliaMyth.Tutorial.{id}.Text".Translate(pawn?.Name?.ToStringShort ?? string.Empty);
            LookTargets lookTargets = targets ?? (pawn != null ? new LookTargets(pawn) : null);
            Letter letter = LetterMaker.MakeLetter(label, text, def, lookTargets);
            if (letter == null) return;

            Find.LetterStack.ReceiveLetter(letter);
            component.MarkTutorialSent(id);
        }

        //开场信：尚无恩惠者即发，正文按文化是否含本模组结构二选一
        public static void CheckGrantHint()
        {
            if (Find.TickManager.TicksGame < GrantHintDelayTicks) return;

            GameComponent_FamiliaMyth component = GameComponent_FamiliaMyth.Instance;
            if (component == null) return;
            if (component.HasTutorialBeenSent(GrantHint) || component.HasTutorialBeenSent(GrantHintNoMeme)) return;

            foreach (Map map in Find.Maps)
            {
                foreach (Pawn pawn in map.mapPawns.FreeColonists)
                {
                    if (pawn.GetPharna() != null) return;
                }
            }

            Ideo ideo = Find.FactionManager?.OfPlayer?.ideos?.PrimaryIdeo;
            if (ideo == null) return;

            bool hasRitual = ideo.PreceptsListForReading.Any(p => p.def.defName == "Ritual_GrantPharna");
            TrySend(hasRitual ? GrantHint : GrantHintNoMeme, null);
        }

        //升华条件与仪式内 CanAscend 同源；满级不提示
        public static void TryCheckAscendable(Hediff_Pharna pharna)
        {
            if (pharna == null || pharna.AtLevelCap) return;
            if (!pharna.MeetsStatusRequirement() || !pharna.HasUsableFeat()) return;
            TrySend(Ascendable, pharna.pawn);
        }
    }
}
