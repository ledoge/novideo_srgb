using System;
using System.Runtime.InteropServices;

/*
 * Class for subscribing to WM_POWERBROADCAST messages
 * See https://docs.microsoft.com/en-us/windows/win32/power/power-setting-guids
 * for MS documentation on power broadcast values
 */

namespace novideo_srgb.PowerBroadcast
{
    public static class PowerBroadcastNotificationHelpers
    {
        private static readonly int WM_POWERBROADCAST = 0x0218;

        [DllImport(@"User32", SetLastError = true,
                              EntryPoint = "RegisterPowerSettingNotification",
                              CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr RegisterPowerSettingNotification(
            IntPtr hRecipient,
            ref Guid PowerSettingGuid,
            int Flags);

        public static void RegisterPowerBroadcastNotification(IntPtr handle, Guid PowerSetting) => RegisterPowerSettingNotification(handle, ref PowerSetting, 0x00);

        public static IntPtr HandleBroadcastNotification(int msg, IntPtr lParam, Action<PowerBroadcastMessage> action)
        {
            if (msg == WM_POWERBROADCAST)
            {
                var message = Marshal.PtrToStructure<PowerBroadcastMessage>(lParam);
                action(message);
            }

            return IntPtr.Zero;
        }
    }
}
