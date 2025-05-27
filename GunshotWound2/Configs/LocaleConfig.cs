// ReSharper disable InconsistentNaming
// ReSharper disable UnassignedField.Global due fields are filling with reflection 

namespace GunshotWound2.Configs {
    using System;
    using System.IO;
    using System.Reflection;
    using Leopotam.Localization;

    public sealed class LocaleConfig {
        private Localization localization;

        public string HelmetProtectedYou;
        public string ArmorProtectedYou;
        public string ArmorPenetrated;

        public string YourHealth;
        public string HisHealth;
        public string HerHealth;
        public string Dead;
        public string Pain;
        public string PainIncreasedMessage;
        public string PainDecreasedMessage;
        public string TotallyHealedMessage;

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
        public string TraumaType;

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

        public string YouTryToBandage;
        public string BandageFailed;
        public string BandageSuccess;

        public string ArmorDestroyed;
        public string PainShockDeath;
        public string DeathReason;
        public string BleedingReason;
        public string RunningWithScissors;

        public string FoundItems;
        public string YourInventory;
        public string HandsAreBusy;

        public string TranslationAuthor;

        public (bool success, string reason) TryToLoad(string scriptPath, string language) {
            string path = Path.ChangeExtension(scriptPath, ".csv");
            if (!File.Exists(path)) {
                return (false, "Localization file was not found");
            }

            var doc = new FileInfo(path);
            try {
                LoadLocalization(language, doc.OpenRead());
            } catch (Exception e) {
                return (false, e.Message);
            }

            return (true, null);
        }

        private void LoadLocalization(string language, Stream stream) {
            GswStopped = "Something went wrong with GSW2 :(";

            localization = new Localization(defaultLanguage: "En");
            localization.SetDefaultAsFallback(true);
            localization.SetLanguage(language);

            using var reader = new StreamReader(stream);
            bool success = localization.AddSource(reader.ReadToEnd(), category: "Base");
            if (!success) {
                throw new Exception("Can't parse localization file");
            }

            Type stringType = typeof(string);
            FieldInfo[] fields = typeof(LocaleConfig).GetFields(BindingFlags.Instance | BindingFlags.Public);
            foreach (FieldInfo field in fields) {
                if (field.FieldType == stringType) {
                    string translation = GetTranslation(field.Name);
                    field.SetValue(this, translation);
                }
            }
        }

        public string GetTranslation(string key) {
            return localization.Get(key, "Base").Item1;
        }
    }
}