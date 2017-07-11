using FreenetTray.Properties;
using NLog;
using NLog.Config;
using NLog.Targets;
using System;


namespace FreenetTray {
    class FNLog {
        /* This class allows logging to reach the VS console during development,
         * regardless of how NLog is configured. 
         * 
         * It appears that NLog is supposed to allow logging to the VS console, 
         * but it frequently does not work correctly. Note that this also
         * prevents NLog from knowing which class originally printed a log
         * statement, but for our purposes that shouldn't affect anything as
         * we log full exceptions when necessary anyway.
         * 
         */
        public const string LogTargetName = "logFile";

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static void Initialize() {

            var config = new LoggingConfiguration();
            var target = new FileTarget {FileName = "${basedir}/FreenetTray.log"};
            config.AddTarget(LogTargetName, target);
            var rule = new LoggingRule("*", LogLevel.FromString(Settings.Default.LogLevel), target);
            config.LoggingRules.Add(rule);
            LogManager.Configuration = config;
        }

        public static void Debug(string format, params object[] args) {
            string f = "[Debug] " + format;
            string line = string.Format(f, args);
            System.Diagnostics.Debug.WriteLine(line);
            Log.Info(format, args);
        }

        public static void Info(string format, params object[] args) {
            string f = "[Info] " + format;
            string line = string.Format(f, args);
            System.Diagnostics.Debug.WriteLine(line);
            Log.Info(format, args);
        }

        public static void Warn(string format, params object[] args) {
            string f = "[Warn] " + format;
            string line = string.Format(f, args);
            System.Diagnostics.Debug.WriteLine(line);
            Log.Warn(format, args);
        }

        public static void Error(string format, params object[] args) {
            string f = "[Error] " + format;
            string line = string.Format(f, args);
            System.Diagnostics.Debug.WriteLine(line);
            Log.Error(format, args);
        }

        public static void ErrorException(Exception ex, string format, params object[] args) {
            string f = "[Error] " + format;
            string line = string.Format(f, args);
            System.Diagnostics.Debug.WriteLine(line);
            Log.Log(LogLevel.Error, ex, format, args);
        }
    }
}
