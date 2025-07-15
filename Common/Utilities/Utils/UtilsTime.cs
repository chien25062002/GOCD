using System;
using UnityEngine;

namespace GOCD.Framework
{
    public static class UtilsTime
    {
        /// <summary>
        /// Thời gian hiện tại (UTC) theo chuẩn Unix Timestamp – đơn vị milliseconds (ms)
        /// </summary>
        public static long UnixTimeMilliseconds => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        /// <summary>
        /// Thời gian hiện tại (UTC) theo chuẩn Unix Timestamp – đơn vị seconds (giây)
        /// </summary>
        public static long UnixTimeSeconds => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        /// <summary>
        /// Trả về true nếu thời gian hiện tại đã vượt qua thời gian expire
        /// </summary>
        public static bool IsExpired(long expireUnixMs)
        {
            return UnixTimeMilliseconds >= expireUnixMs;
        }

        /// <summary>
        /// Tính số giây còn lại cho đến khi hết hạn
        /// </summary>
        public static int SecondsLeft(long expireUnixMs)
        {
            long now = UnixTimeMilliseconds;
            return (int)Mathf.Max(0, (expireUnixMs - now) / 1000);
        }

        /// <summary>
        /// Trả về thời gian còn lại dưới dạng TimeSpan
        /// </summary>
        public static TimeSpan TimeLeft(long expireUnixMs)
        {
            long now = UnixTimeMilliseconds;
            return TimeSpan.FromMilliseconds(Mathf.Max(0, expireUnixMs - now));
        }

        /// <summary>
        /// Format thời gian còn lại thành chuỗi HH:mm:ss hoặc mm:ss nếu nhỏ hơn 1h
        /// </summary>
        public static string FormatTimeLeft(long expireUnixMs)
        {
            TimeSpan left = TimeLeft(expireUnixMs);

            if (left.TotalHours >= 1)
                return string.Format("{0:D2}:{1:D2}:{2:D2}", (int)left.TotalHours, left.Minutes, left.Seconds);
            else
                return string.Format("{0:D2}:{1:D2}", left.Minutes, left.Seconds);
        }

        /// <summary>
        /// Tạo timestamp hết hạn sau số giây (thường dùng để set unlock/cooldown)
        /// </summary>
        public static long CreateExpireAfterSeconds(int seconds)
        {
            return UnixTimeMilliseconds + seconds * 1000;
        }
    }
}
