using System;
using System.Runtime.InteropServices;

namespace Sulakore
{
    internal static class NativeMethods
    {
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("WinInet.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool InternetSetOption(IntPtr hInternet, InternetOption dwOption, IntPtr lpBuffer, int dwBufferLength);
    }
}