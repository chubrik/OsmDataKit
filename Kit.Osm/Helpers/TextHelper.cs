using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Kit.Osm
{
    public static class TextHelper
    {
        #region Constants & Maps

        private static readonly Dictionary<string, string> _translitMap =
            new Dictionary<string, string> {
                { "а", "a" },
                { "б", "b" },
                { "в", "v" },
                { "г", "g" },
                { "д", "d" },
                { "е", "e" },
                { "ё", "yo" },
                { "ж", "zh" },
                { "з", "z" },
                { "и", "i" },
                { "й", "y" },
                { "к", "k" },
                { "л", "l" },
                { "м", "m" },
                { "н", "n" },
                { "о", "o" },
                { "п", "p" },
                { "р", "r" },
                { "с", "s" },
                { "т", "t" },
                { "у", "u" },
                { "ф", "f" },
                { "х", "h" },
                { "ц", "ts" },
                { "ч", "ch" },
                { "ш", "sh" },
                { "щ", "sch" },
                { "ъ", "" },
                { "ы", "i" },
                { "ь", "" },
                { "э", "e" },
                { "ю", "yu" },
                { "я", "ya" },
                { "і", "i" }, // cyrillic "i"
                { "'", "’" },
                { "`", "’" },
                { "ʼ", "’" }, // 700 \u02bc => 8217 \u2009
                //todo
                { "ї", "yi" },
                { "ө", "s" },
                { "ү", "u" },
                { "ҷ", "ch" },
                { "ӣ", "i" },
                { "ӯ", "u" },
                { "ҳ", "h" },
                { "қ", "k" },
                { "ғ", "g" },
                { "ң", "n" },
                { "є", "e" },
                { "ј", "y" }, // cyrillic "j"
                { "љ", "l" },
                { "ћ", "h" },
                { "џ", "ts"},
                { "ќ", "k" },
                { "њ", "n" },
                { "ђ", "h" },
                { "ѓ", "g" },
                { "ґ", "g" },
                { "ѝ", "i" },
                { "ѐ", "e" }
            };

        private static readonly string _cyrillicToLatinPattern =
            _translitMap.Keys.Select(i => i.ToUpper()).Concat(_translitMap.Keys).Join();

        #endregion

        public static string FixApostrophe(string text) => Regex.Replace(text, "[`'‘ʻʼ]", "’");

        public static string CyrillicToLatin(string text)
        {
            Debug.Assert(!text.IsNullOrWhiteSpace());

            if (text.IsNullOrWhiteSpace())
                throw new ArgumentException(nameof(text));

            var sb = new StringBuilder(text);
            sb.Replace("ее", "eye");
            sb.Replace("ье", "ye");
            sb.Replace("ьи", "yi");
            sb.Replace("ъе", "ye");
            sb.Replace("ъи", "yi");
            text = sb.ToString();

            var result = Regex.Replace(
                text, $"[{_cyrillicToLatinPattern}]", i =>
                {
                    var isUpper = i.Value == i.Value.ToUpper();
                    var translited = _translitMap[i.Value.ToLower()];

                    return isUpper && translited.Length > 0
                        ? translited.First().ToString().ToUpper() + translited.Substring(1)
                        : translited;
                });

            if (!Regex.IsMatch(result.ToLower(), @"^[a-z ’-]+$"))
                LogService.LogWarning($"Non-translited cyrillic title: {result}");

            return result;
        }
    }
}
