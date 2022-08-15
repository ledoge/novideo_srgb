## [Download latest release](https://github.com/ledoge/novideo_srgb/releases/latest/download/release.zip)

# About
This tool uses an undocumented NVIDIA API, supported on Fermi and later, to convert colors before sending them to a wide gamut monitor to effectively clamp it to sRGB (alternatively: Display P3, Adobe RGB or BT.2020), based on the chromaticities provided in its EDID. AMD supports this as a hidden setting in their drivers, but NVIDIA doesn't because ???.

ICC profiles are also supported and can be used in two different ways. By default, only the primary coordinates from the ICC profile will be used in place of the values reported in the EDID. This is useful if you want to use a profile created by someone else without taking their gamma/grayscale balance data into account, as that can vary a lot between units. If you enable the `Calibrate gamma to` checkbox, a full LUT-Matrix-LUT calibration will be applied. This is similar to the hardware calibration supported by some monitors and can be used to achieve great color and grayscale accuracy on well-behaved displays.

# Usage
Extract `release.zip` somewhere under your user directory and run `novideo_srgb.exe`. To enable/disable the sRGB clamp for a monitor, simply toggle the "Clamped" checkbox. For using ICC profiles and configuring dithering, click the "Advanced" button.

Generally, the clamp should persist through reboots and driver updates, but it can break sometimes. You can choose to leave the application running minimized in the background to have it automatically reapply the clamp and also handle HDR toggling – see the section "HDR and automatic reapplying" below. 

# Notes for use with EDID data
* If the checkbox for a monitor is locked, it means that the EDID is reporting the sRGB primaries as the monitor's primaries, so the monitor is either natively sRGB or uses an sRGB emulation mode by default. If this is not the case, complain to the manufacturer about the EDID being wrong, and try to find an ICC profile for your monitor to use instead of the EDID data.

* The reported white point is not taken into account when calculating the color space conversion matrix. Instead, the monitor is always assumed to be calibrated to D65 white.

# Notes for use with ICC profiles

* For the gamma options to work properly, the profile must report the display's black point accurately. DisplayCAL's default settings, e.g. with the sRGB preset, work fine.
* Since the color space conversion is done on the GPU side, the ICC profile must not be selected/loaded in Windows or any other application. If you want, you can do another profiling run on top of the active calibration and then use this profile in applications that support color management to achieve even better color accuracy.
* To achieve optimal results, consider creating a custom testchart in DisplayCAL with a high number of neutral (grayscale) patches, such as 256. With that, a grayscale calibration (setting "Tone curve" to anything other than "As measured") should be unnecessary unless your display lacks RGB gain controls, but can lead to better accuracy, especially on poorly behaved displays. The number of colored patches should not matter much. Additionally, configuring DisplayCAL to generate a "Curves + matrix" profile with "Black point compensation" disabled should also result in a lower average error than using an XYZ LUT profile. This advice is based on what worked well for a handful of users, so if you have anything else to add, please let me know.
* The option "Disable 8-bit color optimization" can be used to get better color accuracy in true 10-bit workflows at the cost of 8-bit accuracy. Only enable this if you really know you're working with 10-bit color.
* Only the VCGT (if present), TRC and PCS matrix parts of an ICC profile are used. If present, the A2B1 data is used to calculate (hopefully) higher quality TRC and PCS matrix values.

# HDR and automatic reapplying

Any change in the display setup (such as a monitor being added/removed) will cause the clamp to be reapplied on all monitors, as long as the application is running in the background. The main purpose of this is to handle HDR being toggled in Windows, as the clamp will automatically be disabled for monitors for which HDR is enabled (since colors would get messed up otherwise). Additionally, you can use the "Reapply" button to manually reapply the clamp in case something breaks (e.g. due to a driver bug).

Minimizing the GUI will hide it from the taskbar, so that it'll only be visible in the tray. If you want to run it on boot, e.g. via the `Startup` folder, you can use the `-minimize` command line argument to have it start up minimized.

# Known issues

* The color space transform does not get applied properly to the mouse cursor, which results in it having wrong gamma and colors. This should be hardly noticeable with the default Windows cursor. Workaround: Force software rendering of the cursor, e.g. using [SoftCursor](https://www.monitortests.com/forum/Thread-SoftCursor).

* Windows HDR is handled properly, but NVAPI HDR, which some applications use to output HDR even though Windows HDR is off, will result in wrong colors while the clamp is active. To work around this, you can either enable Windows HDR or disable the clamp manually before launching such applications.

# Dithering

Applying any kind of calibration on the GPU-level usually results in banding unless dithering is used. By default, NVIDIA GPUs do not apply dithering to full range RGB output. Therefore, it is recommended that you use the dither controls to enable and configure dithering. "Bits" should be set to match the bit depth of your GPU output, and "Mode" can be set to whatever looks best to you. Note that "Temporal" works by rapidly switching between colors, which some people's eyes are sensitive to.
