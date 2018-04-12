using System;
using Microsoft.Win32;

namespace FreenetTray {
    public enum JREType {
        None,
        JRE64Bit,
        JRE32Bit,
    }

    public class MissingJRE: Exception { }

    public class MachineConfig {

        private static string JRERegistryKey = @"Software\\JavaSoft\\Java Runtime Environment";
        private static string JRERegistryKey2 = @"Software\\JavaSoft\\JRE";
        private static string[] JREKeys = {JRERegistryKey, JRERegistryKey2};

        private static bool Has64BitJRE {
            get {
                RegistryKey local64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                foreach (var key in JREKeys)
                {
                    if (local64.OpenSubKey(key) != null)
                        return true;
                }
                return false;
            }
        }

        private static bool Has32BitJRE {
            get {
                RegistryKey local32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                RegistryKey jreKey32 = local32.OpenSubKey(JRERegistryKey);
                if (jreKey32 != null) {
                    return true;
                }
                return false;
            }
        }

        // find the best available JRE
        public static JREType GetBestAvailableJRE {
            get {
                if (Environment.Is64BitOperatingSystem) {
                    // if we're on a 64-bit OS and have a 64-bit JRE we want to use it
                    if (Has64BitJRE) {
                        FNLog.Debug("64-bit JRE found");
                        return JREType.JRE64Bit;
                    }
                }

                // if we're on 32-bit OS or no 64-bit JRE was found, we have no other choice
                // but to try the 32-bit JRE
                if (Has32BitJRE) {
                    FNLog.Debug("32-bit JRE found");

                    return JREType.JRE32Bit;
                }

                // no JRE found, this could happen in practice if the only available JRE was uninstalled
                FNLog.Debug("No JRE found");

                return JREType.None;
            }
        }
    }
}
