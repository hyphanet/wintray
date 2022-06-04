using System;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;


namespace FreenetTray {
    public enum JREType {
        None,
        JRE64Bit,
        JRE32Bit,
    }

    public class MissingJRE: Exception { }

    public class MachineConfig
    {

        private static bool isJVM8 = false;

        ///<summary>
        /// Read the wrapper.conf file. 
        /// Read java.command, run it to detect x86/x64 JVM and java version.
        /// Then adjust wrapper.conf with correct launch parameters
        ///  for Java 8 or Java 9+.
        ///</summary>
        public static void CheckConfig(string pathConfig)
        {
            string pathJVM;
            string[] lines = System.IO.File.ReadAllLines(pathConfig);

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                /*
                 * CHECK AND RUN JAVA COMMAND
                 * 
                 */
                if (line.StartsWith("wrapper.java.command="))
                {
                    string[] javcmd = Regex.Split(line, @"wrapper.java.command=");
                    if (javcmd.Length == 2)
                    {
                        if (javcmd[1].Length == 0)  // empty string: no custom java.exe
                        {
                            lines[i] = "wrapper.java.command=java";
                            pathJVM = "java";
                        }
                        else
                        {
                            pathJVM = javcmd[1];
                        }

                        // launch JVM to get more info
                        GetJVMinfo(pathJVM);
                    }
                    else
                    {
                        FNLog.Debug("ERROR: Invalid 'wrapper.java.command' in wrapper.conf.");
                    }
                }


                /*
                 * ENABLE OR DISABLE LAUNCH PARAMETERS FOR JAVA8 OR JAVA 9+
                 * 
                 */
                if (line.StartsWith("wrapper.java.additional."))
                {
                    if (isJVM8 &&
                        (line.Contains("--illegal-access=permit") ||
                        line.Contains("--add-opens=java.base/java.lang=ALL-UNNAMED") ||
                        line.Contains("--add-opens=java.base/java.util=ALL-UNNAMED") ||
                        line.Contains("--add-opens=java.base/java.io=ALL-UNNAMED")))
                    {
                        lines[i] = "# " + line;
                        FNLog.Debug("Disabling additional launch parameters.");
                    }
                }
                else if (line.StartsWith("#wrapper.java.additional.") ||
                          line.StartsWith("# wrapper.java.additional."))
                {
                    if (!isJVM8 &&
                        (line.Contains("--illegal-access=permit") ||
                        line.Contains("--add-opens=java.base/java.lang=ALL-UNNAMED") ||
                        line.Contains("--add-opens=java.base/java.util=ALL-UNNAMED") ||
                        line.Contains("--add-opens=java.base/java.io=ALL-UNNAMED")))
                    {
                        lines[i] = line.Replace("# ", "").Replace("#", "");
                        FNLog.Debug("Enabling additional launch parameters.");
                    }
                }
            }

            System.IO.File.WriteAllLines(pathConfig, lines);    // overwrites file without warning
        }

        private static void GetJVMinfo(string pathJVM)
        {
            using (System.Diagnostics.Process pProcess = new System.Diagnostics.Process())
            {
                pProcess.StartInfo.FileName = pathJVM;
                pProcess.StartInfo.Arguments = "-XshowSettings:all";
                pProcess.StartInfo.UseShellExecute = false;
                pProcess.StartInfo.RedirectStandardOutput = true;
                pProcess.StartInfo.RedirectStandardError = true; // java.exe writes this to StdErr
                pProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                pProcess.StartInfo.CreateNoWindow = true;

                string output = string.Empty;
                pProcess.OutputDataReceived += (sender, args) => output += args.Data;
                pProcess.ErrorDataReceived += (sender, args) => output += args.Data;

                try
                {
                    pProcess.Start();
                    pProcess.BeginOutputReadLine();
                    pProcess.BeginErrorReadLine();
                    pProcess.WaitForExit(5 * 1000); // 5s timeout


                    /* ================================
                     * CHECK JAVA.SPECIFICATION.VERSION
                     * 
                     */

                    // jvmVersion will be "" in case nothing was found in output
                    // can be "1.8" for Java 8, or, starting with Java 9, just "9", "10", ...
                    // you must not use "java.version" as this can be different for early access versions ("19-ea" instead of "19")
                    string jvmVersion = Regex.Match(output, @"java\.specification\.version = (\S+)").Groups[1].Value;
                    if (jvmVersion.Length != 0)
                    {
                        try
                        {
                            float fjvmVersion = float.Parse(jvmVersion, CultureInfo.InvariantCulture);
                            if (fjvmVersion < 1.9)
                            {
                                isJVM8 = true;
                                FNLog.Debug("JVM 8 detected.");
                            }
                            else
                            {
                                isJVM8 = false;
                                FNLog.Debug("JVM 9 or higher detected.");
                            }
                        }
                        catch (Exception e)
                        {
                            FNLog.Debug("ERROR converting to float:");
                            FNLog.Debug(e.ToString());
                        }
                    }
                    else
                    {
                        // inform user, but try starting Freenet anyway
                        // TODO: Adjust this in the future to default to JVM9+
                        FNLog.Debug("No java.specification.version found. Assuming JVM8.");
                        isJVM8 = true;
                    }



                    /* ================================
                     * CHECK JAVA 64-BIT OR 32-BIT
                     * 
                     */

                    // osArch will be "" in case nothing was found in output
                    string osArch = Regex.Match(output, @"os\.arch = (\w+)").Groups[1].Value;
                    if (osArch.Length != 0)
                    {
                        // os.arch on Windows can have four values: https://github.com/openjdk/jdk/blob/master/src/java.base/windows/native/libjava/java_props_md.c
                        if (osArch.Equals("amd64") || osArch.Equals("aarch64"))
                        {
                            GetBestAvailableJRE = JREType.JRE64Bit;
                            FNLog.Debug("64-bit JVM found");
                        }
                        else if (osArch.Equals("x86"))
                        {
                            GetBestAvailableJRE = JREType.JRE32Bit;
                            FNLog.Debug("32-bit JVM found");
                        }
                    }
                    else
                    {
                        GetBestAvailableJRE = JREType.None;
                        FNLog.Debug("No JVM found");
                    }


                }
                catch (Exception e)
                {
                    Debug.WriteLine("ERROR running JVM:");
                    Debug.WriteLine(e.ToString());
                }
            }
        }


        public static JREType GetBestAvailableJRE { get; private set; } = JREType.None;
    }
}
