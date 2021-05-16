using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace GunshotWound2.Utils
{
    public sealed class LocalizationManager
    {
        private readonly Regex _csvMultilineFixRegex = new Regex("\"([^\"]|\"\"|\\n)*\"");
        private readonly Regex _csvParseRegex = new Regex("(?<=^|,)(\"(?:[^\"]|\"\")*\"|[^,]*)");
        private readonly List<string> _csvBuffer = new List<string>(32);

        private readonly Dictionary<string, string[]> _localizationDictionary;
        private int _currentLocaleIndex = -1;

        public LocalizationManager(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                _localizationDictionary = CsvToDict(reader.ReadToEnd());
            }
        }

        public void SetLanguage(string desiredLanguage)
        {
            var languages = _localizationDictionary["Localization"];

            for (var i = 0; i < languages.Length; i++)
            {
                if (!string.Equals(languages[i], desiredLanguage, StringComparison.CurrentCultureIgnoreCase)) continue;

                _currentLocaleIndex = i;
                return;
            }

            _currentLocaleIndex = -1;
        }

        public string GetWord(string key)
        {
            if (_currentLocaleIndex < 0)
            {
                throw new Exception("Language not selected");
            }

            if (!_localizationDictionary.ContainsKey(key))
            {
                throw new Exception($"Word \"{key}\" doesn't exist in localization");
            }

            return _localizationDictionary[key][_currentLocaleIndex];
        }

        private void ParseCsvLine(string data)
        {
            _csvBuffer.Clear();
            var lastAddedPart = "FirstLine";
            try
            {
                data = _csvMultilineFixRegex.Replace(data, m => m.Value.Replace("\n", "space"));
                foreach (Match m in _csvParseRegex.Matches(data))
                {
                    var part = m.Value.Trim();
                    if (part.Length > 0)
                    {
                        if (part[0] == '"' && part[part.Length - 1] == '"')
                        {
                            part = part.Substring(1, part.Length - 2);
                        }

                        part = part.Replace("\"\"", "\"");
                    }

                    lastAddedPart = part;
                    _csvBuffer.Add(part);
                }
            }
            catch (Exception e)
            {
                throw new Exception($"ParsingException! ErrorPart:\n{lastAddedPart}.\nInner:\n{e.Message}", e);
            }
        }

        private Dictionary<string, string[]> CsvToDict(string data)
        {
            var dict = new Dictionary<string, string[]>();
            var headerLen = -1;
            string key;

            using (var reader = new StringReader(data))
            {
                while (reader.Peek() != -1)
                {
                    ParseCsvLine(reader.ReadLine());
                    if (_csvBuffer.Count <= 0 || string.IsNullOrEmpty(_csvBuffer[0])) continue;

                    if (headerLen == -1)
                    {
                        headerLen = _csvBuffer.Count;
                    }

                    if (_csvBuffer.Count != headerLen)
                    {
                        continue;
                    }

                    key = _csvBuffer[0];
                    _csvBuffer.RemoveAt(0);
                    dict[key] = _csvBuffer.ToArray();
                }
            }

            return dict;
        }
    }
}