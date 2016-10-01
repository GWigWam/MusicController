using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PlayerInterface.ValueConverters {

    public class NegativeValueConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            double val = (value is int ? (int)value : value is double ? (double)value : -1);

            var paramStr = parameter as string;
            if(string.IsNullOrEmpty(paramStr)) {
                return val * -1;
            } else if(paramStr == "-") {
                return val >= 0 ? val * -1 : val;
            } else if(paramStr == "+") {
                return val < 0 ? val * -1 : val;
            }
            return val;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            var res = Convert(value, targetType, parameter, culture);

            if(res is double) {
                return ((double)res) * -1;
            } else {
                return res;
            }
        }
    }
}