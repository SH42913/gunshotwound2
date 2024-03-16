namespace GunshotWound2.Configs {
    using System;
    using System.IO;
    using Utils;

    public sealed class LocaleConfig {
        public string HelmetSavedYourHead;
        public string ArmorSavedYourChest;
        public string ArmorSavedYourLowerBody;
        public string ArmorPenetrated;
        public string ArmorInjury;

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

        public string Blackout;
        public string BleedingInHead;
        public string TraumaticBrainInjury;
        public string BrokenNeck;

        public string Health;
        public string YouAreDead;
        public string Pain;
        public string PainIncreasedMessage;
        public string PainDecreasedMessage;
        public string TotallyHealedMessage;

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
        public string DeathReason;

        public string LocalizationAuthor;

        public static (bool success, string reason) TryToLoad(LocaleConfig config, string language) {
            var gswLocalization = new FileInfo("scripts/GSW2/GSW2Localization.csv");
            var scriptsLocalization = new FileInfo("scripts/GSW2Localization.csv");
            if (!gswLocalization.Exists && !scriptsLocalization.Exists) {
                return (false, "Localization file was not found");
            }

            var doc = gswLocalization.Exists ? gswLocalization : scriptsLocalization;
            try {
                LoadLocalization(config, language, doc.OpenRead());
            } catch (Exception e) {
                return (false, e.Message);
            }

            return (true, null);
        }

        private static void LoadLocalization(LocaleConfig config, string language, Stream stream) {
            var manager = new LocalizationManager(stream);
            manager.SetLanguage(language);

            config.HelmetSavedYourHead = manager.GetWord("HelmetSavedYourHead");
            config.ArmorSavedYourChest = manager.GetWord("ArmorSavedYourChest");
            config.ArmorSavedYourLowerBody = manager.GetWord("ArmorSavedYourLowerBody");
            config.ArmorPenetrated = manager.GetWord("ArmorPenetrated");
            config.ArmorInjury = manager.GetWord("ArmorInjury");

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

            config.Blackout = manager.GetWord("Blackout");
            config.BleedingInHead = manager.GetWord("BleedingInHead");
            config.TraumaticBrainInjury = manager.GetWord("TraumaticBrainInjury");
            config.BrokenNeck = manager.GetWord("BrokenNeck");

            config.Health = manager.GetWord("Health");
            config.YouAreDead = manager.GetWord("YouAreDead");
            config.Pain = manager.GetWord("Pain");
            config.PainIncreasedMessage = manager.GetWord("PainIncreasedMessage");
            config.PainDecreasedMessage = manager.GetWord("PainDecreasedMessage");
            config.TotallyHealedMessage = manager.GetWord("TotallyHealedMessage");

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
            config.PainShockDeath = manager.GetWord("PainShockDeath");
            config.DeathReason = manager.GetWord("DeathReason");

            config.LocalizationAuthor = manager.GetWord("TranslationAuthor");
        }
    }
}