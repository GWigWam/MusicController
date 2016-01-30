using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerInterface {

    public static class FormatHelper {

        public static string FormatTimeSpan(TimeSpan? ts) {
            if(ts.HasValue) {
                return FormatTimeSpan(ts.Value);
            } else {
                return string.Empty;
            }
        }

        public static string FormatTimeSpan(TimeSpan ts) {
            var format = Math.Floor(ts.TotalHours) > 0 ? @"h\:m\:ss" : @"m\:ss";
            return ts.ToString(format);
        }
    }
}