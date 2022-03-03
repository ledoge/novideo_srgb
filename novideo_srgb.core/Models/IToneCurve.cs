namespace novideo_srgb.core.Models;

public interface IToneCurve
{
    double SampleAt(double x);
    double SampleInverseAt(double x);
}
