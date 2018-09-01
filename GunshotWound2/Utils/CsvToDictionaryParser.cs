using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace GunshotWound2.Utils
{
    public class CsvToDictionaryParser
    {
        private readonly Regex CsvParseRegex = new Regex ("(?<=^|,)(\"(?:[^\"]|\"\")*\"|[^,]*)");
        private readonly List<string> _csvBuffer = new List<string> (32);
        
        public void ParseCsvLine (string data) {
            _csvBuffer.Clear ();
            foreach (Match m in CsvParseRegex.Matches (data)) {
                var part = m.Value.Trim ();
                if (part.Length > 0) {
                    if (part[0] == '"' && part[part.Length - 1] == '"') {
                        part = part.Substring (1, part.Length - 2);
                    }
                    part = part.Replace ("\"\"", "\"");
                }
                _csvBuffer.Add (part);
            }
        }
        
        public Dictionary<string, string[]> CsvToDict (string data) {
            var dict = new Dictionary<string, string[]> ();
            var headerLen = -1;
            string key;
            
            using (var reader = new StringReader (data)) {
                while (reader.Peek () != -1) {
                    ParseCsvLine (reader.ReadLine ());
                    if (_csvBuffer.Count <= 0 || string.IsNullOrEmpty(_csvBuffer[0])) continue;
                    
                    if (headerLen == -1) {
                        headerLen = _csvBuffer.Count;
                    }
                    if (_csvBuffer.Count != headerLen) {
                        continue;
                    }
                    
                    key = _csvBuffer[0];
                    _csvBuffer.RemoveAt (0);
                    dict[key] = _csvBuffer.ToArray ();
                }
            }
            return dict;
        }
    }
}