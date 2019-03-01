using System;

namespace PosService.Utilities
{
    /// <summary>
    /// DateTime ユーティリティ
    /// </summary>
    public static class DateTimeUtility
    {
        /// <summary>
        /// アプリケーション基準の現在時刻を取得します
        /// </summary>
        /// <returns>アプリケーション基準の現在時刻</returns>
        public static DateTime GetApplicationTimeNow() =>
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(Settings.Instance.ApplicationTimeZone));
    }
}
