using PlayerCore;
using PlayerCore.PlayerEventArgs;
using PlayerCore.Settings;
using PlayerCore.Songs;
using PlayerInterface.Commands;
using System;
using System.ComponentModel;
using System.Timers;
using System.Windows.Input;
using System.Windows.Media;

namespace PlayerInterface.ViewModels {

    public class SmallPlayerViewModel {

        public PlayingVm Playing { get; }

        public NextPrevVm NextPrev { get; }

        public SmallPlayerViewModel(PlayingVm playingVm, NextPrevVm nextPrevVm) {
            Playing = playingVm;
            NextPrev = nextPrevVm;
        }
    }
}
