namespace novideo_srgb
{
    public class LutToneCurve : ToneCurve
    {
        private ushort[] _values;

        public LutToneCurve(ushort[] values)
        {
            _values = values;
        }

        public double SampleAt(double x)
        {
            if (x == 0) return (double)_values[0] / ushort.MaxValue;
            if (x >= 1) return (double)_values[_values.Length - 1] / ushort.MaxValue;

            var index = x * (_values.Length - 1);
            var frac = index - (uint)index;
            return (_values[(uint)index] * (1 - frac) + _values[(uint)index + 1] * frac) / ushort.MaxValue;
        }

        public double SampleInverseAt(double x)
        {
            var value = x * ushort.MaxValue;
            var lowValue = (ushort)value;

            if (_values[0] >= value) return 0;
            if (_values[_values.Length - 1] <= lowValue) return 1;

            var lowerIndex = -1;
            for (var i = 0; i < _values.Length; i++)
            {
                if (lowValue >= _values[i])
                {
                    lowerIndex = i;
                }
            }

            var low = _values[lowerIndex];
            var high = _values[lowerIndex + 1];
            var frac = (value - low) / (high - low);

            return (lowerIndex + frac) / (_values.Length - 1);
        }
    }
}