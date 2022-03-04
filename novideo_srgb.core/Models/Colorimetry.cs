// credit to https://mina86.com/2019/srgb-xyz-matrix/ and http://www.brucelindbloom.com/ for the math

namespace novideo_srgb.core.Models;

internal static partial class Colorimetry
{
    public static Matrix XYZToRGB(ColorSpace colorSpace) => RGBToXYZ(colorSpace).Inverse();
    public static Matrix RGBToRGB(ColorSpace from, ColorSpace to) => XYZToRGB(to) * RGBToXYZ(from);
    public static Matrix RGBToPCSXYZ(ColorSpace colorspace) => RGBToAdaptedXYZ(colorspace, ColorSpaces.D50);
    public static Matrix PCSXYZToRGB(ColorSpace colorspace) => RGBToPCSXYZ(colorspace).Inverse();
    public static Matrix XYZScaleToD50(Matrix matrix) => XYZScale(matrix, ColorSpaces.D50);

    public static Matrix RGBToXYZ(ColorSpace colorSpace)
    {
        var red = colorSpace.Red;
        var green = colorSpace.Green;
        var blue = colorSpace.Blue;
        var white = colorSpace.White;
        var whiteXYZ = Matrix.FromValues(new[,]
            { { white.X / white.Y }, { 1 }, { (1 - white.X - white.Y) / white.Y } });

        var Mprime = Matrix.FromValues(new[,]
        {
            { red.X / red.Y, green.X / green.Y, blue.X / blue.Y },
            { 1, 1, 1 },
            { (1 - red.X - red.Y) / red.Y, (1 - green.X - green.Y) / green.Y, (1 - blue.X - blue.Y) / blue.Y }
        });

        return Mprime * Matrix.FromDiagonal(Mprime.Inverse() * whiteXYZ);
    }

    public static Matrix RGBToAdaptedXYZ(ColorSpace colorspace, Matrix whiteXYZ)
    {
        var xyz = RGBToXYZ(colorspace);
        var bradford = Matrix.FromValues(new[,]
        {
            { 0.8951, 0.2664, -0.1614 },
            { -0.7502, 1.7135, 0.0367 },
            { 0.0389, -0.0685, 1.0296 }
        });
        var ws = colorspace.White;
        var aws = bradford * Matrix.FromValues(new[,]
        {
            { ws.X / ws.Y }, { 1 }, { (1 - ws.X - ws.Y) / ws.Y }
        });
        var awd = bradford * whiteXYZ;
        var m = bradford.Inverse() * Matrix.FromDiagonal(new[]
            { awd[0] / aws[0], awd[1] / aws[1], awd[2] / aws[2] }) * bradford;
        return m * xyz;
    }

    public static Matrix XYZScale(Matrix matrix, Matrix target)
    {
        var result = Matrix.Zero3x3();
        var white = matrix * Matrix.One3x1();
        for (var i = 0; i < 3; i++)
        {
            for (var j = 0; j < 3; j++)
            {
                result[i, j] = matrix[i, j] * target[i] / white[i];
            }
        }

        return result;
    }

}
