using System;
using System.Collections.Generic;
using System.Linq;

namespace Jhulis.Core.Helpers.Extensions
{
    public static class StringExtensions
    {
        //Check Similarity

        /// From https://social.technet.microsoft.com/wiki/contents/articles/26805.c-calculating-percentage-similarity-of-2-strings.aspx
        /// <summary>
        /// Returns the number of steps required to transform the source string
        /// into the target string.
        /// </summary>
        private static int ComputeLevenshteinDistance(string source, string target)
        {
            if (source == null || target == null) return 0;
            if (source.Length == 0 || target.Length == 0) return 0;
            if (source == target) return source.Length;

            int sourceWordCount = source.Length;
            int targetWordCount = target.Length;

            // Step 1
            if (sourceWordCount == 0)
                return targetWordCount;

            if (targetWordCount == 0)
                return sourceWordCount;

            var distance = new int[sourceWordCount + 1, targetWordCount + 1];

            // Step 2
            for (var i = 0; i <= sourceWordCount; distance[i, 0] = i++){}
            for (var j = 0; j <= targetWordCount; distance[0, j] = j++){}
            
            for (var i = 1; i <= sourceWordCount; i++)
            {
                for (var j = 1; j <= targetWordCount; j++)
                {
                    // Step 3
                    int cost = target[j - 1] == source[i - 1] ? 0 : 1;

                    // Step 4
                    distance[i, j] = Math.Min(Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                        distance[i - 1, j - 1] + cost);
                }
            }

            return distance[sourceWordCount, targetWordCount];
        }

        /// <summary>
        /// Calculate percentage similarity of two strings
        /// <param name="source">Source String to Compare with</param>
        /// <param name="target">Targeted String to Compare</param>
        /// <param name="ignoreCase">Ignore case during comparison</param>
        /// <returns>Return Similarity between two strings from 0 to 1.0</returns>
        /// </summary>
        public static double CalculateSimilarityWith(this string source, string target, bool ignoreCase = false)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target)) return 0.0;
            if (source == target) return 1.0;

            if (ignoreCase)
            {
                source = source.RemoveHifenAndUnderline().ToLowerInvariant();
                target = target.RemoveHifenAndUnderline().ToLowerInvariant();
            }

            int stepsToSame = ComputeLevenshteinDistance(source, target);
            return 1.0 - (double) stepsToSame / (double) Math.Max(source.Length, target.Length);
        }

        //Check case type

        public static List<CaseType> GetCaseType(this string str, bool tolerateNumbers = false)
        {
            var result = new List<CaseType>();

            if (str.IsCamelCase(tolerateNumbers))
                result.Add(CaseType.CamelCase);

            if (str.IsPascalCase(tolerateNumbers))
                result.Add(CaseType.PascalCase);

            if (str.IsSnakeCase(tolerateNumbers))
                result.Add(CaseType.SnakeCase);

            if (str.IsKebabCase(tolerateNumbers))
                result.Add(CaseType.KebabCase);

            if (result.Count == 0) result.Add(CaseType.None);

            return result;
        }

        public static bool IsCamelCase(this string str, bool tolerateNumbers = false)
        {
            if (string.IsNullOrWhiteSpace(str)) return false;

            char firstChar = str[0];
            char lastChar = str[str.Length - 1];

            return char.IsLower(firstChar)
                   && (char.IsLower(lastChar) || tolerateNumbers)
                   && !ExistsTwoCharsUpper(str)
                   && (str.All(char.IsLetter) || tolerateNumbers && str.All(char.IsLetterOrDigit));
        }

        public static bool IsPascalCase(this string str, bool tolerateNumbers = false)
        {
            if (string.IsNullOrWhiteSpace(str)) return false;

            char firstChar = str[0];
            char lastChar = str[str.Length - 1];

            return char.IsUpper(firstChar)
                   && (char.IsLower(lastChar) || tolerateNumbers)
                   && !ExistsTwoCharsUpper(str)
                   && (str.All(char.IsLetter) || tolerateNumbers && str.All(char.IsLetterOrDigit));
        }

        public static bool IsSnakeCase(this string str, bool tolerateNumbers = false)
        {
            return !string.IsNullOrWhiteSpace(str) && str.All(c =>
                       tolerateNumbers && (char.IsNumber(c) || char.IsLower(c)) ||
                       !tolerateNumbers && char.IsLetter(c) && char.IsLower(c)
                       || c == '_');
        }

        public static bool IsKebabCase(this string str, bool tolerateNumbers = false)
        {
            return !string.IsNullOrWhiteSpace(str) && str.All(c =>
                       tolerateNumbers && (char.IsNumber(c) || char.IsLower(c)) ||
                       !tolerateNumbers && char.IsLetter(c) && char.IsLower(c)
                       || c == '-');
        }

        public static bool BeginsUpperAndFinishesWithPeriod(this string str)
        {
            return !string.IsNullOrWhiteSpace(str)
                    && char.IsUpper(str.First())
                    && str.Last() == '.';
        }

        //Convert case

        public static string ToCamelCase(this string str)
        {
            string[] pieces;

            switch (str.GetCaseType(true).First())
            {
                case CaseType.None:
                    throw new ArgumentException("String need to be an valid Camel, Pascal, Kebab or Snake case.");
                case CaseType.PascalCase:
                    return char.ToLowerInvariant(str[0]) + str.Substring(1);
                case CaseType.KebabCase:
                    pieces = str.Split('-');
                    str = "";
                    foreach (string piece in pieces)
                    {
                        if (str == "")
                            str += char.ToLowerInvariant(piece[0]) + piece.Substring(1);
                        else
                            str += char.ToUpperInvariant(piece[0]) + piece.Substring(1);
                    }

                    return str;
                case CaseType.SnakeCase:
                    pieces = str.Split('_');
                    str = "";
                    foreach (string piece in pieces)
                    {
                        if (str == "")
                            str += char.ToLowerInvariant(piece[0]) + piece.Substring(1);
                        else
                            str += char.ToUpperInvariant(piece[0]) + piece.Substring(1);
                    }

                    return str;
                default:
                    return char.ToLowerInvariant(str[0]) + str.Substring(1);
            }
        }

        public static string ToPascalCase(this string str)
        {
            switch (str.GetCaseType(true).First())
            {
                case CaseType.None:
                    throw new ArgumentException("String need to be an valid Camel, Pascal, Kebab or Snake case.");
                case CaseType.CamelCase:
                    return char.ToUpperInvariant(str[0]) + str.Substring(1);
                case CaseType.KebabCase:
                    return str
                        .Split('-')
                        .Aggregate("",
                            (currentPiece, nextPiece) =>
                                currentPiece + (char.ToUpperInvariant(nextPiece[0]) + nextPiece.Substring(1)));
                case CaseType.SnakeCase:
                    return str
                        .Split('_')
                        .Aggregate("",
                            (currentPiece, nextPiece) =>
                                currentPiece + (char.ToUpperInvariant(nextPiece[0]) + nextPiece.Substring(1)));
                default:
                    return char.ToUpperInvariant(str[0]) + str.Substring(1);
            }
        }

        public static string ToKebabCase(this string str)
        {
            switch (str.GetCaseType(true).First())
            {
                case CaseType.None:
                    throw new ArgumentException("String need to be an valid Camel, Pascal, Kebab or Snake case.");
                case CaseType.PascalCase:
                case CaseType.CamelCase:
                    var strReturn = "";
                    foreach (char character in str)
                        if (string.IsNullOrEmpty(strReturn) || char.IsLower(character))
                            strReturn += char.ToLowerInvariant(character);
                        else
                            strReturn += "-" + char.ToLowerInvariant(character);
                    return strReturn;
                case CaseType.SnakeCase:
                    return str.Replace('_', '-').ToLowerInvariant();
                default:
                    return char.ToLowerInvariant(str[0]) + str.Substring(1);
            }
        }

        public static string ToSnakeCase(this string str)
        {
            switch (str.GetCaseType(true).First())
            {
                case CaseType.None:
                    throw new ArgumentException("String need to be an valid Camel, Pascal, Kebab or Snake case.");
                case CaseType.PascalCase:
                case CaseType.CamelCase:
                    var strReturn = "";
                    foreach (char character in str)
                        if (string.IsNullOrEmpty(strReturn) || char.IsLower(character))
                            strReturn += char.ToLowerInvariant(character);
                        else
                            strReturn += "_" + char.ToLowerInvariant(character);
                    return strReturn;
                case CaseType.KebabCase:
                    return str.Replace('-', '_').ToLowerInvariant();
                default:
                    return char.ToLowerInvariant(str[0]) + str.Substring(1);
            }
        }

        public static string RemoveHifenAndUnderline(this string str)
        {
            return str.Replace("-", "").Replace("_", "");
        }

        //Split Strings

        public static string[] SplitCompositeWord(this string str)
        {
            var result = new List<string>();
            var currentWord = "";
            var isPreviousCharNumber = false;

            switch (str.GetCaseType(true).First())
            {
                case CaseType.CamelCase:

                    foreach (char c in str)
                    {
                        if (char.IsUpper(c) ||
                            !isPreviousCharNumber && char.IsNumber(c) ||
                            isPreviousCharNumber && !char.IsNumber(c))
                        {
                            result.Add(currentWord);
                            currentWord = string.Empty;
                        }

                        isPreviousCharNumber = char.IsNumber(c);

                        currentWord += c;
                    }

                    if (currentWord.Length > 0)
                        result.Add(currentWord);

                    break;

                case CaseType.PascalCase:

                    foreach (char c in str)
                    {
                        if (
                            !string.IsNullOrEmpty(currentWord) && char.IsUpper(c) ||
                            !isPreviousCharNumber && char.IsNumber(c) ||
                            isPreviousCharNumber && !char.IsNumber(c))
                        {
                            result.Add(currentWord);
                            currentWord = string.Empty;
                        }

                        isPreviousCharNumber = char.IsNumber(c);
                        
                        currentWord += c;
                    }

                    if (currentWord.Length > 0)
                        result.Add(currentWord);

                    break;

                case CaseType.SnakeCase:
                    result.AddRange(str.Split('_'));
                    break;

                case CaseType.KebabCase:
                    result.AddRange(str.Split('-'));
                    break;

                case CaseType.None:
                    break;
            }

            return result.ToArray();
        }

        //Helpers

        private static bool ExistsTwoCharsUpper(string str)
        {
            var lastCharUpper = false;
            foreach (bool thisCharUpper in str.Select(char.IsUpper))
            {
                if (thisCharUpper && lastCharUpper) return true;
                lastCharUpper = thisCharUpper;
            }

            return false;
        }
    }
}
