using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;

namespace FreenetTray.Browsers
{
    class Firefox: Browser
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

        // we check manually if searching the registry failed, these are the full paths we attempt to find
        private static readonly string[] FallbackLocations = {
                                                 @"%PROGRAMFILES%\Mozilla Firefox",
                                                 @"%PROGRAMFILES(X86)%\Mozilla Firefox",
                                               };

        public Firefox()
        {
            // try querying the registry first to find any installed version of firefox
            var currentVersion64 = CurrentVersionInRegistryView(RegistryView.Registry64);
            var currentVersion32 = CurrentVersionInRegistryView(RegistryView.Registry64);

            var parsedVersion64 = GetVersion(currentVersion64);
            var parsedVersion32 = GetVersion(currentVersion32);

            var isUsable64 = parsedVersion64 >= new Version(29, 0);
            var isUsable32 = parsedVersion32 >= new Version(29, 0);


            // check for executable paths, then decide which to prefer after we know what's available
            var path64 = ExecutablePathInRegistryView(RegistryView.Registry64, currentVersion64, parsedVersion64);
            var path32 = ExecutablePathInRegistryView(RegistryView.Registry32, currentVersion32, parsedVersion32);


            var pathfallback = FallbackLocations.Select(location => Environment.ExpandEnvironmentVariables(location) + @"\firefox.exe")
                .Where(File.Exists)
                .FirstOrDefault();

            // this explicitly allows for a case where the 64-bit version is installed but is not
            // considered usable, we fall back to the installed 32-bit version if it is usable, and then
            // fall back to whatever we find in the program files directory as a last resort
            if (path64 != null && isUsable64) {

                _path = path64;

                _version = parsedVersion64;

            } else if (path32 != null && isUsable32) {

                _path = path32;

                _version = parsedVersion32;

            } else if (pathfallback != null) {

                _path = pathfallback;

                // we don't know, but it's not critical
                _version = new System.Version(0, 0);
            }

            _isInstalled = _path != null;

            _isUsable = _version != null;

            /*
             * Firefox 29 and later support -private-window <URL>:
             *      "Open URL in a new private browsing window."
             *
             * See https://developer.mozilla.org/en-US/docs/Mozilla/Command_Line_Options?redirectlocale=en-US&redirectslug=Command_Line_Options#-private
             */
            _args = "-private-window ";

            _name = "Firefox";
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
