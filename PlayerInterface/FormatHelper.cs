using System;

namespace PlayerInterface
{
    public static class FormatHelper
    {
        public static string FormatTimeSpan(TimeSpan? ts) => ts.HasValue ? FormatTimeSpan(ts.Value) : string.Empty;

        public static string FormatTimeSpan(TimeSpan ts) => ts.ToString(ts.Days > 0 ? @"d\.hh\:mm" : ts.Hours > 0 ? @"h\:mm\:ss" : ts.Minutes > 0 ? @"mm\:ss" : "ss");
    }
}
