// credit to https://mina86.com/2019/srgb-xyz-matrix/ and http://www.brucelindbloom.com/ for the math

namespace novideo_srgb.core.Models;

public struct Point
{
    public bool Equals(Point other)
    {
        return X.Equals(other.X) && Y.Equals(other.Y);
    }

    public override bool Equals(object? obj)
    {
        return obj is Point other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (X.GetHashCode() * 397) ^ Y.GetHashCode();
        }
    }

    public double X;
    public double Y;
}
