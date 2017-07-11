using System;
using System.Diagnostics;
using Microsoft.Win32;

namespace FreenetTray.Browsers
{
    class InternetExplorer: Browser
    {
        private static string IERegistryKey = @"Software\Microsoft\Internet Explorer";

        public InternetExplorer()
        {
            // See https://support.microsoft.com/kb/969393

            // NOTE: this is intentionally looking for the 32bit version, because it's very rare
            // for anyone to launch IE 64-bit on purpose. However, since we're running it in
            // private browsing mode and don't need plugins anyway, should we always prefer it?
            RegistryKey hive = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
            RegistryKey key = hive.OpenSubKey(IERegistryKey);

            if (key == null) {
                var value = key.GetValue("version") as string;

                if (value != null) {
                    _version = new Version(value);
                }
            }

            // TODO: Also check for existence of executable?
            _isInstalled = _version != null;

            // See https://en.wikipedia.org/wiki/Internet_Explorer_8#InPrivate
            _isUsable = _version >= new Version(8, 0);

            // See http://msdn.microsoft.com/en-us/library/ie/hh826025%28v=vs.85%29.aspx
            _args = "-private ";

            _path = "iexplore.exe";

            _name = "Internet Explorer";
        }
    }
}
