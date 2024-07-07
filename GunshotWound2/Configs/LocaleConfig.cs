namespace GunshotWound2.Configs {
    using System;
    using System.IO;
    using Leopotam.Localization;

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
        public string ClosedFractureOf;

        public string YourHealth;
        public string HisHealth;
        public string HerHealth;
        public string Dead;
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
        public string GswPauseTip;

        public string AlreadyBandaging;
        public string DontHaveMoneyForBandage;
        public string YouTryToBandage;
        public string BandageFailed;
        public string BandageSuccess;

        public string ArmorDestroyed;
        public string PainShockDeath;
        public string DeathReason;
        public string BleedingReason;

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
            var manager = new Localization(defaultLanguage: "En");
            manager.SetDefaultAsFallback(true);
            manager.SetLanguage(language);

            using var reader = new StreamReader(stream);
            bool success = manager.AddSource(reader.ReadToEnd(), category: "Base");
            if (!success) {
                throw new Exception("Can't parse localization file");
            }

            config.HelmetSavedYourHead = GetBaseTranslation(manager, "HelmetSavedYourHead");
            config.ArmorSavedYourChest = GetBaseTranslation(manager, "ArmorSavedYourChest");
            config.ArmorSavedYourLowerBody = GetBaseTranslation(manager, "ArmorSavedYourLowerBody");
            config.ArmorPenetrated = GetBaseTranslation(manager, "ArmorPenetrated");
            config.ArmorInjury = GetBaseTranslation(manager, "ArmorInjury");

            config.BodyPartHead = GetBaseTranslation(manager, "BodyPartHead");
            config.BodyPartNeck = GetBaseTranslation(manager, "BodyPartNeck");
            config.BodyPartChest = GetBaseTranslation(manager, "BodyPartChest");
            config.BodyPartLowerBody = GetBaseTranslation(manager, "BodyPartLowerBody");
            config.BodyPartArm = GetBaseTranslation(manager, "BodyPartArm");
            config.BodyPartLeg = GetBaseTranslation(manager, "BodyPartLeg");

            config.GrazeWound = GetBaseTranslation(manager, "GrazeWound");

            config.GrazeGswOn = GetBaseTranslation(manager, "GrazeGswOn");
            config.FleshGswOn = GetBaseTranslation(manager, "FleshGswOn");
            config.PenetratingGswOn = GetBaseTranslation(manager, "PenetratingGswOn");
            config.PerforatingGswOn = GetBaseTranslation(manager, "PerforatingGswOn");
            config.AvulsiveGswOn = GetBaseTranslation(manager, "AvulsiveGswOn");

            config.HeavyBrainDamage = GetBaseTranslation(manager, "HeavyBrainDamage");
            config.BulletFlyThroughYourHead = GetBaseTranslation(manager, "BulletFlyThroughYourHead");
            config.BulletTornApartYourBrain = GetBaseTranslation(manager, "BulletTornApartYourBrain");

            config.LightBruise = GetBaseTranslation(manager, "LightBruise");
            config.LightBruiseOn = GetBaseTranslation(manager, "LightBruiseOn");
            config.MediumBruiseOn = GetBaseTranslation(manager, "MediumBruiseOn");
            config.HeavyBruiseOn = GetBaseTranslation(manager, "HeavyBruiseOn");
            config.AbrazionWoundOn = GetBaseTranslation(manager, "AbrazionWoundOn");
            config.WindedFromImpact = GetBaseTranslation(manager, "WindedFromImpact");

            config.IncisionWoundOn = GetBaseTranslation(manager, "IncisionWoundOn");
            config.LacerationWoundOn = GetBaseTranslation(manager, "LacerationWoundOn");
            config.StabWoundOn = GetBaseTranslation(manager, "StabWoundOn");

            config.Blackout = GetBaseTranslation(manager, "Blackout");
            config.BleedingInHead = GetBaseTranslation(manager, "BleedingInHead");
            config.TraumaticBrainInjury = GetBaseTranslation(manager, "TraumaticBrainInjury");
            config.BrokenNeck = GetBaseTranslation(manager, "BrokenNeck");
            config.ClosedFractureOf = GetBaseTranslation(manager, "ClosedFractureOf");

            config.YourHealth = GetBaseTranslation(manager, "YourHealth");
            config.HisHealth = GetBaseTranslation(manager, "HisHealth");
            config.HerHealth = GetBaseTranslation(manager, "HerHealth");
            config.Dead = GetBaseTranslation(manager, "Dead");
            config.Pain = GetBaseTranslation(manager, "Pain");
            config.PainIncreasedMessage = GetBaseTranslation(manager, "PainIncreasedMessage");
            config.PainDecreasedMessage = GetBaseTranslation(manager, "PainDecreasedMessage");
            config.TotallyHealedMessage = GetBaseTranslation(manager, "TotallyHealedMessage");

            config.ArmorLooksGreat = GetBaseTranslation(manager, "ArmorLooksGreat");
            config.ScratchesOnArmor = GetBaseTranslation(manager, "ScratchesOnArmor");
            config.DentsOnArmor = GetBaseTranslation(manager, "DentsOnArmor");
            config.ArmorLooksAwful = GetBaseTranslation(manager, "ArmorLooksAwful");

            config.Crits = GetBaseTranslation(manager, "Crits");
            config.NervesCrit = GetBaseTranslation(manager, "NervesCrit");
            config.HeartCrit = GetBaseTranslation(manager, "HeartCrit");
            config.LungsCrit = GetBaseTranslation(manager, "LungsCrit");
            config.StomachCrit = GetBaseTranslation(manager, "StomachCrit");
            config.GutsCrit = GetBaseTranslation(manager, "GutsCrit");
            config.ArmsCrit = GetBaseTranslation(manager, "ArmsCrit");
            config.LegsCrit = GetBaseTranslation(manager, "LegsCrit");

            config.Wounds = GetBaseTranslation(manager, "Wounds");

            config.DontHaveMoneyForHelmet = GetBaseTranslation(manager, "DontHaveMoneyForHelmet");

            config.InternalBleeding = GetBaseTranslation(manager, "InternalBleeding");
            config.SeveredArtery = GetBaseTranslation(manager, "SeveredArtery");
            config.SeveredArteryMessage = GetBaseTranslation(manager, "SeveredArteryMessage");

            config.PlayerNervesCritMessage = GetBaseTranslation(manager, "PlayerNervesCritMessage");
            config.ManNervesCritMessage = GetBaseTranslation(manager, "ManNervesCritMessage");
            config.WomanNervesCritMessage = GetBaseTranslation(manager, "WomanNervesCritMessage");

            config.PlayerHeartCritMessage = GetBaseTranslation(manager, "PlayerHeartCritMessage");
            config.ManHeartCritMessage = GetBaseTranslation(manager, "ManHeartCritMessage");
            config.WomanHeartCritMessage = GetBaseTranslation(manager, "WomanHeartCritMessage");

            config.PlayerLungsCritMessage = GetBaseTranslation(manager, "PlayerLungsCritMessage");
            config.ManLungsCritMessage = GetBaseTranslation(manager, "ManLungsCritMessage");
            config.WomanLungsCritMessage = GetBaseTranslation(manager, "WomanLungsCritMessage");

            config.PlayerStomachCritMessage = GetBaseTranslation(manager, "PlayerStomachCritMessage");
            config.ManStomachCritMessage = GetBaseTranslation(manager, "ManStomachCritMessage");
            config.WomanStomachCritMessage = GetBaseTranslation(manager, "WomanStomachCritMessage");

            config.PlayerGutsCritMessage = GetBaseTranslation(manager, "PlayerGutsCritMessage");
            config.ManGutsCritMessage = GetBaseTranslation(manager, "ManGutsCritMessage");
            config.WomanGutsCritMessage = GetBaseTranslation(manager, "WomanGutsCritMessage");

            config.PlayerArmsCritMessage = GetBaseTranslation(manager, "PlayerArmsCritMessage");
            config.ManArmsCritMessage = GetBaseTranslation(manager, "ManArmsCritMessage");
            config.WomanArmsCritMessage = GetBaseTranslation(manager, "WomanArmsCritMessage");

            config.PlayerLegsCritMessage = GetBaseTranslation(manager, "PlayerLegsCritMessage");
            config.ManLegsCritMessage = GetBaseTranslation(manager, "ManLegsCritMessage");
            config.WomanLegsCritMessage = GetBaseTranslation(manager, "WomanLegsCritMessage");

            config.UnbearablePainMessage = GetBaseTranslation(manager, "UnbearablePainMessage");

            config.AddingRange = GetBaseTranslation(manager, "AddingRange");
            config.RemovingRange = GetBaseTranslation(manager, "RemovingRange");

            config.ThanksForUsing = GetBaseTranslation(manager, "ThanksForUsing");
            config.GswStopped = GetBaseTranslation(manager, "GswStopped");
            config.GswIsPaused = GetBaseTranslation(manager, "GswIsPaused");
            config.GswIsWorking = GetBaseTranslation(manager, "GswIsWorking");
            config.GswPauseTip = GetBaseTranslation(manager, "GswPauseTip");

            config.AlreadyBandaging = GetBaseTranslation(manager, "AlreadyBandaging");
            config.DontHaveMoneyForBandage = GetBaseTranslation(manager, "DontHaveMoneyForBandage");
            config.YouTryToBandage = GetBaseTranslation(manager, "YouTryToBandage");
            config.BandageFailed = GetBaseTranslation(manager, "BandageFailed");
            config.BandageSuccess = GetBaseTranslation(manager, "BandageSuccess");

            config.ArmorDestroyed = GetBaseTranslation(manager, "ArmorDestroyed");
            config.PainShockDeath = GetBaseTranslation(manager, "PainShockDeath");
            config.DeathReason = GetBaseTranslation(manager, "DeathReason");
            config.BleedingReason = GetBaseTranslation(manager, "BleedingReason");

            config.LocalizationAuthor = GetBaseTranslation(manager, "TranslationAuthor");
        }

        private static string GetBaseTranslation(Localization localization, string key) {
            return localization.Get(key, "Base").Item1;
        }
    }
}