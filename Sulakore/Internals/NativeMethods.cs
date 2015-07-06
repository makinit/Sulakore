/* Copyright

    GitHub(Source): https://GitHub.com/ArachisH/Sulakore

    .NET library for creating Habbo Hotel related desktop applications.
    Copyright (C) 2015 ArachisH

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License along
    with this program; if not, write to the Free Software Foundation, Inc.,
    51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.

    See License.txt in the project root for license information.
*/

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