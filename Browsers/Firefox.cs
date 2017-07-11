using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Linq;

namespace FreenetTray.Browsers
{
    class Firefox : IBrowser
    {
        private static string CurrentVersionInRegistryView(RegistryView view) {
            string value = null;

            foreach (var keypair in VersionRegistryKeys) {
                RegistryKey hive = RegistryKey.OpenBaseKey(keypair.Item1, view);
                RegistryKey key = hive.OpenSubKey(keypair.Item2);
                if (key == null) {
                    return null;
                }

                value = key.GetValue("CurrentVersion") as string;

                if (value != null) {
                    break;
                }
            }

            return value;
        }

        private static string ExecutablePathInRegistryView(RegistryView view, string currentVersion, Version version) {
            if (currentVersion == null || version == null) {
                return null;
            }

            string value = null;

            foreach (var keypair in PathRegistryKeys) {
                RegistryKey hive = RegistryKey.OpenBaseKey(keypair.Item1, view);
                var query = string.Format(keypair.Item2
                    .Replace("{CurrentVersion}", "{0}")
                    .Replace("{VersionNumber}", "{1}"),
                     currentVersion, version);

                RegistryKey key = hive.OpenSubKey(query);
                if (key == null) {
                    return null;
                }

                value = key.GetValue("PathToExe") as string;

                if (value != null) {
                    break;
                }
            }

            return value;
        }


        /*
         * https://developer.mozilla.org/en-US/docs/Adding_Extensions_using_the_Windows_Registry
         * is out of date as of this writing - it uses "Mozilla Firefox" instead of "Firefox".
         * Earlier versions use HKEY_LOCAL_MACHINE but current ones use HKEY_CURRENT_USER.
         */
        private static readonly TupleList<RegistryHive, string> VersionRegistryKeys = new TupleList<RegistryHive, string> {
            // this is current as of firefox 54
            { RegistryHive.LocalMachine, @"SOFTWARE\Mozilla\Mozilla Firefox" },
            { RegistryHive.CurrentUser, @"SOFTWARE\Mozilla\Mozilla Firefox" },
            { RegistryHive.CurrentConfig, @"SOFTWARE\Mozilla\Mozilla Firefox" },
        };

        private static readonly TupleList<RegistryHive, string> PathRegistryKeys = new TupleList<RegistryHive, string> {
            // CurrentVersion is {VersionNumber} {Locale}. In these keys {VersionNumber}
            // and {CurrentVersion} are replaced before lookup.
            { RegistryHive.LocalMachine, @"SOFTWARE\Mozilla\Mozilla Firefox\{VersionNumber}\Main" },
            { RegistryHive.LocalMachine, @"SOFTWARE\Mozilla\Mozilla Firefox {VersionNumber}\bin" },
            { RegistryHive.CurrentUser, @"SOFTWARE\Mozilla\Mozilla Firefox\{CurrentVersion}\Main" },
            { RegistryHive.CurrentUser, @"SOFTWARE\Mozilla\Mozilla Firefox {VersionNumber}\bin" },
        };

        private readonly bool _isInstalled;
        private readonly Version _version;
        private readonly string _path;
        // we check manually if searching the registry failed, these are the full paths we attempt to find
        private static readonly string[] FallbackLocations = {
                                                 @"%PROGRAMFILES%\Mozilla Firefox",
                                                 @"%PROGRAMFILES(X86)%\Mozilla Firefox",
                                               };

        public Firefox()
        {
            var currentVersion = GetCurrentVersion();
            _version = GetVersion(currentVersion);

            _path = GetPath(currentVersion, _version);
            _isInstalled = _path != null;
        }

        public bool Open(Uri target)
        {
            if (!IsAvailable())
            {
                return false;
            }
            /*
             * Firefox 29 and later support -private-window <URL>:
             *      "Open URL in a new private browsing window."
             *
             * See https://developer.mozilla.org/en-US/docs/Mozilla/Command_Line_Options?redirectlocale=en-US&redirectslug=Command_Line_Options#-private
             */
            Process.Start(_path, "-private-window " + target);
            return true;
        }

        public bool IsAvailable()
        {
            return _isInstalled && _version >= new Version(29, 0);
        }

        public string GetName()
        {
            return "Firefox";
        }

        // Return null if the version cannot be determined.
        private static Version GetVersion(string currentVersion)
        {
            // TODO: Version.TryParse(), added in .NET 4, could make this the only null return.
            if (currentVersion == null)
            {
                return null;
            }

            try
            {
                // CurrentVersion contains "version.number (locale)"
                return new Version(currentVersion.Split(new[] { ' ' }, 2)[0]);
            }
            catch (OverflowException)
            {
            }
            catch (FormatException)
            {
            }
            catch (ArgumentOutOfRangeException)
            {
            }

            return null;
        }
    }
}
