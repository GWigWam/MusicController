using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerCore
{
    public static class Volume
    {
        /*
        Decibel is a logarithmic relative scale,
            with 0 being full volume and negative or positive values indicating quieter and louder sounds respectively.

        Digital audio uses linear 'Amplitude' values instead, expressed in NAudio as a float,
            with 0 being no sound, getting louder up to 1 (full volume) and increasing in loudness in the >1 range.

        A decibel value of -6 or +6 means one halving or doubling of volume, -12 or +12 is half or twice that again.

            Db      Linear
            -40     0.01
            -20     0.1
            -18     0.125
            -12     0.25
            -6      0.5
             0      1
             6      2
             12     4
             18     8
             20     10
             40     100
         */

        public static class Linear
        {
            /// <summary>
            /// Convert linear volume float to decibel. <br /> <br />
            /// <c>dB = 20 * log10(amplitude)</c>
            /// </summary>
            public static double ToDecibel(double linearValue) =>
                linearValue switch {
                    < 0 => throw new ArgumentException($"{nameof(linearValue)} must be >= 0", nameof(linearValue)),
                    0 => -96,
                    _ => 20f * Math.Log10(linearValue)
                };
        }

        public static class Decibel
        {
            /// <summary>
            /// Convert decibel volume float to a linear value. <br /> <br />
            /// <c>amplitude = 10^(dB / 20)</c>
            /// </summary>
            public static double ToLinear(double decibelValue)
                => Math.Pow(10, decibelValue / 20f);
        }
    }
}
