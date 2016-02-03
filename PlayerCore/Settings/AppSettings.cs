﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerCore.Settings {

    public class AppSettings : SettingsFile {
        private float volume = 1;

        [JsonProperty]
        public float Volume {
            get { return volume; }
            set {
                if(value != volume && value >= 0 && value <= 1) {
                    volume = value;
                    RaiseChanged(new SettingChangedEventArgs(typeof(AppSettings), nameof(Volume)));
                }
            }
        }

        private bool startMinimized = true;

        [JsonProperty]
        public bool StartMinimized {
            get { return startMinimized; }
            set {
                if(value != startMinimized) {
                    startMinimized = value;
                    RaiseChanged(new SettingChangedEventArgs(typeof(AppSettings), nameof(StartMinimized)));
                }
            }
        }

        public AppSettings(string filePath) : base(filePath) {
            // ---
        }

        [JsonConstructor]
        protected AppSettings() : base() {
        }
    }
}