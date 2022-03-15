using System.Runtime.InteropServices;

namespace novideo_srgb.core.PowerBroadcast;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct PowerBroadcastMessage
{
    public Guid PowerSetting;
    public uint DataLength;
    public byte Data;
}
