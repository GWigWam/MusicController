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
            var res = ts.ToString(ts.Days > 0 ? @"d\.hh\:mm" : ts.Hours > 1 ? @"h\:mm\:ss" : ts.Minutes > 0 ? @"mm\:ss" : "ss");
            return res;
        }
    }
}