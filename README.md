## [Download latest release](https://github.com/ledoge/novideo_srgb/releases/latest/download/release.zip)

# About
This tool uses an undocumented NVIDIA API, supported on Fermi and later, to convert colors before sending them to a wide gamut monitor to effectively clamp it to sRGB (alternatively, Display P3 or Adobe RGB), based on the chromaticities provided in its EDID. AMD supports this as a hidden setting in their drivers, but NVIDIA doesn't because ???.

ICC profiles are also supported and can be used in two different ways. By default, only the primary coordinates from the ICC profile will be used instead of the values reported in the EDID. This can be useful if you want to use a profile created by someone else without taking their gamma/grayscale balance data into account, as that can vary a lot between units. If you enable the `Calibrate gamma to` checkbox, a full LUT-Matrix-LUT calibration will be applied. This is similar to the hardware calibration supported by some monitors and can be used to achieve great color and grayscale accuracy on well-behaved displays.

# Notes for use with EDID data
* If the checkbox for a monitor is locked, it means that the EDID is reporting the sRGB primaries as the monitor primaries, so the monitor is either natively sRGB or uses an sRGB emulation mode by default. If this is not the case, complain to the manufacturer about the EDID being wrong.

* The reported white point is not taken into account when calculating the color space conversion matrix. Instead, the monitor is always assumed to be calibrated to D65 white.

# Notes for use with ICC profiles

* For the gamma options to work properly, the profile must report the display's black point accurately. DisplayCAL's default settings, e.g. with the sRGB preset, work fine.
* Since the color space conversion is done on the GPU side, the ICC profile must not be selected/loaded in Windows or any other application. If you want, you can do another profiling run on top of the active calibration and then use this profile in applications that support color management to achieve even better color accuracy.
* Only the VCGT (if present), TRC and PCS matrix parts of an ICC profile are used. If present, the A2B1 data is used to calculate (hopefully) higher quality TRC and PCS matrix values.
