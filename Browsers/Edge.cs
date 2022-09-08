using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace FreenetTray.Browsers {
    class Edge: Browser {

        private static string NTVersionRegistryKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";

        public Edge() {
            RegistryKey reg = null;

            if (Environment.Is64BitOperatingSystem) {
                RegistryKey local64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                reg = local64.OpenSubKey(NTVersionRegistryKey);
            } else {
                RegistryKey local32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                reg = local32.OpenSubKey(NTVersionRegistryKey);
            }

            if (reg != null) {
                string productName = reg.GetValue("ProductName") as string;

                _isInstalled = productName.StartsWith("Windows 10");
            } else {
                _isInstalled = false;
            }

            _isUsable = _isInstalled;

            // there's no version info but it isn't useful anyway
            _version = new System.Version(0, 0);

            _args = "microsoft-edge:";

            // there is no .exe, instead a url like scheme is used with explorer.exe
            _path = Path.Combine(Environment.GetEnvironmentVariable("windir"), "explorer.exe");

            _name = "Edge";
        }

        public override bool Open(Uri target)
        {
            if (!IsAvailable())
            {
                return false;
            }

            // Edge needs qoutes around the argument
            Process.Start(_path, string.Format("\"{0}\"", _args + target));
            return true;
        }

    }
}
