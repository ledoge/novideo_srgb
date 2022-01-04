# About
This tool uses an undocumented NVIDIA API, supported on Fermi and later, to convert colors before sending them to a wide gamut monitor to effectively clamp it to sRGB (based on the chromaticities provided in its EDID). AMD supports this as a hidden setting in their drivers, but NVIDIA doesn't because ???.

ICC profiles are also supported and can either be used to only remap the gamut (which is probably what you want when using one not created by yourself) or for a full LUT-Matrix-LUT calibration, which should lead to great grayscale and color accuracy on well-behaved displays.

# Notes for use with EDID data
* If the checkbox for a monitor is locked, it means that the EDID is reporting the sRGB primaries as the monitor primaries, so the monitor is either natively sRGB or uses an sRGB emulation mode by default. If this is not the case, complain to the manufacturer about the EDID being wrong.

* The reported white point is not taken into account when calculating the color space conversion matrix. Instead, the monitor is always assumed to be calibrated to D65 white.

# Notes for use with ICC profiles

* To achieve a full calibration, you must enable the `Calibrate gamma to` checkbox and select your desired gamma target. This is independent from the gamma you chose to calibrate to when creating the profile.
* For the gamma options to work properly, the profile's tone response curves must report the black point accurately. Using DisplayCAL, this means that `Black point compensation` must be disabled and `Profile type` has to be set to `Curves + matrix` or `Single curve + matrix` and (enable `Show advanced options` under `Options` to see these settings). For the former, leaving `Tone curve` set to `As measured` should be fine, but for the latter, it should be set to some (any) target to ensure grayscale neutrality.
* Since the color space conversion is done on the GPU side, the ICC profile must not be selected/loaded in Windows or any other application. If you want, you can do another profiling run on top of the active calibration and then use this profile in applications that support color management to achieve even better color accuracy.
* Only the VCGT (if present), TRC and PCS matrix parts of an ICC profile are used.
