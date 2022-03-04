// credit to https://mina86.com/2019/srgb-xyz-matrix/ and http://www.brucelindbloom.com/ for the math

namespace novideo_srgb.core.Models;

public struct ColorSpace
{
    public bool Equals(ColorSpace other)
    {
        return Red.Equals(other.Red) && Green.Equals(other.Green) && Blue.Equals(other.Blue) &&
                White.Equals(other.White);
    }

    public override bool Equals(object? obj)
    {
        return obj is ColorSpace other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Red.GetHashCode();
            hashCode = (hashCode * 397) ^ Green.GetHashCode();
            hashCode = (hashCode * 397) ^ Blue.GetHashCode();
            hashCode = (hashCode * 397) ^ White.GetHashCode();
            return hashCode;
        }
    }

    public Point Red;
    public Point Green;
    public Point Blue;
    public Point White;
}
