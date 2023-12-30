using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PlayerCore.Persist {

    public class PersistentPropertyChangedEventArgs : EventArgs {
        public PropertyInfo ChangedProperty;
        public string ChangedPropertyName => ChangedProperty.Name;

        public PersistentPropertyChangedEventArgs(PropertyInfo changedProperty) {
            ChangedProperty = changedProperty;
        }

        public PersistentPropertyChangedEventArgs(Type type, string propertyName) {
            ChangedProperty = type.GetProperty(propertyName);
        }
    }
}