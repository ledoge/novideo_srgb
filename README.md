# About
This tool uses an undocumented NVIDIA API, supported on Fermi and later, to convert colors before sending them to a wide gamut monitor to effectively clamp it to sRGB (based on the chromaticities provided in its EDID). AMD supports this as a hidden setting in their drivers, but NVIDIA doesn't because ???.

I'm not an expert on color management and don't even have a wide gamut monitor to test this properly, so do let me know if this works well for you.

# Notes
* If the checkbox for a monitor is locked, it means that the EDID has the sRGB flag set, so the monitor is either natively sRGB or uses an sRGB emulation mode by default. If this is not the case, complain to the manufacturer about the EDID being wrong. Also, it seems like some monitors (such as certain LG models) have the sRGB flag set while still reporting non-sRGB primaries. This violates the spec, but you should be able to use [this build](https://github.com/ledoge/novideo_srgb/releases/tag/v0.3a) which ignores the flag and just uses the reported primaries anyway.

* The reported white point is not taken into account when calculating the color space conversion matrix. Instead, the monitor is always assumed to be calibrated to D65 white.
