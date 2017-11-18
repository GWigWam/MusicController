using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PlayerInterface.Themes {
    public class ThemeManager : INotifyPropertyChanged {
        public const string DefaultThemeName = "Default";

        private static readonly Dictionary<string, string> ThemeFiles = new Dictionary<string, string> {
            [DefaultThemeName] = "Default.xaml",
            ["Dark"] = "Dark.xaml"
        };

        public static IEnumerable<string> AvailableThemes => ThemeFiles.Keys;

        private static ThemeManager instance;
        public static ThemeManager Instance => instance = (instance ?? new ThemeManager());

        public event PropertyChangedEventHandler PropertyChanged;

        private ResourceDictionary theme = null;
        public ResourceDictionary Theme {
            get => theme;
            set {
                if (theme != value) {
                    theme = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Theme)));
                }
            }
        }

        private ThemeManager()
        {
            LoadTheme(DefaultThemeName);
        }

        public void LoadTheme(string name = DefaultThemeName) {
            var rd = GetNamed(name);
            if (rd != null) {
                Theme = rd;
            }
        }

        private ResourceDictionary GetNamed(string name) {
            if (ThemeFiles.ContainsKey(name)) {
                var file = ThemeFiles[name];
                var uri = new Uri($"{Assembly.GetExecutingAssembly().GetName().Name};component\\Themes/{file}", UriKind.Relative);
                var dict = (ResourceDictionary)Application.LoadComponent(uri);
                return dict;
            } else {
                return null;
            }
        }
    }
}
