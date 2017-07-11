using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace FreenetTray.Browsers
{
    class Opera: Browser
    {

        private static string OperaRegistryKey = @"Software\Opera Software\Last Stable Install Path";


        private static string RegistryPathForView(RegistryView view) {
            RegistryKey hive = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, view);
            RegistryKey key = hive.OpenSubKey(OperaRegistryKey);
            if (key == null) {
                return null;
            }

            string value = key.GetValue("opera.exe") as string;

            return value;
        }

        // find the path to opera, preferring 64-bit if available
        private static string RegistryPath {
            get {
                if (Environment.Is64BitOperatingSystem) {
                    var path64 = RegistryPathForView(RegistryView.Registry64);

                    if (path64 != null) {
                        return path64;
                    }

                    var path32 = RegistryPathForView(RegistryView.Registry32);

                    if (path32 != null) {
                        return path32;
                    }

                    return null;
                }
                else {
                    var path32 = RegistryPathForView(RegistryView.Registry32);

                    if (path32 != null) {
                        return path32;
                    }

                    return null;
                }
            }
        }

        public Opera()
        {
            /*
             * TODO: Opera 26 adds launcher.exe and does not support -newprivatetab. Documentation
             * on what it supports in its place, if anything, has not been forthcoming.
             */
            // Key present with Opera 21.
            var possiblePath = RegistryPath;

            _isInstalled = File.Exists(possiblePath);
            if (_isInstalled)
            {
                _path = possiblePath;
            }

            // we don't know at the moment, but might need to add support to properly 
            // handle the private browsing argument when it works
            _version = new System.Version(0, 0);

            _isUsable = true;
            
            // See http://www.opera.com/docs/switches
            _args = "-newprivatetab ";

            _name = "Opera";
        }
    }
}
