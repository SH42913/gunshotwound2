// ––‒‒–‒–––‒–‒‒––‒–‒‒––‒–‒–––‒–‒––––‒––––––‒‒‒‒––––‒–‒––‒––––‒––‒‒–‒–––‒–‒
// Коммерческая лицензия подписчика
// (c) 2023 Leopotam <leopotam@yandex.ru>
// ––‒‒–‒–––‒–‒‒––‒–‒‒––‒–‒–––‒–‒––––‒––––––‒‒‒‒––––‒–‒––‒––––‒––‒‒–‒–––‒–‒

#if ENABLE_IL2CPP
using System;
using Unity.IL2CPP.CompilerServices;
#endif

namespace Leopotam.Localization {
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using Serialization.Csv;
#if ENABLE_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif

    public class Localization {
        public const string HeaderKey = "KEY";

        readonly Dictionary<string, Category> _categories;
        readonly List<ILocalizationListener> _listeners;
        readonly string _defaultLanguage;
        bool _defaultAsFallback;
        string _language;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Localization(string defaultLanguage) {
            _categories = new(32);
            _listeners = new(64);
            _language = defaultLanguage;
            _defaultLanguage = defaultLanguage;
            _defaultAsFallback = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Localization SetDefaultAsFallback(bool state) {
            _defaultAsFallback = state;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Localization SetLanguage(string lang) {
            _language = lang;
            foreach (var l in _listeners) {
                l.OnLanguageChanged();
            }

            return this;
        }

        public string Language() {
            return _language;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (string, bool) Get(string token, string category) {
            return GetFor(token, category, _language);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (string, bool) GetFor(string token, string category, string lang) {
            var (v, ok) = GetForInternal(token, category, lang);
            if (!ok && _defaultAsFallback && string.CompareOrdinal(lang, _defaultLanguage) != 0) {
                (v, ok) = GetForInternal(token, category, _defaultLanguage);
            }

            return (v, ok);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        (string, bool) GetForInternal(string token, string category, string lang) {
            if (_categories.TryGetValue(category, out var cat)) {
                if (cat.Langs.TryGetValue(lang, out var langIdx)) {
                    if (cat.Tokens.TryGetValue(token, out var line)) {
                        var v = line[langIdx];
                        return !string.IsNullOrEmpty(v) ? (v, true) : (token, false);
                    }
                }
            }

            return (token, false);
        }

        public bool AddSource(string csv, string category) {
            var (cat, ok) = ParseCategory(csv);
            if (!ok) {
                return false;
            }

            _categories[category] = cat;
            return true;
        }

        public Localization ClearSources() {
            _categories.Clear();
            return this;
        }

        public Localization AddListener(ILocalizationListener listener) {
            _listeners.Add(listener);
            return this;
        }

        public Localization RemoveListener(ILocalizationListener listener) {
            _listeners.Remove(listener);
            return this;
        }

        (Category, bool) ParseCategory(string csv) {
            var (data, ok) = CsvReader.ParseKeyedLists(csv, true);
            if (!ok || !data.TryGetValue(HeaderKey, out var headerData)) {
                return (default, false);
            }

            data.Remove(HeaderKey);

            // сканируем индексы языков.
            Dictionary<string, int> header = new(headerData.Count);
            for (int i = 0, iMax = headerData.Count; i < iMax; i++) {
                var lang = headerData[i];
                if (!string.IsNullOrEmpty(lang)) {
                    header[lang] = i;
                }
            }

            return (new() {
                Langs = header,
                Tokens = data
            }, true);
        }

        sealed class Category {
            public Dictionary<string, int> Langs;
            public Dictionary<string, List<string>> Tokens;
        }
    }

    public interface ILocalizationListener {
        void OnLanguageChanged();
    }
}
#if ENABLE_IL2CPP
// Unity IL2CPP performance optimization attribute.
namespace Unity.IL2CPP.CompilerServices {
    enum Option {
        NullChecks = 1,
        ArrayBoundsChecks = 2
    }

    [AttributeUsage (AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Property, Inherited
 = false, AllowMultiple = true)]
    class Il2CppSetOptionAttribute : Attribute {
        public Option Option { get; private set; }
        public object Value { get; private set; }

        public Il2CppSetOptionAttribute (Option option, object value) { Option = option; Value = value; }
    }
}
#endif