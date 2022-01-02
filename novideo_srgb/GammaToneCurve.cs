using System;

namespace novideo_srgb
{
    public class GammaToneCurve : ToneCurve
    {
        private readonly double _gamma;
        private readonly double _a = 1;
        private readonly double _b;
        private readonly double _c;

        public GammaToneCurve(double gamma)
        {
            _gamma = gamma;
        }

        public GammaToneCurve(double gamma, double black, bool relative = false)
        {
            _gamma = !relative || black == 0
                ? gamma
                : Math.Log((black - 1) * Math.Pow(2, gamma) / (black * Math.Pow(2, gamma) - 1), 2);
            _a = 1 - black;
            _c = black;
        }

        public GammaToneCurve(double gamma, double black, double outputOffset)
        {
            _gamma = gamma;

            if (black == 0) return;
            var outBlack = outputOffset * black;
            var lwg = Math.Pow(1 - outBlack, 1 / gamma);
            var lbg = Math.Pow(black - outBlack, 1 / gamma);
            _a = Math.Pow(lwg - lbg, gamma);
            _b = lbg / (lwg - lbg);
            _c = outBlack;
        }

        public double SampleAt(double x)
        {
            if (x >= 1) return 1;
            return _a * Math.Pow(Math.Max(x + _b, 0), _gamma) + _c;
        }

        public double SampleInverseAt(double x)
        {
            if (_a != 1) throw new NotSupportedException();
            if (x >= 1) return 1;
            return Math.Pow(x, 1 / _gamma);
        }
    }
}