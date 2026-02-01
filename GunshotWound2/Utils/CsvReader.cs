// ––‒‒–‒–––‒–‒‒––‒–‒‒––‒–‒–––‒–‒––––‒––––––‒‒‒‒––––‒–‒––‒––––‒––‒‒–‒–––‒–‒
// Коммерческая лицензия подписчика
// (c) 2023 Leopotam <leopotam@yandex.ru>
// ––‒‒–‒–––‒–‒‒––‒–‒‒––‒–‒–––‒–‒––––‒––––––‒‒‒‒––––‒–‒––‒––––‒––‒‒–‒–––‒–‒

#if ENABLE_IL2CPP
using System;
using Unity.IL2CPP.CompilerServices;
#endif

namespace Leopotam.Serialization.Csv {
    using System.Collections.Generic;
#if ENABLE_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif

    public static class CsvReader {
        public static (Dictionary<string, string>, bool) ParseKeyedValues(string csv, bool normalizeNewLines) {
            var (parsedTable, ok) = ParseTable(csv, normalizeNewLines);
            if (!ok || (parsedTable.Count > 0 && parsedTable[0].Count < 2)) {
                return (default, false);
            }

            Dictionary<string, string> dict = new(parsedTable.Count);
            foreach (var line in parsedTable) {
                dict[line[0]] = line[1];
            }

            return (dict, true);
        }

        public static (Dictionary<string, List<string>>, bool) ParseKeyedLists(string csv, bool normalizeNewLines) {
            var (parsedTable, ok) = ParseTable(csv, normalizeNewLines);
            if (!ok || (parsedTable.Count > 0 && parsedTable[0].Count < 2)) {
                return (default, false);
            }

            Dictionary<string, List<string>> dict = new(parsedTable.Count);
            foreach (var line in parsedTable) {
                var key = line[0];
                line.RemoveAt(0);
                dict[key] = line;
            }

            return (dict, true);
        }

        public static (List<List<string>>, bool) ParseTable(string csv, bool normalizeNewLines) {
            List<List<string>> lines = new(32);
            List<string> line = new(8);
            var quotes = 0;
            var start = 0;
            var cols = -1;
            string val;
            char c;
            var len = csv.Length;
            for (var i = 0; i <= len; i++) {
                c = i < len ? csv[i] : '\n';
                if (c == '\"') {
                    quotes++;
                }

                if ((c == ',' || c == '\n') && (quotes & 1) == 0) {
                    if (i > start) {
                        val = csv.Substring(start, i - start).Trim();
                        if (normalizeNewLines) {
                            val = val.Replace("\\n", "\n");
                        }

                        if (quotes > 0) {
                            val = val
                                  .Substring(1, val.Length - 2)
                                  .Replace("\"\"", "\"");
                        }
                    } else {
                        val = "";
                    }

                    line.Add(val);
                    quotes = 0;
                    start = i + 1;
                    if (c == '\n') {
                        if (cols == -1) {
                            cols = line.Count;
                        }

                        if (cols != line.Count) {
                            return (default, false);
                        }

                        lines.Add(line);
                        if (i < len) {
                            line = new List<string>(cols);
                        }
                    }
                }
            }

            return quotes != 0 ? (default, false) : (lines, true);
        }
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