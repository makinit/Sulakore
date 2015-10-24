using System;
using System.Runtime.InteropServices;

using Microsoft.Win32;

namespace Sulakore
{
    internal static class NativeMethods
    {
        private static RegistryKey _proxyRegistry;
        private static RegistryKey ProxyRegistry
        {
            get
            {
                if (_proxyRegistry == null)
                {
                    _proxyRegistry = Registry.CurrentUser
                        .OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", true);
                }
                return _proxyRegistry;
            }
        }

        public static bool IsSslSupported { get; set; }
        public static bool IsProxyRegistered
        {
            get
            {
                int proxyEnable = (int)(ProxyRegistry
                    .GetValue("ProxyEnable") ?? 0);

                return proxyEnable == 1;
            }
        }

        [DllImport("wininet.dll")]
        public static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int dwBufferLength);

        public static void TerminateProxy()
        {
            if (ProxyRegistry.GetValue("ProxyEnable") != null)
                ProxyRegistry.SetValue("ProxyEnable", 0);

            RefreshIESettings();
        }
        public static void InitiateProxy(int port)
        {
            string proxyServer = string.Format("http=127.0.0.1:{0};{1}",
                port, IsSslSupported ? "https=127.0.0.1:" + port : string.Empty);

            ProxyRegistry.SetValue("ProxyServer", proxyServer);
            ProxyRegistry.SetValue("ProxyOverride", "<-loopback>;<local>");

            ProxyRegistry.SetValue("ProxyEnable", 1);
            RefreshIESettings();
        }

        private static void RefreshIESettings()
        {
            InternetSetOption(IntPtr.Zero, 39, IntPtr.Zero, 0);
            InternetSetOption(IntPtr.Zero, 37, IntPtr.Zero, 0);
        }
    }
}