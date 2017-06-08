using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerInterface.ViewModels {

    public class SongMenuItemViewModel {
        public string Title { get; }

        public Action Execute { get; }

        public SongMenuItemViewModel(string title, Action action) {
            Title = title;
            Execute = action;
        }
    }
}