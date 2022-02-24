namespace novideo_srgb.core.Models
{
    public interface ToneCurve
    {
        double SampleAt(double x);
        double SampleInverseAt(double x);
    }
}