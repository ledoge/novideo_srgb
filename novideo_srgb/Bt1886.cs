using System;

namespace novideo_srgb
{
    public class Bt1886 : ToneCurve
    {
        private const double G = 2.4;
        private double _a;
        private double _b;

        public Bt1886(double black)
        {
            _a = Math.Pow(1 - Math.Pow(black, 1 / G), G);
            _b = Math.Pow(black, 1 / G) / (1 - Math.Pow(black, 1 / G));
        }

        public double SampleAt(double x)
        {
            return _a * Math.Pow(Math.Max(x + _b, 0), G);
        }

        public double SampleInverseAt(double x)
        {
            if (!(_a == 1 && _b == 0)) throw new NotSupportedException();
            if (x >= 1) return 1;
            return Math.Pow(x, 1 / G);
        }
    }
}