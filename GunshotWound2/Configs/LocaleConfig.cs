using System;
using System.IO;
using GunshotWound2.Utils;

namespace GunshotWound2.Configs
{
    public sealed class LocaleConfig
    {
        public string HelmetSavedYourHead;
        public string ArmorSavedYourChest;
        public string ArmorSavedYourLowerBody;
        public string ArmorPenetrated;

        public string BodyPartHead;
        public string BodyPartNeck;
        public string BodyPartChest;
        public string BodyPartLowerBody;
        public string BodyPartArm;
        public string BodyPartLeg;

        public string GrazeWound;
        public string GrazeGswOn;
        public string FleshGswOn;
        public string PenetratingGswOn;
        public string PerforatingGswOn;
        public string AvulsiveGswOn;

        public string HeavyBrainDamage;
        public string BulletFlyThroughYourHead;
        public string BulletTornApartYourBrain;

        public string LightBruise;
        public string LightBruiseOn;
        public string MediumBruiseOn;
        public string HeavyBruiseOn;
        public string AbrazionWoundOn;
        public string WindedFromImpact;

        public string IncisionWoundOn;
        public string LacerationWoundOn;
        public string StabWoundOn;

        public string BodyBlown;
        public string HeadBlown;
        public string NeckBlown;
        public string ChestBlown;
        public string LowerBodyBlown;
        public string ArmBlown;
        public string LegBlown;

        public string Blackout;
        public string BleedingInHead;
        public string TraumaticBrainInjury;
        public string BrokenNeck;

        public string Health;
        public string YouAreDead;
        public string Pain;

        public string ArmorLooksGreat;
        public string ScratchesOnArmor;
        public string DentsOnArmor;
        public string ArmorLooksAwful;

        public string Crits;
        public string NervesCrit;
        public string HeartCrit;
        public string LungsCrit;
        public string StomachCrit;
        public string GutsCrit;
        public string ArmsCrit;
        public string LegsCrit;

        public string Wounds;

        public string DontHaveMoneyForHelmet;

        public string InternalBleeding;
        public string SeveredArtery;
        public string SeveredArteryMessage;

        public string PlayerNervesCritMessage;
        public string ManNervesCritMessage;
        public string WomanNervesCritMessage;

        public string PlayerHeartCritMessage;
        public string ManHeartCritMessage;
        public string WomanHeartCritMessage;

        public string PlayerLungsCritMessage;
        public string ManLungsCritMessage;
        public string WomanLungsCritMessage;

        public string PlayerStomachCritMessage;
        public string ManStomachCritMessage;
        public string WomanStomachCritMessage;

        public string PlayerGutsCritMessage;
        public string ManGutsCritMessage;
        public string WomanGutsCritMessage;

        public string PlayerArmsCritMessage;
        public string ManArmsCritMessage;
        public string WomanArmsCritMessage;

        public string PlayerLegsCritMessage;
        public string ManLegsCritMessage;
        public string WomanLegsCritMessage;

        public string UnbearablePainMessage;

        public string AddingRange;
        public string RemovingRange;

        public string ThanksForUsing;
        public string GswStopped;
        public string GswIsPaused;
        public string GswIsWorking;

        public string AlreadyBandaging;
        public string DontHaveMoneyForBandage;
        public string YouTryToBandage;
        public string BandageFailed;
        public string BandageSuccess;

        public string ArmorDestroyed;

        public string PainShockDeath;

        public string LocalizationAuthor;

        private static void FillWithDefaultValues(LocaleConfig config)
        {
            config.HelmetSavedYourHead = "Your helmet saved your head";
            config.ArmorSavedYourChest = "Your body armor has protected you from a chest injury";
            config.ArmorSavedYourLowerBody = "Your body armor has protected you from a lower body injury";
            config.ArmorPenetrated = "Your armor was penetrated";

            config.BodyPartHead = "head";
            config.BodyPartNeck = "neck";
            config.BodyPartChest = "chest";
            config.BodyPartLowerBody = "lower body";
            config.BodyPartArm = "arm";
            config.BodyPartLeg = "leg";

            config.GrazeWound = "Graze wound";

            config.GrazeGswOn = "Graze GSW on";
            config.FleshGswOn = "Flesh GSW on";
            config.PenetratingGswOn = "Penetrating GSW on";
            config.PerforatingGswOn = "Perforating GSW on";
            config.AvulsiveGswOn = "Avulsive GSW on";

            config.HeavyBrainDamage = "Heavy brain damage";
            config.BulletFlyThroughYourHead = "A bullet has penetrated your skull";
            config.BulletTornApartYourBrain = "A bullet has fractured your skull";

            config.LightBruise = "Light bruise";
            config.LightBruiseOn = "Light bruise on";
            config.MediumBruiseOn = "Medium bruise on";
            config.HeavyBruiseOn = "Heavy bruise on";
            config.AbrazionWoundOn = "Abrasion wound on";
            config.WindedFromImpact = "Winded from impact";

            config.IncisionWoundOn = "Incision wound on";
            config.LacerationWoundOn = "Laceration wound on";
            config.StabWoundOn = "Stab wound on";

            config.BodyBlown = "Body blown";
            config.HeadBlown = "Head blown";
            config.NeckBlown = "Neck blown";
            config.ChestBlown = "Chest blown";
            config.LowerBodyBlown = "Lower body blown";
            config.ArmBlown = "Arm blown";
            config.LegBlown = "Leg blown";

            config.Blackout = "Blackout possible";
            config.BleedingInHead = "Intracranial bleeding";
            config.TraumaticBrainInjury = "Traumatic brain injury";
            config.BrokenNeck = "Damage to cervical vertebrae";

            config.Health = "Health";
            config.YouAreDead = "You are dead!";
            config.Pain = "Pain";

            config.ArmorLooksGreat = "Your armor appears undamaged";
            config.ScratchesOnArmor = "Some plates of your armor is broken";
            config.DentsOnArmor = "Your armor is noticeably damaged";
            config.ArmorLooksAwful = "Your body armor is practically useless";

            config.Crits = "Critical damaged";
            config.NervesCrit = "nerves";
            config.HeartCrit = "heart";
            config.LungsCrit = "lungs";
            config.StomachCrit = "stomach";
            config.GutsCrit = "guts";
            config.ArmsCrit = "arms";
            config.LegsCrit = "legs";

            config.Wounds = "Wounds";

            config.DontHaveMoneyForHelmet = "Insufficient funds to purchase a helmet";

            config.InternalBleeding = "Internal bleeding";
            config.SeveredArtery = "Punctured artery";
            config.SeveredArteryMessage = "An artery was severed";

            config.PlayerNervesCritMessage = "You can't feel your limbs";
            config.ManNervesCritMessage = "He looks like his spine was damaged";
            config.WomanNervesCritMessage = "She looks like her spine was damaged";

            config.PlayerHeartCritMessage = "You feel like life is leaving you";
            config.ManHeartCritMessage = "He coughs up blood";
            config.WomanHeartCritMessage = "She coughs up blood";

            config.PlayerLungsCritMessage = "You cough up blood";
            config.ManLungsCritMessage = "He coughs up blood";
            config.WomanLungsCritMessage = "She coughs up blood";

            config.PlayerStomachCritMessage = "You feel yourself very sick";
            config.ManStomachCritMessage = "He looks very sick";
            config.WomanStomachCritMessage = "She looks very sick";

            config.PlayerGutsCritMessage = "You can see your guts";
            config.ManGutsCritMessage = "He looks very sick";
            config.WomanGutsCritMessage = "She looks very sick";

            config.PlayerArmsCritMessage = "You feel awful pain in your arm";
            config.ManArmsCritMessage = "His arm looks broken";
            config.WomanArmsCritMessage = "Her arm looks broken";

            config.PlayerLegsCritMessage = "You feel awful pain in your leg";
            config.ManLegsCritMessage = "His leg looks broken";
            config.WomanLegsCritMessage = "Her leg looks broken";

            config.UnbearablePainMessage = "You lose consciousness from the overwhelming pain";

            config.AddingRange = "Increasing range";
            config.RemovingRange = "Decreasing range";

            config.ThanksForUsing = "Thanks for using";
            config.GswStopped = "GSW2 operations have ended :(";
            config.GswIsPaused = "GSW2 is paused";
            config.GswIsWorking = "GSW2 is working";

            config.AlreadyBandaging = "You're already bandaging yourself";
            config.DontHaveMoneyForBandage = "You don't have enough money for a bandage";
            config.YouTryToBandage = "You try to bandage self. You need to stand still for {0} seconds!";
            config.BandageFailed = "Bandaging has failed. You need to stand still to apply a bandage!";
            config.BandageSuccess = "You applied bandage to {0}";

            config.ArmorDestroyed = "Your armor falls apart";

            config.PainShockDeath = "You have died of shock";

            config.LocalizationAuthor = "~r~SH42913";
        }

        public static (bool success, string reason) TryToLoadLocalization(LocaleConfig config, string language)
        {
            var gswLocalization = new FileInfo("scripts/GSW2/GSW2Localization.csv");
            var scriptsLocalization = new FileInfo("scripts/GSW2Localization.csv");
            if (!gswLocalization.Exists && !scriptsLocalization.Exists)
            {
                return (false, "Localization file was not found");
            }

            var doc = gswLocalization.Exists
                ? gswLocalization
                : scriptsLocalization;

            try
            {
                LoadLocalization(config, language, doc.OpenRead());
            }
            catch (Exception e)
            {
                FillWithDefaultValues(config);
                return (false, e.Message);
            }

            return (true, null);
        }

        private static void LoadLocalization(LocaleConfig config, string language, Stream stream)
        {
            var manager = new LocalizationManager(stream);
            manager.SetLanguage(language);

            config.HelmetSavedYourHead = manager.GetWord("HelmetSavedYourHead");
            config.ArmorSavedYourChest = manager.GetWord("ArmorSavedYourChest");
            config.ArmorSavedYourLowerBody = manager.GetWord("ArmorSavedYourLowerBody");
            config.ArmorPenetrated = manager.GetWord("ArmorPenetrated");

            config.BodyPartHead = manager.GetWord("BodyPartHead");
            config.BodyPartNeck = manager.GetWord("BodyPartNeck");
            config.BodyPartChest = manager.GetWord("BodyPartChest");
            config.BodyPartLowerBody = manager.GetWord("BodyPartLowerBody");
            config.BodyPartArm = manager.GetWord("BodyPartArm");
            config.BodyPartLeg = manager.GetWord("BodyPartLeg");

            config.GrazeWound = manager.GetWord("GrazeWound");

            config.GrazeGswOn = manager.GetWord("GrazeGswOn");
            config.FleshGswOn = manager.GetWord("FleshGswOn");
            config.PenetratingGswOn = manager.GetWord("PenetratingGswOn");
            config.PerforatingGswOn = manager.GetWord("PerforatingGswOn");
            config.AvulsiveGswOn = manager.GetWord("AvulsiveGswOn");

            config.HeavyBrainDamage = manager.GetWord("HeavyBrainDamage");
            config.BulletFlyThroughYourHead = manager.GetWord("BulletFlyThroughYourHead");
            config.BulletTornApartYourBrain = manager.GetWord("BulletTornApartYourBrain");

            config.LightBruise = manager.GetWord("LightBruise");
            config.LightBruiseOn = manager.GetWord("LightBruiseOn");
            config.MediumBruiseOn = manager.GetWord("MediumBruiseOn");
            config.HeavyBruiseOn = manager.GetWord("HeavyBruiseOn");
            config.AbrazionWoundOn = manager.GetWord("AbrazionWoundOn");
            config.WindedFromImpact = manager.GetWord("WindedFromImpact");

            config.IncisionWoundOn = manager.GetWord("IncisionWoundOn");
            config.LacerationWoundOn = manager.GetWord("LacerationWoundOn");
            config.StabWoundOn = manager.GetWord("StabWoundOn");

            config.BodyBlown = manager.GetWord("BodyBlown");
            config.HeadBlown = manager.GetWord("HeadBlown");
            config.NeckBlown = manager.GetWord("NeckBlown");
            config.ChestBlown = manager.GetWord("ChestBlown");
            config.LowerBodyBlown = manager.GetWord("LowerBodyBlown");
            config.ArmBlown = manager.GetWord("ArmBlown");
            config.LegBlown = manager.GetWord("LegBlown");

            config.Blackout = manager.GetWord("Blackout");
            config.BleedingInHead = manager.GetWord("BleedingInHead");
            config.TraumaticBrainInjury = manager.GetWord("TraumaticBrainInjury");
            config.BrokenNeck = manager.GetWord("BrokenNeck");

            config.Health = manager.GetWord("Health");
            config.YouAreDead = manager.GetWord("YouAreDead");
            config.Pain = manager.GetWord("Pain");

            config.ArmorLooksGreat = manager.GetWord("ArmorLooksGreat");
            config.ScratchesOnArmor = manager.GetWord("ScratchesOnArmor");
            config.DentsOnArmor = manager.GetWord("DentsOnArmor");
            config.ArmorLooksAwful = manager.GetWord("ArmorLooksAwful");

            config.Crits = manager.GetWord("Crits");
            config.NervesCrit = manager.GetWord("NervesCrit");
            config.HeartCrit = manager.GetWord("HeartCrit");
            config.LungsCrit = manager.GetWord("LungsCrit");
            config.StomachCrit = manager.GetWord("StomachCrit");
            config.GutsCrit = manager.GetWord("GutsCrit");
            config.ArmsCrit = manager.GetWord("ArmsCrit");
            config.LegsCrit = manager.GetWord("LegsCrit");

            config.Wounds = manager.GetWord("Wounds");

            config.DontHaveMoneyForHelmet = manager.GetWord("DontHaveMoneyForHelmet");

            config.InternalBleeding = manager.GetWord("InternalBleeding");
            config.SeveredArtery = manager.GetWord("SeveredArtery");
            config.SeveredArteryMessage = manager.GetWord("SeveredArteryMessage");

            config.PlayerNervesCritMessage = manager.GetWord("PlayerNervesCritMessage");
            config.ManNervesCritMessage = manager.GetWord("ManNervesCritMessage");
            config.WomanNervesCritMessage = manager.GetWord("WomanNervesCritMessage");

            config.PlayerHeartCritMessage = manager.GetWord("PlayerHeartCritMessage");
            config.ManHeartCritMessage = manager.GetWord("ManHeartCritMessage");
            config.WomanHeartCritMessage = manager.GetWord("WomanHeartCritMessage");

            config.PlayerLungsCritMessage = manager.GetWord("PlayerLungsCritMessage");
            config.ManLungsCritMessage = manager.GetWord("ManLungsCritMessage");
            config.WomanLungsCritMessage = manager.GetWord("WomanLungsCritMessage");

            config.PlayerStomachCritMessage = manager.GetWord("PlayerStomachCritMessage");
            config.ManStomachCritMessage = manager.GetWord("ManStomachCritMessage");
            config.WomanStomachCritMessage = manager.GetWord("WomanStomachCritMessage");

            config.PlayerGutsCritMessage = manager.GetWord("PlayerGutsCritMessage");
            config.ManGutsCritMessage = manager.GetWord("ManGutsCritMessage");
            config.WomanGutsCritMessage = manager.GetWord("WomanGutsCritMessage");

            config.PlayerArmsCritMessage = manager.GetWord("PlayerArmsCritMessage");
            config.ManArmsCritMessage = manager.GetWord("ManArmsCritMessage");
            config.WomanArmsCritMessage = manager.GetWord("WomanArmsCritMessage");

            config.PlayerLegsCritMessage = manager.GetWord("PlayerLegsCritMessage");
            config.ManLegsCritMessage = manager.GetWord("ManLegsCritMessage");
            config.WomanLegsCritMessage = manager.GetWord("WomanLegsCritMessage");

            config.UnbearablePainMessage = manager.GetWord("UnbearablePainMessage");

            config.AddingRange = manager.GetWord("AddingRange");
            config.RemovingRange = manager.GetWord("RemovingRange");

            config.ThanksForUsing = manager.GetWord("ThanksForUsing");
            config.GswStopped = manager.GetWord("GswStopped");
            config.GswIsPaused = manager.GetWord("GswIsPaused");
            config.GswIsWorking = manager.GetWord("GswIsWorking");

            config.AlreadyBandaging = manager.GetWord("AlreadyBandaging");
            config.DontHaveMoneyForBandage = manager.GetWord("DontHaveMoneyForBandage");
            config.YouTryToBandage = manager.GetWord("YouTryToBandage");
            config.BandageFailed = manager.GetWord("BandageFailed");
            config.BandageSuccess = manager.GetWord("BandageSuccess");

            config.ArmorDestroyed = manager.GetWord("ArmorDestroyed");

            config.LocalizationAuthor = manager.GetWord("TranslationAuthor");
        }
    }
}