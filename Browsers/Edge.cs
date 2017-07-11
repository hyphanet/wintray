using System;
using System.IO;
using Microsoft.Win32;

namespace FreenetTray.Browsers {
    class Edge: Browser {

        private static string NTVersionRegistryKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";

        public Edge() {
            // for this key we want registry redirection enabled, so no registry view is used
            var reg = Registry.LocalMachine.OpenSubKey(NTVersionRegistryKey);

            string productName = reg.GetValue("ProductName") as string;

            // there's no version info but it isn't useful anyway
            _version = new System.Version(0, 0);

            _isInstalled = productName.StartsWith("Windows 10");

            _isUsable = _isInstalled;

            _args = "microsoft-edge:";

            // there is no .exe, instead a url like scheme is used with explorer.exe
            _path = Path.Combine(Environment.GetEnvironmentVariable("windir"), "explorer.exe");

            _name = "Edge";
        }

    }
}
