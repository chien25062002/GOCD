using System;
using System.Collections.Generic;
using System.Globalization;
using GOCD.Framework.Diagnostics;
using UnityEngine;

namespace GOCD.Framework
{
    public static class UtilsNumber
    {
        static readonly string strFormat = "{0:0.##}{1}";
        static readonly double minValueFormat = 999.0;

        private static readonly Dictionary<int, string> units = new()
        {
            {0, ""}, {1, "K"}, {2, "M"}, {3, "B"}, {4, "T"},
        };

        // ---------- FLOAT VERSION ----------
        public static string Format(float value) => Format((double)value);
        public static float ParseCompactNumberFloat(string val) => (float)ParseCompactNumber(val);

        // ---------- DOUBLE VERSION ----------
        public static string Format(double value)
        {
            if (value < minValueFormat)
                return Math.Round(value).ToString(CultureInfo.InvariantCulture);

            int n = (int)Math.Floor(Math.Log(value, 1000));
            double m = value / Math.Pow(1000, n);

            string unit = n < units.Count
                ? units[n]
                : ToUnitString(n - units.Count);

            return string.Format(strFormat, Math.Round(m, 2), unit);
        }

        public static string Format(double value, int decimalDigits = 2)
        {
            if (value < minValueFormat)
                return Math.Round(value).ToString(CultureInfo.InvariantCulture);

            int n = (int)Math.Floor(Math.Log(value, 1000));
            double m = value / Math.Pow(1000, n);

            string unit = n < units.Count
                ? units[n]
                : ToUnitString(n - units.Count);

            string decimalFormat = "0." + new string('#', decimalDigits);
            return string.Format(CultureInfo.InvariantCulture, "{0:" + decimalFormat + "}{1}", Math.Round(m, decimalDigits), unit);
        }

        public static string FormatVN(double value, int decimalDigits = 2)
        {
            if (value < minValueFormat)
            {
                var culture = new CultureInfo("vi-VN")
                {
                    NumberFormat = { NumberGroupSeparator = ".", NumberDecimalSeparator = "." }
                };
                string format = "0." + new string('#', decimalDigits);
                return Math.Round(value, decimalDigits).ToString(format, culture);
            }

            int n = (int)Math.Floor(Math.Log(value, 1000));
            double m = value / Math.Pow(1000, n);

            string unit = n < units.Count
                ? units[n]
                : ToUnitString(n - units.Count);

            string compact = Math.Round(m, decimalDigits).ToString($"F{decimalDigits}", CultureInfo.InvariantCulture).TrimEnd('0').TrimEnd('.');
            return $"{compact}{unit}";
        }

        public static string FormatVN_CompactRounded(double value, int decimalDigits = 2)
        {
            if (value < minValueFormat)
            {
                var culture = new CultureInfo("vi-VN")
                {
                    NumberFormat = { NumberGroupSeparator = ".", NumberDecimalSeparator = "." }
                };
                return Math.Round(value).ToString("N0", culture);
            }

            int n = (int)Math.Floor(Math.Log(value, 1000));
            double m = value / Math.Pow(1000, n);

            string unit = n < units.Count
                ? units[n]
                : ToUnitString(n - units.Count);

            string compact = Math.Round(m, decimalDigits).ToString($"F{decimalDigits}", CultureInfo.InvariantCulture).TrimEnd('0').TrimEnd('.');
            return $"{compact}{unit}";
        }

        public static double ParseCompactNumber(string val)
        {
            if (string.IsNullOrWhiteSpace(val))
                return 0;
        
            val = val.Trim();
            int letterStartIndex = -1;
            for (int i = 0; i < val.Length; i++)
            {
                if (char.IsLetter(val[i]))
                {
                    letterStartIndex = i;
                    break;
                }
            }
        
            if (letterStartIndex == -1)
            {
                if (double.TryParse(val, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
                    return result;
                GOCDDebug.Log($"ParseCompactNumber: Cannot parse '{val}', returning 0", Color.red);
                return 0;
            }
        
            string numberPart = val[..letterStartIndex];
            string unitPart = val[letterStartIndex..]; // ❌ bỏ .ToLowerInvariant()
        
            if (!double.TryParse(numberPart, NumberStyles.Float, CultureInfo.InvariantCulture, out var baseValue))
            {
                GOCDDebug.Log($"ParseCompactNumber: Cannot parse number '{numberPart}', returning 0", Color.red);
                return 0;
            }
        
            // ✅ xử lý phân biệt đơn vị viết hoa
            double multiplier = unitPart switch
            {
                "K" => 1_000,
                "M" => 1_000_000,
                "B" => 1_000_000_000,
                "T" => 1_000_000_000_000,
                _ => ParseExtendedUnit(unitPart.ToLowerInvariant()) // ✅ chỉ ép thường nếu là unit mở rộng
            };
        
            return baseValue * multiplier;
        }

        private static double ParseExtendedUnit(string unitPart)
        {
            int powerIndex = FromUnitString(unitPart);
            powerIndex += 5; // bắt đầu sau "T"
            return Math.Pow(1000, powerIndex);
        }

        private static string ToUnitString(int index)
        {
            string result = "";
            do
            {
                result = (char)('a' + (index % 26)) + result;
                index = index / 26 - 1;
            } while (index >= 0);
            return result;
        }

        private static int FromUnitString(string unit)
        {
            int result = 0;
            foreach (char c in unit)
            {
                int val = c - 'a';
                if (val < 0 || val > 25)
                {
                    GOCDDebug.LogWarning($"Invalid character '{c}' in unit '{unit}'", Color.red);
                    return 0;
                }
                result = result * 26 + val + 1;
            }
            return result - 1;
        }

        // ---------- COMMON FORMAT ----------
        public static string FormatPriceVN(int value)
        {
            var culture = new CultureInfo("vi-VN")
            {
                NumberFormat = { NumberGroupSeparator = "." }
            };
            return value.ToString("N0", culture);
        }
        
        #region Format

        public static string FormatWithSeparator(double value)
        {
            var culture = new CultureInfo("vi-VN")
            {
                NumberFormat =
                {
                    NumberGroupSeparator = ".",
                    NumberDecimalSeparator = ","
                }
            };

            return value.ToString("N0", culture);
        }
        
        public static string FormatWithSeparator(double value, int decimalDigits)
        {
            var culture = new CultureInfo("vi-VN")
            {
                NumberFormat =
                {
                    NumberGroupSeparator = ".",
                    NumberDecimalSeparator = ","
                }
            };

            string format = "N" + decimalDigits;
            return value.ToString(format, culture);
        }

        public static string FormatWithSeparator(float value) => FormatWithSeparator((double)value);


        #endregion
    }

    public static class CompactNumberMath
    {
        public static string Add(string a, string b)
        {
            double valA = UtilsNumber.ParseCompactNumber(a);
            double valB = UtilsNumber.ParseCompactNumber(b);
            return UtilsNumber.Format(valA + valB);
        }

        public static string Subtract(string a, string b)
        {
            double valA = UtilsNumber.ParseCompactNumber(a);
            double valB = UtilsNumber.ParseCompactNumber(b);
            return UtilsNumber.Format(Math.Max(valA - valB, 0));
        }

        public static string Multiply(string a, float multiplier)
        {
            double val = UtilsNumber.ParseCompactNumber(a);
            return UtilsNumber.Format(val * multiplier);
        }

        public static string Divide(string a, float divisor)
        {
            double val = UtilsNumber.ParseCompactNumber(a);
            return UtilsNumber.Format(divisor == 0 ? 0 : val / divisor);
        }

        public static bool GreaterThan(string a, string b)
        {
            double valA = UtilsNumber.ParseCompactNumber(a);
            double valB = UtilsNumber.ParseCompactNumber(b);
            return valA > valB;
        }
    }
}
