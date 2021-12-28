using System;

namespace novideo_srgb
{
    public class GammaToneCurve : ToneCurve
    {
        private readonly float _gamma;
        private readonly double _black;

        public GammaToneCurve(float gamma)
        {
            _gamma = gamma;
        }

        public GammaToneCurve(float gamma, double black)
        {
            _gamma = gamma;
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
            return Math.Pow(x, 1 / _gamma);
        }
    }
}