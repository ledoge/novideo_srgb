namespace novideo_srgb.core.Models.ToneCurves;

public interface IToneCurve
{
    double SampleAt(double x);
    double SampleInverseAt(double x);
}
