using MathNet.Numerics.LinearAlgebra;

// credit to https://mina86.com/2019/srgb-xyz-matrix/ for the math
namespace novideo_srgb
{
    public static class Colorimetry
    {
        public struct Point
        {
            public double x;
            public double y;
        }

        public struct ColorSpace
        {
            public Point red;
            public Point green;
            public Point blue;
            public Point white;
        }

        public static Point D65 = new Point {x = 0.312713, y = 0.329016};

        public static ColorSpace sRGB = new ColorSpace
        {
            red = new Point {x = 0.64, y = 0.33},
            green = new Point {x = 0.3, y = 0.6},
            blue = new Point {x = 0.15, y = 0.06},
            white = D65
        };

        public static ColorSpace P3Display = new ColorSpace
        {
            red = new Point {x = 0.68, y = 0.32},
            green = new Point {x = 0.265, y = 0.69},
            blue = new Point {x = 0.15, y = 0.06},
            white = D65
        };

        public static Matrix<double> ColorSpaceToXYZ(ColorSpace colorSpace)
        {
            var red = colorSpace.red;
            var green = colorSpace.green;
            var blue = colorSpace.blue;
            var white = colorSpace.white;
            var whiteXYZ = Matrix<double>.Build.DenseOfArray(new[,]
                {{white.x / white.y}, {1}, {(1 - white.x - white.y) / white.y}});

            var Mprime = Matrix<double>.Build.DenseOfArray(new[,]
            {
                {red.x / red.y, green.x / green.y, blue.x / blue.y},
                {1, 1, 1},
                {(1 - red.x - red.y) / red.y, (1 - green.x - green.y) / green.y, (1 - blue.x - blue.y) / blue.y}
            });

            return Mprime * Matrix<double>.Build.DiagonalOfDiagonalVector((Mprime.Inverse() * whiteXYZ).Column(0));
        }

        public static Matrix<double> XYZToColorSpace(ColorSpace colorSpace)
        {
            return ColorSpaceToXYZ(colorSpace).Inverse();
        }

        public static Matrix<double> ColorSpaceToColorSpace(ColorSpace from, ColorSpace to)
        {
            var result = XYZToColorSpace(to) * ColorSpaceToXYZ(from);
            result.CoerceZero(1e-14);
            return result;
        }
    }
}