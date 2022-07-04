using System;
using System.Runtime.InteropServices;

namespace novideo_srgb.PowerBroadcast
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct PowerBroadcastMessage
    {
        public Guid PowerSetting;
        public uint DataLength;
        public byte Data;
    }
}
