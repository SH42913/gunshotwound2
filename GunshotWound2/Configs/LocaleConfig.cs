namespace GunshotWound2.Configs
{
    public class LocaleConfig
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

        public static void FillWithDefaultValues(LocaleConfig config)
        {
            config.HelmetSavedYourHead = "Helmet saved your head";
            config.ArmorSavedYourChest = "Armor saved your chest";
            config.ArmorSavedYourLowerBody = "Armor saved your lower body";
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
            config.BulletFlyThroughYourHead = "Bullet fly through your head";
            config.BulletTornApartYourBrain = "Bullet torn apart your brain";

            config.LightBruise = "Light bruise";
            config.LightBruiseOn = "Light bruise on";
            config.MediumBruiseOn = "Medium bruise on";
            config.HeavyBruiseOn = "Heavy bruise on";
            config.AbrazionWoundOn = "Abrazion wound on";
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
            config.BleedingInHead = "Bleeding in the head";
            config.TraumaticBrainInjury = "Traumatic brain injury";
            config.BrokenNeck = "Broken neck";

            config.Health = "Health";
            config.YouAreDead = "You are dead!";
            config.Pain = "Pain";

            config.ArmorLooksGreat = "Your armor looks great";
            config.ScratchesOnArmor = "Your armor has some scratches";
            config.DentsOnArmor = "Your armor has large dents";
            config.ArmorLooksAwful = "Your armor looks awful";

            config.Crits = "Critical damaged";
            config.NervesCrit = "nerves";
            config.HeartCrit = "heart";
            config.LungsCrit = "lungs";
            config.StomachCrit = "stomach";
            config.GutsCrit = "guts";
            config.ArmsCrit = "arms";
            config.LegsCrit = "legs";

            config.Wounds = "Wounds";

            config.DontHaveMoneyForHelmet = "You don't have enough money to buy helmet";

            config.InternalBleeding = "Internal bleeding";
            config.SeveredArtery = "Severed artery";
            config.SeveredArteryMessage = "Artery was severed!";

            config.PlayerNervesCritMessage = "You feel you can't control your arms and legs anymore";
            config.ManNervesCritMessage = "He looks like his spine was damaged";
            config.WomanNervesCritMessage = "She looks like her spine was damaged";

            config.PlayerHeartCritMessage = "You feel awful pain in your chest";
            config.ManHeartCritMessage = "He coughs up blood";
            config.WomanHeartCritMessage = "She coughs up blood";

            config.PlayerLungsCritMessage = "It's very hard for you to breathe";
            config.ManLungsCritMessage = "He coughs up blood";
            config.WomanLungsCritMessage = "She coughs up blood";

            config.PlayerStomachCritMessage = "You feel yourself very sick";
            config.ManStomachCritMessage = "He looks very sick";
            config.WomanStomachCritMessage = "She looks very sick";

            config.PlayerGutsCritMessage = "You feel yourself very sick";
            config.ManGutsCritMessage = "He looks very sick";
            config.WomanGutsCritMessage = "She looks very sick";

            config.PlayerArmsCritMessage = "It's looks like arm bone was broken";
            config.ManArmsCritMessage = "His arm looks very bad";
            config.WomanArmsCritMessage = "Her arm looks very bad";

            config.PlayerLegsCritMessage = "It's looks like leg bone was broken";
            config.ManLegsCritMessage = "His leg looks very bad";
            config.WomanLegsCritMessage = "Her leg looks very bad";

            config.UnbearablePainMessage = "You got a pain shock! You lose consciousness!";

            config.AddingRange = "Adding range";
            config.RemovingRange = "Removing range";

            config.ThanksForUsing = "Thanks for using";
            config.GswStopped = "GSW2 stopped, sorry :(";
            config.GswIsPaused = "GSW2 is paused";
            config.GswIsWorking = "GSW2 is working";

            config.AlreadyBandaging = "You're already bandaging";
            config.DontHaveMoneyForBandage = "You don't have enough money for bandage";
            config.YouTryToBandage = "You try to bandage self. You need to stand still for {0} seconds!";
            config.BandageFailed = "Bandaging is failed. You need to stand still for apply bandage!";
            config.BandageSuccess = "You applied bandage to {0}";

            config.ArmorDestroyed = "Your armor fall apart";

            config.PainShockDeath = "You have dead from pain shock";

            config.LocalizationAuthor = "~r~SH42913";
        }
    }
}