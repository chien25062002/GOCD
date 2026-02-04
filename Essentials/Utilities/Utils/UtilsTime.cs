using System;
using UnityEngine;

namespace CodeSketch.Utitlities.Utils
{
    /// <summary>
    /// Unified time utilities for save/load, cooldown, offline progress,
    /// expiration, scheduling.
    /// Uses UTC Unix Timestamp (milliseconds).
    /// </summary>
    public static class UtilsTime
    {
        // =====================================================
        // NOW
        // =====================================================

        public static long NowMs => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        public static long NowSeconds => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // =====================================================
        // SAVE TIME
        // =====================================================

        public static long CreateSaveTime() => NowMs;
        public static bool IsValidSaveTime(long savedUnixMs) => savedUnixMs > 0;

        // =====================================================
        // DURATION
        // =====================================================

        public static int SecondsSince(long savedUnixMs)
        {
            if (savedUnixMs <= 0) return 0;
            long delta = NowMs - savedUnixMs;
            return delta <= 0 ? 0 : (int)(delta / 1000);
        }

        public static TimeSpan TimeSince(long savedUnixMs)
        {
            if (savedUnixMs <= 0) return TimeSpan.Zero;
            long delta = NowMs - savedUnixMs;
            return delta <= 0 ? TimeSpan.Zero : TimeSpan.FromMilliseconds(delta);
        }

        // =====================================================
        // EXPIRE / COOLDOWN
        // =====================================================

        public static long CreateExpireAfterSeconds(int seconds)
        {
            return NowMs + (long)seconds * 1000;
        }

        public static bool IsExpired(long expireUnixMs)
        {
            return NowMs >= expireUnixMs;
        }

        public static int SecondsLeft(long expireUnixMs)
        {
            long delta = expireUnixMs - NowMs;
            return delta <= 0 ? 0 : (int)(delta / 1000);
        }

        public static TimeSpan TimeLeft(long expireUnixMs)
        {
            long delta = expireUnixMs - NowMs;
            return delta <= 0 ? TimeSpan.Zero : TimeSpan.FromMilliseconds(delta);
        }

        // =====================================================
        // RANGE / CLAMP
        // =====================================================

        public static bool IsNowInRange(long startUnixMs, long endUnixMs)
        {
            long now = NowMs;
            return now >= startUnixMs && now <= endUnixMs;
        }

        public static int ClampElapsedSeconds(long savedUnixMs, int maxSeconds)
        {
            return Mathf.Min(SecondsSince(savedUnixMs), maxSeconds);
        }

        // =====================================================
        // FORMAT – CORE
        // =====================================================

        /// <summary>
        /// Format TimeSpan as:
        /// HH:mm:ss (>= 1 hour)
        /// mm:ss    (< 1 hour)
        /// </summary>
        public static string FormatTime(TimeSpan time)
        {
            if (time.TotalHours >= 1)
                return $"{(int)time.TotalHours:D2}:{time.Minutes:D2}:{time.Seconds:D2}";
            else
                return $"{time.Minutes:D2}:{time.Seconds:D2}";
        }

        // =====================================================
        // FORMAT – FROM SECONDS
        // =====================================================

        /// <summary>
        /// Format seconds to mm:ss
        /// </summary>
        public static string FormatMMSS(int seconds)
        {
            if (seconds <= 0) return "00:00";
            int m = seconds / 60;
            int s = seconds % 60;
            return $"{m:D2}:{s:D2}";
        }

        /// <summary>
        /// Format seconds to HH:mm:ss
        /// </summary>
        public static string FormatHHMMSS(int seconds)
        {
            if (seconds <= 0) return "00:00:00";
            int h = seconds / 3600;
            int m = (seconds % 3600) / 60;
            int s = seconds % 60;
            return $"{h:D2}:{m:D2}:{s:D2}";
        }

        /// <summary>
        /// Auto format seconds:
        /// >= 1h → HH:mm:ss
        /// <  1h → mm:ss
        /// </summary>
        public static string FormatAuto(int seconds)
        {
            if (seconds >= 3600)
                return FormatHHMMSS(seconds);
            else
                return FormatMMSS(seconds);
        }

        // =====================================================
        // FORMAT – FROM TIMESTAMP
        // =====================================================

        public static string FormatTimeLeft(long expireUnixMs)
        {
            return FormatAuto(SecondsLeft(expireUnixMs));
        }

        public static string FormatTimeSince(long savedUnixMs)
        {
            return FormatAuto(SecondsSince(savedUnixMs));
        }

        // =====================================================
        // FORMAT – HUMAN READABLE (OPTIONAL)
        // =====================================================

        /// <summary>
        /// Human readable format:
        /// 2d 3h 15m
        /// 5h 10m
        /// 45s
        /// </summary>
        public static string FormatHuman(int seconds)
        {
            if (seconds <= 0) return "0s";

            int d = seconds / 86400;
            seconds %= 86400;
            int h = seconds / 3600;
            seconds %= 3600;
            int m = seconds / 60;
            int s = seconds % 60;

            if (d > 0) return $"{d}d {h}h {m}m";
            if (h > 0) return $"{h}h {m}m";
            if (m > 0) return $"{m}m {s}s";
            return $"{s}s";
        }
    }
}
