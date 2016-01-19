using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;

namespace Common
{
    public static class StringExtensions
    {
        private const int EncryptionKeySize = 128;
        private static readonly List<Type> EncryptionKeySource = new List<Type> { typeof(Guid), typeof(Thread), typeof(object) };
        private static readonly byte[] EncryptionKey;
        private static readonly byte[] EncryptionIV = { 72, 11, 209, 1, 213, 215, 251, 210, 32, 184, 154, 160, 128, 78, 108, 131 };
        private static readonly Func<SymmetricAlgorithm> EncryptionAlgorithmFactory = Aes.Create;

        private static readonly ConcurrentDictionary<Type, MethodInfo> ConversionsDictionary = new ConcurrentDictionary<Type, MethodInfo>();

        public static class EnumHelper
        {
            static readonly MethodInfo _enumTryParse;

            static EnumHelper()
            {
                _enumTryParse = typeof(Enum).GetMethods(BindingFlags.Public | BindingFlags.Static).First(m => m.Name == "TryParse" && m.GetParameters().Length == 3);
            }

            public static bool TryParse(
                Type enumType,
                string value,
                bool ignoreCase,
                out object enumValue)
            {
                MethodInfo genericEnumTryParse = _enumTryParse.MakeGenericMethod(enumType);

                object[] args = { value, ignoreCase, Enum.ToObject(enumType, 0) };
                bool success = (bool)genericEnumTryParse.Invoke(null, args);
                enumValue = args[2];

                return success;
            }
        }

        static StringExtensions()
        {
            EncryptionKey = EncryptionKeySource.Select(type => Encoding.UTF8.GetBytes(type.FullName)).SelectMany(bytes => bytes).Take(EncryptionKeySize / 8).ToArray();
        }

        public static T? ConvertTo<T>(this string str)
            where T : struct
        {
            if (String.IsNullOrEmpty(str))
            {
                return null;
            }

            return (T?)ConvertValueType(str, typeof(T));
        }

        public static Uri ConvertToUri(this string str)
        {
            if (String.IsNullOrEmpty(str))
            {
                return null;
            }

            return new Uri(str);
        }

        public static List<T> ConvertToListOf<T>(this string str, char splitValue = ',')
            where T : struct
        {
            if (String.IsNullOrEmpty(str)) return new List<T>();

            return str
                .Split(splitValue)
                .Select(ConvertTo<T>)
                .Where(item => item != null)
                .Select(item => item.Value)
                .ToList();
        }

        public static IList ConvertToListOf(this string str, Type itemType, char splitValue = ',')
        {
            var ret = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(itemType));

            var values =
                str
                .Split(splitValue)
                .Select(rawItem => ConvertValueType(rawItem, itemType))
                .Where(item => item != null);

            foreach (var value in values)
            {
                ret.Add(value);
            }

            return ret;
        }

        public static string ToBase64String(this string str, Encoding encoding = null)
        {
            var actualEncoding = encoding ?? Encoding.UTF8;
            return Convert.ToBase64String(actualEncoding.GetBytes(str));
        }

        public static string FromBase64String(this string base64Str, Encoding encoding = null)
        {
            var actualEncoding = encoding ?? Encoding.UTF8;
            return actualEncoding.GetString(Convert.FromBase64String(base64Str));
        }

        public static string ToEncryptedText(this string plainText)
        {
            if (String.IsNullOrEmpty(plainText))
            {
                throw new ArgumentNullException();
            }

            string encryptedText;

            using (var algorithm = EncryptionAlgorithmFactory())
            {
                algorithm.Key = EncryptionKey;
                algorithm.IV = EncryptionIV;

                using (var encryptor = algorithm.CreateEncryptor(algorithm.Key, algorithm.IV))
                using (var memoryStream = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    using (var writer = new StreamWriter(cryptoStream))
                    {
                        writer.Write(plainText);
                    }

                    encryptedText = Convert.ToBase64String(memoryStream.ToArray());
                }
            }

            return encryptedText;
        }

        public static string ToPlainText(this string encryptedText)
        {
            if (String.IsNullOrEmpty(encryptedText))
            {
                throw new ArgumentNullException();
            }

            string plainText;


            try
            {
                using (var algorithm = EncryptionAlgorithmFactory())
                {
                    algorithm.Key = EncryptionKey;
                    algorithm.IV = EncryptionIV;

                    using (var decryptor = algorithm.CreateDecryptor(algorithm.Key, algorithm.IV))
                    using (var memoryStream = new MemoryStream(Convert.FromBase64String(encryptedText)))
                    using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    using (var reader = new StreamReader(cryptoStream))
                    {
                        plainText = reader.ReadToEnd();
                    }
                }
            }
            catch (Exception)
            {
                throw new LogicException("Unable to decrypt the encrypted text.");
            }

            return plainText;
        }

        private static object ConvertValueType(string raw, Type targetType)
        {
            if (targetType.IsEnum)
            {
                object enumValue;
                return EnumHelper.TryParse(targetType, raw, true, out enumValue) ? enumValue : null;
            }

            if (targetType == typeof(DateTime))
            {
                DateTime date;
                return DateTime.TryParse(raw, out date) ? (object)date : null;
            }

            if (targetType == typeof(TimeSpan))
            {
                //try to parse int and use it for seconds
                int seconds;
                if (int.TryParse(raw, out seconds))
                {
                    return TimeSpan.FromSeconds(seconds);
                }

                //try to parse Timespan
                TimeSpan time;
                if (TimeSpan.TryParse(raw, out time))
                {
                    return time;
                }

                //return default value
                return null;
            }

            var tryParseMethod = ConversionsDictionary.GetOrAdd(
                targetType,
                type => targetType
                    .GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .FirstOrDefault(m => m.Name == "TryParse" && m.GetParameters().Length == 2));

            if (tryParseMethod == null)
            {
                return null;
            }

            var args = new[] { raw, Activator.CreateInstance(targetType) };
            if ((bool)tryParseMethod.Invoke(null, args) == false)
            {
                return null;
            }

            return args[1];
        }

        public static string ToJson<T>(this T obj) where T : class
        {
            if (obj == null)
                return null;

            var serializer = new JavaScriptSerializer();
            string output = serializer.Serialize(obj);
            return output;
        }

        /// <summary>
        /// Compares two strings ignoring case.
        /// </summary>
        /// <param name="self">First string to compare.</param>
        /// <param name="value">Second string to compare.</param>
        /// <returns>-1 if first string is less; 0 if strings are the same; 1 if first string is greater</returns>
        public static int CompareToNoCase(this string self, string value)
        {
            return string.Compare(self, value, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Checks if a string contains another string ignoring case.
        /// </summary>
        /// <param name="self">String to check.</param>
        /// <param name="value">Substring to look for.</param>
        /// <returns>True if the first string contains the second one ignoring case.</returns>
        public static bool ContainsNoCase(this string self, string value)
        {
            return self == null ? value == null : (self.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        /// <summary>
        /// Checks if a string contains either of given substrings ignoring case.
        /// </summary>
        /// <param name="self">String to check.</param>
        /// <param name="values">Substrings to look for.</param>
        /// <returns>True if the first string contains either of the substrings ignoring case.</returns>
        public static bool ContainsAnyNoCase(this string self, params string[] values)
        {
            return values.Any(self.ContainsNoCase);
        }

        /// <summary>
        /// Counts character of occurences in a string.
        /// </summary>
        /// <param name="self">String to search.</param>
        /// <param name="value">Character to look for.</param>
        /// <returns>Number of times ch occurs in the string.</returns>
        public static int Count(this string self, char value)
        {
            return self.Count(t => t == value);
        }

        /// <summary>
        /// Checks if a string ends with a given suffix ignoring case.
        /// </summary>
        /// <param name="self">String to check.</param>
        /// <param name="value">Suffix to check.</param>
        /// <returns>True if the first string ends with the second one ignoring case.</returns>
        public static bool EndsWithNoCase(this string self, string value)
        {
            return self == null ? value == null : self.EndsWith(value, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Checks if a string ends with either of given set of sufficies ignoring case.
        /// </summary>
        /// <param name="self">String to check.</param>
        /// <param name="values">Sufficies to check.</param>
        /// <returns>True if the first string begins with either of the sufficies ignoring case.</returns>
        public static bool EndsWithAnyNoCase(this string self, params string[] values)
        {
            return values.Any(self.EndsWithNoCase);
        }

        /// <summary>
        /// Performs case-insensitive comparison of two strings.
        /// </summary>
        /// <param name="self">First string to compare.</param>
        /// <param name="value">Second string to compare.</param>
        /// <returns>True if strings are the same ignoring case.</returns>
        public static bool EqualsNoCase(this string self, string value)
        {
            return self == null ? value == null : self.Equals(value, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Performs case-insensitive comparison of a given string against a set of strings.
        /// </summary>
        /// <param name="self">First string to compare.</param>
        /// <param name="values">Strings to compare against.</param>
        /// <returns>True if any of the values strings matches a ignoring case.</returns>
        public static bool EqualsAnyNoCase(this string self, params string[] values)
        {
            return values.Any(self.EqualsNoCase);
        }

        /// <summary>
        /// Returns first word from the string skipping any leading whitespace.
        /// </summary>
        /// <param name="self">String to get first word from.</param>
        /// <returns>First word from beginning of the string or empty string if none found.</returns>
        public static string FirstWord(this string self)
        {
            var startIndex = 0;
            while (startIndex < self.Length && char.IsWhiteSpace(self[startIndex]))
            {
                startIndex++;
            }

            var endIndex = startIndex;
            while (endIndex < self.Length && !char.IsWhiteSpace(self[endIndex]))
            {
                endIndex++;
            }

            return self.Substring(startIndex, endIndex - startIndex);
        }

        /// <summary>
        /// Returns last word from the string skipping any trailing whitespace.
        /// </summary>
        /// <param name="self">String to get last word from.</param>
        /// <returns>Last word from the end of the string or empty string if none found.</returns>
        public static string LastWord(this string self)
        {
            var endIndex = self.Length - 1;
            while (endIndex > 0 && char.IsWhiteSpace(self[endIndex]))
            {
                endIndex--;
            }

            var startIndex = endIndex;
            while (startIndex > 0 && char.IsLetterOrDigit(self[startIndex]))
            {
                startIndex--;
            }

            return endIndex > startIndex ? self.Substring(startIndex + 1, endIndex - startIndex) : string.Empty;
        }

        /// <summary>
        /// Replaces the format items in string with the string representation of a corresponding object in a specified array.
        /// Uses InvariantCulture formatting for the values.
        /// </summary>
        /// <param name="self">A composite format string.</param>
        /// <param name="arguments">An object array that contains zero or more objects to format.</param>
        /// <returns>
        /// A copy of format in which the format items have been replaced by the string
        /// representation of the corresponding objects in args.
        /// </returns>
        public static string FormatInvariant(this string self, params object[] arguments)
        {
            return string.Format(CultureInfo.InvariantCulture, self, arguments);
        }

        /// <summary>
        /// Returns case-insensitive hash code for a string.
        /// </summary>
        /// <param name="self">String to get hash code for.</param>
        /// <returns>Case-insensitive hash code.</returns>
        public static int GetHashCodeNoCase(this string self)
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(self);
        }

        /// <summary>
        /// Returns index of specified occurence of a substring.
        /// </summary>
        /// <param name="self">String to check.</param>
        /// <param name="value">Value to look for.</param>
        /// <param name="startIndex">Index to start searching from.</param>
        /// <returns>Index of first occurency of value in the string beginning with startIndex or -1 if none found.</returns>
        public static int IndexOfNoCase(this string self, string value, int startIndex = 0)
        {
            return self == null ? -1 : self.IndexOf(value, startIndex, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Replaces all case-insensitive occurences of oldValue with newValue.
        /// </summary>
        /// <param name="self">String containing occurences of oldValue</param>
        /// <param name="oldValue">Value to look for</param>
        /// <param name="newValue">Value to replace oldValue with</param>
        /// <returns>String where all case-insensitive occurencens of oldValue are replaced with newValue.</returns>
        public static string ReplaceNoCase(this string self, string oldValue, string newValue)
        {
            if (string.IsNullOrEmpty(oldValue))
            {
                throw new ArgumentNullException("oldValue");
            }

            if (newValue == null)
            {
                throw new ArgumentNullException("newValue");
            }

            if (self == null)
            {
                return null;
            }

            var builder = new StringBuilder();

            for (var i = 0; i < self.Length;)
            {
                var next = self.IndexOfNoCase(oldValue, i);
                if (next < 0)
                {
                    builder.Append(self.Substring(i));
                    break;
                }
                else
                {
                    builder.Append(self.Substring(i, next - i));
                    builder.Append(newValue);
                    i = next + oldValue.Length;
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// Checks if a string starts with a given prefix ignoring case.
        /// </summary>
        /// <param name="self">String to check.</param>
        /// <param name="value">Prefix to check.</param>
        /// <returns>True if the first string begins with the second one ignoring case.</returns>
        public static bool StartsWithNoCase(this string self, string value)
        {
            return self == null ? value == null : self.StartsWith(value, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Checks if a string starts with either of given set of prefixes ignoring case.
        /// </summary>
        /// <param name="self">String to check.</param>
        /// <param name="values">Prefixes to check.</param>
        /// <returns>True if the first string begins with either of the prefixes ignoring case.</returns>
        public static bool StartsWithAnyNoCase(this string self, params string[] values)
        {
            return values.Any(self.StartsWithNoCase);
        }


        /// <summary>
        ///  Converts the specified string to titlecase using <see cref="CultureInfo.InvariantCulture"/>.
        /// </summary>
        /// <param name="self">The string to convert to titlecase.</param>
        /// <returns>The specified string converted to titlecase.</returns>
        public static string ToTitleCaseInvariant(this string self)
        {
            return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(self);
        }

        /// <summary>
        /// Trims prefix from a string if present.
        /// </summary>
        /// <param name="self">String to trim.</param>
        /// <param name="prefix">Prefix string to check.</param>
        /// <returns>String without prefix if string starts with it; initial string otherwise.</returns>
        public static string TrimPrefix(this string self, string prefix)
        {
            return (self != null && prefix != null && self.StartsWith(prefix, StringComparison.Ordinal)) ? self.Substring(prefix.Length) : self;
        }

        /// <summary>
        /// Trims prefix from a string if present (case-insensitive).
        /// </summary>
        /// <param name="self">String to trim.</param>
        /// <param name="prefix">Prefix string to check.</param>
        /// <returns>String without prefix if string starts with it; initial string otherwise.</returns>
        public static string TrimPrefixNoCase(this string self, string prefix)
        {
            return (self != null && prefix != null && self.StartsWithNoCase(prefix)) ? self.Substring(prefix.Length) : self;
        }

        /// <summary>
        /// Trims suffix from a string if present.
        /// </summary>
        /// <param name="self">String to trim.</param>
        /// <param name="suffix">Suffix string to check.</param>
        /// <returns>String without suffix if string starts with it; initial string otherwise.</returns>
        public static string TrimSuffix(this string self, string suffix)
        {
            return (self != null && suffix != null && self.EndsWith(suffix, StringComparison.Ordinal)) ? self.Substring(0, self.Length - suffix.Length) : self;
        }

        /// <summary>
        /// Truncate a string to a maximum length
        /// </summary>
        /// <param name="source">The source string</param>
        /// <param name="maxLength">The maximum length for the string</param>
        /// <returns>The truncated string</returns>
        public static string Truncate(this string source, int maxLength)
        {
            if (string.IsNullOrEmpty(source) || source.Length <= maxLength)
            {
                return source;
            }

            return maxLength > 3 ? string.Join(source.Substring(0, maxLength - 3), "...") : "...";
        }

    }
}
