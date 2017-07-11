using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace FreenetTray
{
    public class NodeController
    {
        public enum CrashType
        {
            WrapperFileNotFound,
            PathTooLong,
            WrapperCrashed,
        }

        public class MissingConfigValueException : Exception
        {
            public readonly string Filename;
            public readonly string Value;

            public MissingConfigValueException(string filename, string value)
            {
                Filename = filename;
                Value = value;
            }
        }

        // System Error Codes
        // See http://msdn.microsoft.com/en-us/library/windows/desktop/ms681382%28v=vs.85%29.aspx
        // TODO: Is there a C# assembly with these?
        private const int ERROR_FILE_NOT_FOUND = 0x2;
        private const int ERROR_INSUFFICIENT_BUFFER = 0x7A;
        private const int ERROR_ACCESS_DENIED = 0x5;

        public delegate void CrashHandler(CrashType type);

        public CrashHandler OnCrashed;
        public EventHandler OnStarted;
        public EventHandler OnStopped;

        private Process _wrapper;
        private readonly ProcessStartInfo _wrapperInfo = new ProcessStartInfo();

        private readonly NodeConfig _config;

        public string WrapperLogFilename { get { return _config.WrapperLogFilename; } }
        public int FProxyPort { get { return _config.FProxyPort; } }
        public string DownloadsDir { get { return _config.DownloadsDir; } }

        private const string FreenetIniFilename = @"freenet.ini";
        private const string WrapperConfFilename = "wrapper.conf";

        public static string WrapperFilename {
            get {
                switch (MachineConfig.GetBestAvailableJRE) {
                    case JREType.JRE64Bit:
                        return @"wrapper\freenetwrapper-64.exe";
                    case JREType.JRE32Bit:
                        return @"wrapper\freenetwrapper.exe";
                    case JREType.None:
                        // there is no JRE installed at all
                        throw new MissingJRE();
                    default:
                        throw new MissingJRE();
                }
            }
        }

        // TODO: Where to document? Throws FileNotFound, DirectoryNotFound, MissingJRE
        public NodeController()
        {
            if (Properties.Settings.Default.CustomLocation.Length != 0)
            {
                _config = new NodeConfig(Properties.Settings.Default.CustomLocation);
            }
            else
            {
                Exception configException = null;
                foreach (var path in new[]
                {
                    Directory.GetCurrentDirectory(),
                    Environment.ExpandEnvironmentVariables(@"%LocalAppData%\Freenet"),
                })
                {
                    // TODO: If the wrapper has problems with arguments with non-ASCII characters should
                    // this the wrapper invocation change the working directory? Won't work in the general
                    // case because the pidfile location could contain non-ASCII characters, but it
                    // works for the default configuration.
                    // http://sourceforge.net/p/wrapper/bugs/290/
                    try
                    {
                        _config = new NodeConfig(path);
                        configException = null;
                        break;
                    }
                    catch (Exception e)
                    {
                        configException = e;
                    }
                }

                if (configException != null)
                {
                    FNLog.Error("Failed to detect Freenet installation.", configException);
                    throw configException;
                }
            }

            // Search for an existing wrapper process.
            try
            {
                using (var reader = new StreamReader(_config.PidFilename))
                {
                    var line = reader.ReadLine();

                    if (line != null)
                    {
                        var pid = int.Parse(line);
                        _wrapper = Process.GetProcessById(pid);
                        _wrapper.EnableRaisingEvents = true;
                        _wrapper.Exited += Wrapper_Exited;
                    }
                }
            }
            catch (ArgumentException)
            {
                FNLog.Debug("No process has the PID in the PID file.");
                // The wrapper can refuse to start if there is a stale PID file - "strict".
                try
                {
                    File.Delete(_config.PidFilename);
                }
                catch (IOException)
                {
                    // TODO: Be louder about this? Or will the wrapper fail to start and exit nonzero?
                    FNLog.Debug("Stale PID file is still held.");
                }
            }
            catch (FormatException)
            {
                FNLog.Debug("PID file does not contain an integer.");
            }
            catch (OverflowException)
            {
                FNLog.Debug("PID file does not contain an integer.");
            }
            catch (FileNotFoundException)
            {
                FNLog.Debug("PID file not found.");
            }

            /*
             * Hide the wrapper window when launching it. This prevents (or at least heavily complicates)
             * stopping it with Process.CloseMainWindow() or by sending ctrl + C.
             */
            _wrapperInfo.FileName = Path.Combine(_config.RelativeTo, WrapperFilename);
            // TODO: Is it worthwhile to omit the pidfile here when it's in the config file?
            _wrapperInfo.Arguments = "-c " + WrapperConfFilename + " wrapper.pidfile=" + _config.PidFilename;
            _wrapperInfo.UseShellExecute = false;
            _wrapperInfo.CreateNoWindow = true;
        }

        /*
         * TODO: What are the function documentation comments supposed to be formatted like?
         * Start the node if it is not already started.
         * 
         * Throws FileNotFoundException
         */
        public void Start()
        {
            if (IsRunning())
            {
                return;
            }

            try
            {
                // TODO: Under what circumstances will Process.Start() return null?
                _wrapper = Process.Start(_wrapperInfo);
                _wrapper.EnableRaisingEvents = true;
                _wrapper.Exited += Wrapper_Exited;
            }
            catch (Win32Exception ex)
            {
                // http://msdn.microsoft.com/en-us/library/0w4h05yb%28v=vs.110%29.aspx
                switch (ex.NativeErrorCode)
                {
                    case ERROR_FILE_NOT_FOUND:
                        FNLog.Error("Cannot start Freenet: wrapper executable not found.");
                        OnCrashed(CrashType.WrapperFileNotFound);
                        return;
                    case ERROR_INSUFFICIENT_BUFFER:
                    case ERROR_ACCESS_DENIED:
                        FNLog.Error("Cannot start Freenet: the file path is too long.");
                        OnCrashed(CrashType.PathTooLong);
                        return;
                    default:
                        FNLog.ErrorException(ex, "Cannot start Freenet: Process.Start() gave an error code it is not documented as giving.");
                        throw;
                }
            }

            OnStarted(this, null);
        }

        public void Stop()
        {
            if (IsRunning())
            {
                // TODO: Tolerate missing file.
                File.Delete(_config.AnchorFilename);
            }
        }

        public Boolean IsRunning()
        {
            return _wrapper != null && !_wrapper.HasExited;
        }

        private void Wrapper_Exited(object sender, EventArgs e)
        {
            // TODO: Is exit code enough to distinguish between stopping and crashing?
            if (_wrapper.ExitCode == 0)
            {
                FNLog.Debug("Wrapper exited.");
                OnStopped(sender, e);
            }
            else
            {
                FNLog.Error("Wrapper crashed. Exit code: {0}", _wrapper.ExitCode);
                OnCrashed(CrashType.WrapperCrashed);
            }
        }

        private class NodeConfig
        {
            public readonly string AnchorFilename;
            public readonly string PidFilename;
            public readonly string WrapperLogFilename;
            public readonly int FProxyPort;
            public readonly string RelativeTo;
            public readonly string DownloadsDir;

            public NodeConfig(string relativeTo)
            {
                RelativeTo = relativeTo;
                /*
                 * Read wrapper config: wrapper log location, PID file location, anchor location.
                 * The PID file location is specified on the command line, so if none is read
                 * it will use a default. It's not in the default wrapper.conf and is defined on
                 * the command line in run.sh.
                 */
                PidFilename = "freenet.pid";
                // wrapper.conf is relative to the wrapper's location.
                var wrapperDir = Directory.GetParent(Path.Combine(relativeTo, WrapperFilename));
                foreach (var line in File.ReadAllLines(wrapperDir.FullName + '\\' + WrapperConfFilename))
                {
                    // TODO: Map between constants and variables to reduce repetition?
                    if (Defines(line, "wrapper.logfile"))
                    {
                        WrapperLogFilename = Path.Combine(relativeTo, Value(line));
                    }
                    else if (Defines(line, "wrapper.pidfile"))
                    {
                        PidFilename = Path.Combine(relativeTo, Value(line));
                    }
                    else if (Defines(line, "wrapper.anchorfile"))
                    {
                        AnchorFilename = Path.Combine(relativeTo, Value(line));
                    }
                }

                // TODO: A mapping between config location and variable would reduce verbosity here too.
                if (WrapperLogFilename == null)
                {
                    throw new MissingConfigValueException(WrapperConfFilename, "wrapper.logfile");
                }

                if (AnchorFilename == null)
                {
                    throw new MissingConfigValueException(WrapperConfFilename, "wrapper.anchorfile");
                }

                // Read Freenet config: FProxy port TODO: Use ini-parser instead
                // TODO: Does this need to wait until the node is running for the first run?
                var freenetIniLines = File.ReadAllLines(Path.Combine(relativeTo, FreenetIniFilename));

                var port = RequireValue(freenetIniLines, "fproxy.port");
                var isValid = int.TryParse(port, out FProxyPort);
                if (!isValid)
                {
                    FNLog.Error("fproxy.port is not an integer.");
                    throw new MissingConfigValueException(FreenetIniFilename, "fproxy.port");
                }

                DownloadsDir = Path.Combine(RelativeTo,
                                            RequireValue(freenetIniLines, "node.downloadsDir"));
            }

            private static bool Defines(string line, string key)
            {
                // TODO: Does this need to tolerate whitespace between the key and the =? Find an INI library somewhere maybe?
                return line.StartsWith(key + "=");
            }

            private static string Value(string line)
            {
                return line.Split(new[] { '=' }, 2)[1];
            }

            private static string RequireValue(IEnumerable<string> lines, string key)
            {
                try
                {
                    return Value(lines.First(line => Defines(line, key)));
                }
                catch (InvalidOperationException)
                {
                    throw new MissingConfigValueException(FreenetIniFilename, key);
                }
            }
        }
    }
}
