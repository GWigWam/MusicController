using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerInterface.ViewModels {

    public class SongMenuItemViewModel {
        public string Title { get; set; }

        public Action<SongViewModel> Action { get; set; }

        public void Execute(SongViewModel svm) {
            if(svm != null) {
                Action?.Invoke(svm);
            } else {
                throw new ArgumentNullException(nameof(svm));
            }
        }
    }
}