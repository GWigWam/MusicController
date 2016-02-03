using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PlayerCore.Settings {

    public class SettingChangedEventArgs : EventArgs {
        public PropertyInfo ChangedProperty;
        public string ChangedPropertyName => ChangedProperty.Name;

        public SettingChangedEventArgs(PropertyInfo changedProperty) {
            ChangedProperty = changedProperty;
        }

        public SettingChangedEventArgs(Type type, string propertyName) {
            ChangedProperty = type.GetProperty(propertyName);
        }
    }
}