using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TimeStamp
{
    public static class Extensions
    {
        public static string GetFullExceptionMessage(this Exception e)
        {
            Exception ie = e;
            string eMsg = ie.Message;
            while (ie.InnerException != null)
            {
                ie = ie.InnerException;
                eMsg += "\r\n" + ie.Message;
            }
            return eMsg;
        }

        public static int GetWeekOfYearISO8601(this DateTime time)
        {
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }

            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        public static TimeSpan Sum<T>(this IEnumerable<T> collection, Func<T, TimeSpan> selector)
        {
            TimeSpan sum = TimeSpan.Zero;
            foreach (var value in collection)
            {
                sum += selector(value);
            }
            return sum;
        }

        public static TimeSpan Average<T>(this IEnumerable<T> collection, Func<T, TimeSpan> selector)
        {
            var values = collection.Select(c => selector(c).TotalHours).ToArray();

            return TimeSpan.FromHours(values.Average());
        }

        public static double RoundToTotalQuarterHours(this TimeSpan time)
        {
            var hours = Math.Floor(time.TotalHours);
            var fractional = time.TotalHours - hours;
            // Minuten runden auf 
            // 0.25 (viertelstunde): >= 7.5 && < 22.5
            // 0.50 (halbestunde): >= 22.5 && < 
            // 0.75 (dreiviertelstunde):
            // 0.00 (ganze stunde):
            if (fractional <= 0.125 || fractional > 0.875)
                fractional = 0;
            else if (fractional <= 0.375)
                fractional = 0.25;
            else if (fractional <= 0.625)
                fractional = 0.50;
            else if (fractional <= 0.875)
                fractional = 0.75;

            return hours + fractional;
        }

        public static string[] Split(this string str, Regex matcher)
        {
            if (str == null)
                return null;
            if (matcher == null)
                return new string[] { str };

            var matches = matcher.Matches(str);

            List<string> results = new List<string>();

            int index = 0;
            foreach (Match match in matches)
            {
                if (index < match.Index)
                {
                    results.Add(str.Substring(index, match.Index - index));
                }

                results.Add(str.Substring(match.Index, match.Length));
                index = match.Index + match.Length;

            }
            if (index <= str.Length - 1)
                results.Add(str.Substring(index));
            return results.ToArray();
        }

        public static IEnumerable<StringDifference> WordDifferences(this string original, string modified)
        {
            List<StringDifference> diffs = new List<StringDifference>();

            string[] originalWords = original.Split(new Regex(@"\W"));
            string[] modifiedWords = modified.Split(new Regex(@"\W"));

            int arrayIndexOriginal = 0;
            int arrayIndexModified = 0;
            int stringIndexOriginal = 0;
            int stringIndexModified = 0;

            while (arrayIndexOriginal < originalWords.Count() && arrayIndexModified < modifiedWords.Count())
            {
                if (originalWords[arrayIndexOriginal] != modifiedWords[arrayIndexModified])
                {
                    var current = new StringDifference() { OriginalIndex = stringIndexOriginal, ModifiedIndex = stringIndexModified };

                    //find diff end:
                    var end = FindDiffEnd(originalWords.Skip(arrayIndexOriginal).ToArray(), modifiedWords.Skip(arrayIndexModified).ToArray());

                    int diffOriginalLength = originalWords.Skip(arrayIndexOriginal).Take(end.Item1).Sum(w => w.Length);
                    int diffModifiedLength = modifiedWords.Skip(arrayIndexModified).Take(end.Item2).Sum(w => w.Length);

                    current.OriginalValue = original.Substring(stringIndexOriginal, diffOriginalLength);
                    current.ModifiedValue = modified.Substring(stringIndexModified, diffModifiedLength);

                    diffs.Add(current);

                    stringIndexOriginal += diffOriginalLength;
                    stringIndexModified += diffModifiedLength;
                    arrayIndexOriginal += end.Item1;
                    arrayIndexModified += end.Item2;
                }

                else
                {
                    stringIndexOriginal += originalWords[arrayIndexOriginal].Length;
                    stringIndexModified += modifiedWords[arrayIndexModified].Length;
                    arrayIndexOriginal++;
                    arrayIndexModified++;
                }
            }

            if (arrayIndexOriginal < originalWords.Count() || arrayIndexModified < modifiedWords.Count())
            {
                diffs.Add(new StringDifference() { OriginalIndex = stringIndexOriginal, ModifiedIndex = stringIndexModified, OriginalValue = original.Substring(stringIndexOriginal), ModifiedValue = modified.Substring(stringIndexModified) });
            }

            return diffs;
        }

        public static int FirstIndexOf<T>(this IEnumerable<T> collection, Func<T, bool> predicate)
        {
            if (collection != null && predicate != null)
            {
                for (int i = 0; i < collection.Count(); i++)
                    if (predicate(collection.ElementAt(i)))
                        return i;
            }
            return -1;
        }

        private static Tuple<int, int> FindDiffEnd(string[] originalRestWords, string[] modifiedRestWords)
        {
            for (int i = 0; i < Math.Max(originalRestWords.Count(), modifiedRestWords.Count()); i++)
            {
                if (i < originalRestWords.Count() && i < modifiedRestWords.Count())
                    if (originalRestWords[i] == modifiedRestWords[i] && !String.IsNullOrWhiteSpace(originalRestWords[i]))
                        return new Tuple<int, int>(i, i);

                if (i < modifiedRestWords.Count())
                {
                    int matchIndex = originalRestWords.Take(i).FirstIndexOf(w => w == modifiedRestWords[i] && !String.IsNullOrWhiteSpace(w));
                    if (matchIndex != -1)
                        return new Tuple<int, int>(matchIndex, i);
                }

                if (i < originalRestWords.Count())
                {
                    int matchIndex = modifiedRestWords.Take(i).FirstIndexOf(w => w == originalRestWords[i] && !String.IsNullOrWhiteSpace(w));
                    if (matchIndex != -1)
                        return new Tuple<int, int>(i, matchIndex);
                }
            }

            return new Tuple<int, int>(originalRestWords.Length, modifiedRestWords.Length);
        }

        public static string[] GetUniqueAbbreviations(this IEnumerable<string> values, int maxLengths = 10)
        {
            var abbs = values.ToDictionary(a => a, a => a.Substring(0, Math.Min(maxLengths, a.Length)) + (a.Length > maxLengths ? "..." : ""));

            foreach (var duplicates in abbs.GroupBy(v => v.Value).Where(g => g.Count() > 1))
            {
                IEnumerable<StringDifference> diffs = null;

                for (int i = 0; i < duplicates.Count() - 1; i++)
                {
                    diffs = duplicates.ElementAt(i).Key.WordDifferences(duplicates.ElementAt(i + 1).Key);

                    // Product Development (VES)
                    // Product Development (LDD-Cap)

                    // Product De...
                    // Product De...

                    // Original Diff: VES
                    // Modified Diff: LDD-Cap

                    // Product..VES..
                    // Pro..LDD-Cap..

                    var newValue = duplicates.ElementAt(i).Key;
                    var diffValue = diffs.First().OriginalValue;

                    bool isTrailing = diffs.First().OriginalIndex >= newValue.Length - diffs.First().OriginalValue.Length;

                    newValue = newValue.Substring(0, Math.Max(2, Math.Min(maxLengths - diffValue.Length, newValue.Length))) + ".." + diffValue + (isTrailing ? "" : "..");

                    abbs[duplicates.ElementAt(i).Key] = newValue;
                }

                if (diffs != null)
                {
                    var newValue = duplicates.Last().Key;
                    var diffValue = diffs.First().ModifiedValue;

                    bool isTrailing = diffs.First().ModifiedIndex >= newValue.Length - diffs.First().ModifiedValue.Length;

                    newValue = newValue.Substring(0, Math.Max(2, Math.Min(maxLengths - diffValue.Length, newValue.Length))) + ".." + diffValue + (isTrailing ? "" : "..");

                    abbs[duplicates.Last().Key] = newValue;
                }
            }

            return abbs.Values.ToArray();
        }
    }


    public class StringDifference
    {
        public string OriginalValue { get; set; }
        public string ModifiedValue { get; set; }
        public int OriginalIndex { get; set; }
        public int ModifiedIndex { get; set; }
    }
}
