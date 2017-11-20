using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace PlayerInterface.Themes {
    public class ThemeManager : INotifyPropertyChanged {
        public const string DefaultThemeName = "Default";

        private static readonly Dictionary<string, string> ThemeFiles = new Dictionary<string, string> {
            [DefaultThemeName] = "Default.xaml",
            ["Dark"] = "Dark.xaml"
        };

        public static IEnumerable<string> AvailableThemes => ThemeFiles.Keys;
        
        public static readonly ThemeManager Instance = new ThemeManager();

        public event PropertyChangedEventHandler PropertyChanged;

        public string CurrentThemeName { get; private set; } = DefaultThemeName;        

        private Dictionary<string, Dictionary<string, Brush>> Themes { get; } = new Dictionary<string, Dictionary<string, Brush>>();

        public Dictionary<string, Brush> Theme => Themes[CurrentThemeName];

        private ThemeManager()
        {
            Init();
            SetTheme(DefaultThemeName);
        }

        public void SetTheme(string name = DefaultThemeName) {
            CurrentThemeName = name;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Theme)));
        }

        private void Init() {
            foreach (var kvp in ThemeFiles) {
                var theme = LoadResourceFile(kvp.Key);
                Themes[kvp.Key] = theme;
            }
        }

        private Dictionary<string, Brush> LoadResourceFile(string name) {
            if (ThemeFiles.ContainsKey(name)) {
                var file = ThemeFiles[name];
                var uri = new Uri($"{Assembly.GetExecutingAssembly().GetName().Name};component\\Themes/{file}", UriKind.Relative);
                var resourceDict = (ResourceDictionary)Application.LoadComponent(uri);

                var res = new Dictionary<string, Brush>();
                foreach (DictionaryEntry entry in resourceDict) {
                    if (entry.Key is string key && entry.Value is Brush brush) {
                        if (brush.CanFreeze) {
                            brush.Freeze();
                        }
                        res[key] = brush;
                    }
                }
                return res;
            } else {
                return null;
            }
        }
    }
}
