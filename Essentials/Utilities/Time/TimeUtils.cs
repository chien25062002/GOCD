using System;
using UnityEngine;

namespace CFramework
{
    public static class TimeUtils
    {
        // ========== Timestamp cơ bản ==========
        /// <summary>
        /// Lấy thời gian thực tế hiện tại theo chuẩn Unix timestamp (giây).
        /// Sử dụng để tính toán khoảng thời gian đã trôi qua, ví dụ:
        /// - Hồi phục sau khi tắt game.
        /// - Cooldown, phần thưởng theo thời gian.
        /// Kết quả là số giây đã trôi qua kể từ 00:00:00 ngày 01/01/1970 (UTC).
        /// </summary>
        public static long GetUnixTimeSeconds() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        public static long GetUnixTimeMilliseconds() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        public static long ToUnixTimeSeconds(DateTime dateTime) => new DateTimeOffset(dateTime).ToUnixTimeSeconds();
        public static long ToUnixTimeMilliseconds(DateTime dateTime) => new DateTimeOffset(dateTime).ToUnixTimeMilliseconds();
        public static DateTime FromUnixTimeSeconds(long unixTimeSeconds) => DateTimeOffset.FromUnixTimeSeconds(unixTimeSeconds).UtcDateTime;
        public static DateTime FromUnixTimeMilliseconds(long unixTimeMilliseconds) => DateTimeOffset.FromUnixTimeMilliseconds(unixTimeMilliseconds).UtcDateTime;

        // ========== Tính elapsed ==========
        public static double GetElapsedSeconds(long pastUnixSeconds) => GetUnixTimeSeconds() - pastUnixSeconds;
        public static double GetElapsedMilliseconds(long pastUnixMilliseconds) => GetUnixTimeMilliseconds() - pastUnixMilliseconds;

        // ========== Format thời gian ==========
        public static string FormatTimestamp(long unixTimeSeconds)
        {
            var dt = FromUnixTimeSeconds(unixTimeSeconds);
            return dt.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static string FormatElapsedTime(double seconds)
        {
            TimeSpan t = TimeSpan.FromSeconds(seconds);
            return $"{t.Days}d {t.Hours}h {t.Minutes}m {t.Seconds}s";
        }

        public static string FormatElapsedTimeShort(double seconds)
        {
            TimeSpan t = TimeSpan.FromSeconds(seconds);
            if (t.TotalDays >= 1) return $"{(int)t.TotalDays}d {t.Hours}h";
            if (t.TotalHours >= 1) return $"{(int)t.TotalHours}h {t.Minutes}m";
            if (t.TotalMinutes >= 1) return $"{(int)t.TotalMinutes}m {t.Seconds}s";
            return $"{t.Seconds}s";
        }

        // ========== Khoảng thời gian giữa 2 timestamp ==========
        public static double GetDurationSeconds(long startUnix, long endUnix) => endUnix - startUnix;
        public static double GetDurationMilliseconds(long startUnix, long endUnix) => endUnix - startUnix;

        // ========== Tính phần trăm tiến độ ==========
        public static float GetProgress01(long startUnix, long endUnix, long currentUnix)
        {
            if (endUnix <= startUnix) return 1f;
            return Mathf.Clamp01((float)(currentUnix - startUnix) / (endUnix - startUnix));
        }

        // ========== Kiểm tra hết hạn ==========
        public static bool HasExpired(long startUnix, double durationSeconds)
        {
            return GetElapsedSeconds(startUnix) >= durationSeconds;
        }

        // ========== Chuyển đổi đơn vị ==========
        public static double SecondsToMinutes(double seconds) => seconds / 60.0;
        public static double SecondsToHours(double seconds) => seconds / 3600.0;
        public static double SecondsToDays(double seconds) => seconds / 86400.0;

        public static double MinutesToSeconds(double minutes) => minutes * 60.0;
        public static double HoursToSeconds(double hours) => hours * 3600.0;
        public static double DaysToSeconds(double days) => days * 86400.0;

        // ========== Tăng giảm timestamp ==========
        public static long AddSeconds(long unixTime, double seconds) => unixTime + (long)seconds;
        public static long AddMinutes(long unixTime, double minutes) => unixTime + (long)(minutes * 60.0);
        public static long AddHours(long unixTime, double hours) => unixTime + (long)(hours * 3600.0);
        public static long AddDays(long unixTime, double days) => unixTime + (long)(days * 86400.0);

        // ========== Clamp timestamp ==========
        public static long ClampUnixTime(long unixTime, long min, long max) => Math.Min(Math.Max(unixTime, min), max);

        // ========== Chênh lệch ngày/giờ ==========
        public static int GetDaysDifference(long unixA, long unixB) => (int)Math.Abs((unixA - unixB) / 86400);
        public static int GetHoursDifference(long unixA, long unixB) => (int)Math.Abs((unixA - unixB) / 3600);
    }
}
