using System;

namespace novideo_srgb
{
    public class GammaToneCurve : ToneCurve
    {
        private readonly double _gamma;
        private readonly double _black;

        public GammaToneCurve(double gamma)
        {
            _gamma = gamma;
        }

        public GammaToneCurve(double gamma, double black, bool relative = false)
        {
            _gamma = !relative
                ? gamma
                : Math.Log((black - 1) * Math.Pow(2, gamma) / (black * Math.Pow(2, gamma) - 1), 2);
            _black = black;
        }

        public double SampleAt(double x)
        {
            if (_black == 0) return Math.Pow(x, _gamma);

            if (x >= 1) return 1;
            return Math.Pow(x, _gamma) * (1 - _black) + _black;
        }

        public double SampleInverseAt(double x)
        {
            if (_black != 0) throw new NotSupportedException();
            if (x >= 1) return 1;
            return Math.Pow(x, 1 / _gamma);
        }
    }
}