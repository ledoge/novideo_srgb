## [Download latest release](https://github.com/ledoge/novideo_srgb/releases/latest/download/release.zip)

# About
This tool uses an undocumented NVIDIA API, supported on Fermi and later, to convert colors before sending them to a wide gamut monitor to effectively clamp it to sRGB (alternatively: Display P3, Adobe RGB or BT.2020), based on the chromaticities provided in its EDID. AMD supports this as a hidden setting in their drivers, but NVIDIA doesn't because ???.

ICC profiles are also supported and can be used in two different ways. By default, only the primary coordinates from the ICC profile will be used in place of the values reported in the EDID. This is useful if you want to use a profile created by someone else without taking their gamma/grayscale balance data into account, as that can vary a lot between units. If you enable the `Calibrate gamma to` checkbox, a full LUT-Matrix-LUT calibration will be applied. This is similar to the hardware calibration supported by some monitors and can be used to achieve great color and grayscale accuracy on well-behaved displays.

# Usage
Extract `release.zip` somewhere and run `novideo_srgb.exe`. To enable/disable the sRGB clamp for a monitor, simply toggle the "Clamped" checkbox. For using ICC profiles and configuring dithering, click the "Advanced" button.

# Notes for use with EDID data
* If the checkbox for a monitor is locked, it means that the EDID is reporting the sRGB primaries as the monitor's primaries, so the monitor is either natively sRGB or uses an sRGB emulation mode by default. If this is not the case, complain to the manufacturer about the EDID being wrong, and try to find an ICC profile for your monitor to use instead of the EDID data.

* The reported white point is not taken into account when calculating the color space conversion matrix. Instead, the monitor is always assumed to be calibrated to D65 white.

# Notes for use with ICC profiles

* For the gamma options to work properly, the profile must report the display's black point accurately. DisplayCAL's default settings, e.g. with the sRGB preset, work fine.
* Since the color space conversion is done on the GPU side, the ICC profile must not be selected/loaded in Windows or any other application. If you want, you can do another profiling run on top of the active calibration and then use this profile in applications that support color management to achieve even better color accuracy.
* To achieve optimal results, consider creating a custom testchart in DisplayCAL with a high number of neutral (grayscale) patches, such as 256. With that, a grayscale calibration (setting "Tone curve" to anything other than "As measured") should be unnecessary unless your display lacks RGB gain controls, and might even be detrimental to the accuracy. The number of colored patches should not matter much. Additionally, configuring DisplayCAL to generate a "Curves + matrix" profile with "Black point compensation" disabled should also result in a lower average error than using an XYZ LUT profile. This advice is based on what worked well for a handful of users, so if you have anything else to add, please let me know.
* The option "Disable 8-bit color optimization" can be used to get better color accuracy in true 10-bit workflows at the cost of 8-bit accuracy. Only enable this if you really know you're working with 10-bit color.
* Only the VCGT (if present), TRC and PCS matrix parts of an ICC profile are used. If present, the A2B1 data is used to calculate (hopefully) higher quality TRC and PCS matrix values.

# Dithering

Applying any kind of calibration on the GPU-level usually results in banding unless dithering is used. By default, NVIDIA GPUs do not apply dithering to full range RGB output. Therefore, it is recommended that you use the dither controls to enable and configure dithering. "Bits" should be set to match the bit depth of your GPU output, and "Mode" can be set to whatever looks best to you. Note that "Temporal" works by rapidly switching between colors, which some people's eyes are sensitive to.
