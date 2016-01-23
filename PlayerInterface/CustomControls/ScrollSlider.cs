using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace PlayerInterface.CustomControls {

    public class ScrollSlider : Slider {

        public ScrollSlider() : base() {
            MouseWheel += ScrollSlider_MouseWheel;
        }

        private void ScrollSlider_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e) {
            if(e.Delta > 0) {
                if(Value + LargeChange > Maximum) {
                    Value = Maximum;
                } else {
                    Value += LargeChange;
                }
            } else if(e.Delta < 0) {
                if(Value - LargeChange < Minimum) {
                    Value = Minimum;
                } else {
                    Value -= LargeChange;
                }
            }
        }
    }
}