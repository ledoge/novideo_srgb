using MathNet.Numerics.LinearAlgebra;

// credit to https://mina86.com/2019/srgb-xyz-matrix/ for the math
namespace novideo_srgb
{
    public static class Colorimetry
    {
        public struct Point
        {
            public double X;
            public double Y;
        }

        public struct ColorSpace
        {
            public Point Red;
            public Point Green;
            public Point Blue;
            public Point White;
        }

        public static Point D65 = new Point {X = 0.3127, Y = 0.3290};

        public static ColorSpace sRGB = new ColorSpace
        {
            Red = new Point {X = 0.64, Y = 0.33},
            Green = new Point {X = 0.3, Y = 0.6},
            Blue = new Point {X = 0.15, Y = 0.06},
            White = D65
        };

        public static ColorSpace P3Display = new ColorSpace
        {
            Red = new Point {X = 0.68, Y = 0.32},
            Green = new Point {X = 0.265, Y = 0.69},
            Blue = new Point {X = 0.15, Y = 0.06},
            White = D65
        };

        public static Matrix<double> RGBToXYZ(ColorSpace colorSpace)
        {
            var red = colorSpace.Red;
            var green = colorSpace.Green;
            var blue = colorSpace.Blue;
            var white = colorSpace.White;
            var whiteXYZ = Matrix<double>.Build.DenseOfArray(new[,]
                {{white.X / white.Y}, {1}, {(1 - white.X - white.Y) / white.Y}});

            var Mprime = Matrix<double>.Build.DenseOfArray(new[,]
            {
                {red.X / red.Y, green.X / green.Y, blue.X / blue.Y},
                {1, 1, 1},
                {(1 - red.X - red.Y) / red.Y, (1 - green.X - green.Y) / green.Y, (1 - blue.X - blue.Y) / blue.Y}
            });

            return Mprime * Matrix<double>.Build.DiagonalOfDiagonalVector((Mprime.Inverse() * whiteXYZ).Column(0));
        }

        public static Matrix<double> XYZToRGB(ColorSpace colorSpace)
        {
            return RGBToXYZ(colorSpace).Inverse();
        }

        public static Matrix<double> RGBToRGB(ColorSpace from, ColorSpace to)
        {
            var result = XYZToRGB(to) * RGBToXYZ(from);
            result.CoerceZero(1e-14);
            return result;
        }
    }
}