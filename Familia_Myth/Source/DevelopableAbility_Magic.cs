using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    [StaticConstructorOnStartup]
    public class DevelopableAbility_Magic : DevelopableAbility, ISpellPowerModifier
    {
        private static readonly Texture2D AutoStoreTexture = ContentFinder<Texture2D>.Get("Familia_Myth/UI/Commands/PsyfocusAutoStore", true);
        private static readonly Texture2D ResotreTexture = ContentFinder<Texture2D>.Get("Familia_Myth/UI/Commands/PsyfocusRestore", true);

        public float storedPsyfocus;
        public bool autoStore = true;
        public static bool IsAutoStoring;
        private const float StoreThreshold = .5f;
        private const float PsyfocusStoredBase = 0.5f;
        private const float PsyfocusStoredPerProf = 0.005f;
        private const float EntropyRecoveryPerProf = 0.0005f;
        private const float SpellPowerPerProf = 0.0025f;
        public const float ProfGainPerEntropy = 0.05f;
        public const float ProfGainPerPsyfocus = 5f;
        public float MaxStoredPsyfocus => PsyfocusStoredBase + proficiency * PsyfocusStoredPerProf;

        public override bool ApplyModifiers(StatDef stat, ref float value)
        {
            if (pharna == null || stat == null) return false;
            if (stat.defName == "PsychicEntropyRecoveryRate"){
                value += proficiency * EntropyRecoveryPerProf;
                return true;
            }
            return false;
        }

        public void ApplySpellPowerModifier(ref float power)
        {
            power += proficiency * SpellPowerPerProf;
        }

        public override string GetExplanation(StatDef stat)
        {
            if (pharna == null || stat == null) 
                return "";
            string label = "FamiliaMyth.DevelopableAbility.MagicLabel".Translate();
            string str = "Lv." + pharna.level.ToString()+ " " + label + Rank + ": ";
            if (stat.defName == "PsychicEntropyRecoveryRate"){
                str += "+" + (proficiency * EntropyRecoveryPerProf).ToString("F3") + "/s";
                return str;
            }

            return "";
        }

        public override void Tick()
        {
            if (!autoStore) return;
            var tracker = pharna?.pawn?.psychicEntropy;
            if (tracker == null) return;

            if (tracker.CurrentPsyfocus > StoreThreshold && storedPsyfocus < MaxStoredPsyfocus)
            {
                float excess = tracker.CurrentPsyfocus - StoreThreshold;
                float room = MaxStoredPsyfocus - storedPsyfocus;
                float store = Mathf.Min(excess, room);

                if (store > 0.0005f)
                {
                    //标记为自动存储
                    IsAutoStoring = true;
                    tracker.OffsetPsyfocusDirectly(-store); //消耗对应量的精神力
                    IsAutoStoring = false;
                    storedPsyfocus += store;
                }
            }
        }

        private void RestorePsyfocus()
        {
            var tracker = pharna?.pawn?.psychicEntropy;
            if (tracker == null || storedPsyfocus <= 0) return;

            float restore = Mathf.Min(storedPsyfocus, 1f -  tracker.CurrentPsyfocus);
            if (restore <= 0f) return;

            tracker.OffsetPsyfocusDirectly(restore);
            storedPsyfocus -= restore;
            //关闭自动存储
            if (autoStore)
                Messages.Message(new Message("FamiliaMyth.DevelopableAbility.MagicAutoStoreOffMessage".Translate(), MessageTypeDefOf.TaskCompletion));
            autoStore = false;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            yield return new Gizmo_PsyfocusStored(this);
            yield return new Command_Action
            {
                defaultLabel = "FamiliaMyth.DevelopableAbility.MagicRestorePsyfocus".Translate(),
                defaultDesc = "FamiliaMyth.DevelopableAbility.MagicRestorePsyfocusDesc".Translate(),
                action = RestorePsyfocus,
                disabledReason = "FamiliaMyth.DevelopableAbility.MagicRestorePsyfocusDisabled".Translate(),
                Disabled = storedPsyfocus <= 0f,
                icon = ResotreTexture
            };
            yield return new Command_Toggle
            {
                defaultLabel = "FamiliaMyth.DevelopableAbility.MagicAutoStoreLabel".Translate(),
                defaultDesc = "FamiliaMyth.DevelopableAbility.MagicAutoStoreDesc".Translate(),
                isActive = () => autoStore,
                toggleAction = () => autoStore = !autoStore,
                icon = AutoStoreTexture
            };
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref storedPsyfocus, "storedPsyfocus");
            Scribe_Values.Look(ref autoStore, "autoStore");
        }
    }

    [StaticConstructorOnStartup]
    public class Gizmo_PsyfocusStored : Gizmo
    {
        private readonly DevelopableAbility_Magic magic;
        private static readonly Texture2D BarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.34f, 0.42f, 0.43f));
        private static readonly Texture2D EmptyBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.03f, 0.035f, 0.05f));

        public Gizmo_PsyfocusStored(DevelopableAbility_Magic ability)
        {
            this.magic = ability;
            Order = -90;
        }

        public override float GetWidth(float maxWidth) => 212f;

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect rect = new(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
            Rect r2 = rect.ContractedBy(6f);
            Widgets.DrawWindowBackground(rect);
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(r2.x, r2.y + 6f, r2.width, Text.LineHeight), "FamiliaMyth.DevelopableAbility.MagicStoreBarTitle".Translate());

            //进度条
            Rect bar = new(r2.x, r2.y + 32f, r2.width - 10f, 22f);
            float ratio = magic.MaxStoredPsyfocus > 0f ? magic.storedPsyfocus / magic.MaxStoredPsyfocus : 0f;
            Widgets.FillableBar(bar, Mathf.Clamp01(ratio), BarTex, EmptyBarTex, doBorder: true);
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(bar, magic.storedPsyfocus.ToStringPercent("0.#") + " / " + magic.MaxStoredPsyfocus.ToStringPercent("0.#"));
            Text.Anchor = TextAnchor.UpperLeft;

            //悬停提示
            TooltipHandler.TipRegion(bar, () =>
            {
                string tip = "FamiliaMyth.DevelopableAbility.MagicStoreTooltip".Translate(
                    magic.storedPsyfocus.ToStringPercent("0.#"), magic.MaxStoredPsyfocus.ToStringPercent("0.#"));
                return tip;
            },
                Gen.HashCombineInt(magic.GetHashCode(), 133873));

            return new GizmoResult(GizmoState.Clear);
        }
    }
}