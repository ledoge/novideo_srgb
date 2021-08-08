# About
This tool uses an undocumented NVIDIA API, supported on Fermi and later, to convert colors before sending them to a wide gamut monitor to effectively clamp it to sRGB (based on the chromaticities provided in its EDID). AMD supports this as a hidden setting in their drivers, but NVIDIA doesn't because ???.

I'm not an expert on color management and don't even have a wide gamut monitor to test this properly, so do let me know if this works well for you.
