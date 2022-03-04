namespace novideo_srgb.core.Models;

public static class ColorSpaces
{
    public static readonly Point D65 = new Point { X = 0.3127, Y = 0.3290 };

    public static readonly Matrix D50 = Matrix.FromValues(new[,] { { 0.9642 }, { 1 }, { 0.8249 } });

    public static readonly ColorSpace sRGB = new ColorSpace
    {
        Red = new Point { X = 0.64, Y = 0.33 },
        Green = new Point { X = 0.3, Y = 0.6 },
        Blue = new Point { X = 0.15, Y = 0.06 },
        White = D65
    };

    public static readonly ColorSpace DisplayP3 = new ColorSpace
    {
        Red = new Point { X = 0.68, Y = 0.32 },
        Green = new Point { X = 0.265, Y = 0.69 },
        Blue = new Point { X = 0.15, Y = 0.06 },
        White = D65
    };

    public static readonly ColorSpace AdobeRGB = new ColorSpace
    {
        Red = new Point { X = 0.64, Y = 0.33 },
        Green = new Point { X = 0.21, Y = 0.71 },
        Blue = new Point { X = 0.15, Y = 0.06 },
        White = D65
    };

    public static readonly ColorSpace[] AllColorSpaces = new[] { sRGB, DisplayP3, AdobeRGB };
}
