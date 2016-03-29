using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using FreenetTray.Browsers;
using FreenetTray.Properties;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace FreenetTray
{
    public partial class CommandsMenu : Form
    {
        // Milliseconds between connection attempts while waiting for startup.
        private const int SocketPollInterval = 100;
        /*
         * Milliseconds to wait for startup before notifying the user that Freenet is starting.
         *
         * See http://www.nngroup.com/articles/response-times-3-important-limits/
         */
        private const int SlowOpenThreshold = 3000;
        // Milliseconds to show notification balloons.
        private const int SlowOpenTimeout = 5000;
        private const int WelcomeTimeout = 10000;

        public const string LogTargetName = "logFile";

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private NodeController _node;

        public CommandsMenu()
        {
            InitializeComponent();

            var config = new LoggingConfiguration();
            var target = new FileTarget {FileName = "${basedir}/FreenetTray.log"};
            config.AddTarget(LogTargetName, target);
            var rule = new LoggingRule("*", LogLevel.FromString(Settings.Default.LogLevel), target);
            config.LoggingRules.Add(rule);
            LogManager.Configuration = config;

            FormClosed += (sender, e) => trayIcon.Visible = false;
            Shown += (sender, e) => Hide();

            // TODO: Read registry to check if the old tray runs at startup and change settings accordingly. (or offer to?)
            /*
             * TODO: Will the settings be saved always or only if non-default? If always saved this
             * introduces a fingerprint of Freenet on the system even when it doesn't do anything on
             * startup.
             */
        }

        private void CommandsMenu_Load(object sender, EventArgs e)
        {
            trayIcon.Icon = Resources.Offline;
            BeginInvoke(new Action(FindNode));
        }

        private void NodeStarted(object sender, EventArgs e)
        {
            RefreshMenu(true);
        }

        private void NodeStopped(object sender, EventArgs e)
        {
            RefreshMenu(false);
        }

        private void NodeCrashed(NodeController.CrashType crashType)
        {
            RefreshMenu(false);
            BeginInvoke(new Action(new CrashDialog(crashType, Start, ViewLogs).Show));
        }

        private void FindNode()
        {
            while (true)
            {
                try
                {
                    _node = new NodeController();
                    break;
                }
                catch (FileNotFoundException e)
                {
                    Log.Error(e);
                }
                catch (DirectoryNotFoundException e)
                {
                    Log.Error(e);
                }
                catch (NodeController.MissingConfigValueException e)
                {
                    // If the configuration files exist but are missing required
                    // values it is sufficiently surprising to warrant an error
                    // dialog.
                    Log.Error(strings.MalformedConfig, e.Filename, e.Value);
                    MessageBox.Show(String.Format(strings.MalformedConfig, e.Filename, e.Value), "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                // TODO: Explain what happened to prompt a custom location?
                try
                {
                    PreferencesWindow.PromptCustomLocation(this);
                }
                catch (OperationCanceledException)
                {
                    /* User exited the file browser. */
                    Application.Exit();
                    return;
                }
            }

            _node.OnStarted += NodeStarted;
            _node.OnStopped += NodeStopped;
            _node.OnCrashed += NodeCrashed;

            foreach (var menuItem in new[]
            {
                openFreenetMenuItem,
                startFreenetMenuItem,
                stopFreenetMenuItem,
                downloadsMenuItem,
                viewLogsMenuItem,
                preferencesMenuItem,
                hideIconMenuItem,
            })
            {
                menuItem.Enabled = true;
            }

            // Set menu up for whether there is an existing node.
            RefreshMenu(_node.IsRunning());

            ReadCommandLine();
        }

        private void openFreenetMenuItem_Click(object sender = null, EventArgs e = null)
        {
            Start();

            var pollFproxy = new Thread(() =>
            {
                var fproxyListening = false;
                var showSlowOpen = Settings.Default.ShowSlowOpenTip;
                var openArgs = e as OpenArgs;
                if (openArgs != null)
                {
                    showSlowOpen = openArgs.ShowSlow;
                }

                /*
                 * TODO: Programatic way to get loopback address? This would not support IPv6.
                 * Use FProxy bind interface?
                 */
                var loopback = new IPAddress(new byte[] { 127, 0, 0, 1 });

                var timer = new Stopwatch();

                using (var sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    timer.Start();
                    while (_node.IsRunning())
                    {
                        try
                        {
                            sock.Connect(loopback, _node.FProxyPort);
                            fproxyListening = true;
                            break;
                        }
                        catch (SocketException ex)
                        {
                            Log.Debug("Connecting got error: {0}",
                                Enum.GetName(typeof (SocketError), ex.SocketErrorCode));
                            Thread.Sleep(SocketPollInterval);
                        }

                        // Show a startup notification if it's taking a while.
                        if (showSlowOpen && timer.ElapsedMilliseconds > SlowOpenThreshold)
                        {
                            BeginInvoke(new Action(() =>
                            {
                                trayIcon.BalloonTipText = strings.FreenetStarting;
                                trayIcon.ShowBalloonTip(SlowOpenTimeout);
                            }));
                            showSlowOpen = false;
                        }
                    }
                    timer.Stop();
                }

                if (fproxyListening)
                {
                    Log.Debug("FProxy listening after {0}", timer.Elapsed);
                    BrowserUtil.Open(new Uri(String.Format("http://localhost:{0:d}", _node.FProxyPort)), true);
                }
            });
            pollFproxy.Start();
        }

        private void startFreenetMenuItem_Click(object sender = null, EventArgs e = null)
        {
            Start();
        }

        private void stopFreenetMenuItem_Click(object sender = null, EventArgs e = null)
        {
            BeginInvoke(new Action(_node.Stop));
        }

        private void viewLogsMenuItem_Click(object sender = null, EventArgs e = null)
        {
            ViewLogs();
        }

        private void preferencesMenuItem_Click(object sender = null, EventArgs e = null)
        {
            new PreferencesWindow(BrowserUtil.GetAvailableBrowsers()).Show();
        }

        private void hideIconMenuItem_Click(object sender = null, EventArgs e = null)
        {
            // The node will continue running.
            Application.Exit();
        }

        /*
         * Stop the node if it is running, wait for it to exit, then quit the tray.
         */
        private void exitMenuItem_Click(object sender = null, EventArgs e = null)
        {
            if (_node != null)
            {
                /*
                 * Register an event handler for the exit, but if the node is already
                 * stopped exit immediately because the event would never fire. This
                 * order of checking avoids missing a node exit: if the check came
                 * before the handler was registered, the node could exit between
                 * the check and the handler registration and the tray would hang.
                 */
                _node.OnStopped += (o, args) => Application.Exit();
                if (!_node.IsRunning())
                {
                    Application.Exit();
                }
                else
                {
                    _node.Stop();
                }
            }
            else
            {
                Application.Exit();
            }
        }

        private void trayIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                openFreenetMenuItem_Click(sender, e);
            }
        }

        private void Start()
        {
            BeginInvoke(new Action(_node.Start));
        }

        private void ViewLogs()
        {
            Process.Start("notepad.exe", _node.WrapperLogFilename);
        }

        private void RefreshMenu(bool running)
        {
            BeginInvoke(new Action(() =>
            {
                startFreenetMenuItem.Enabled = !running;
                stopFreenetMenuItem.Enabled = running;
                hideIconMenuItem.Visible = running;
                trayIcon.Icon = running ? Resources.Online : Resources.Offline;
            }));
        }

        private void ReadCommandLine()
        {
            /*
             * TODO: Difficulties with this implementation are ignoring the application name if it is
             * present and supporting arguments with parameters.
             */
            foreach (var arg in Environment.GetCommandLineArgs())
            {
                switch (arg)
                {
                    case "-open":
                        openFreenetMenuItem_Click();
                        break;
                    case "-start":
                        startFreenetMenuItem_Click();
                        break;
                    case "-stop":
                        stopFreenetMenuItem_Click();
                        break;
                    case "-downloads":
                        downloadsMenuItem_Click();
                        break;
                    case "-logs":
                        viewLogsMenuItem_Click();
                        break;
                    case "-preferences":
                        preferencesMenuItem_Click();
                        break;
                    case "-hide":
                        hideIconMenuItem_Click();
                        break;
                    case "-exit":
                        exitMenuItem_Click();
                        break;
                    case "-welcome":
                        trayIcon.BalloonTipIcon = ToolTipIcon.Info;
                        trayIcon.BalloonTipTitle = strings.FreenetStarting;
                        trayIcon.BalloonTipText = strings.WelcomeTip;
                        trayIcon.ShowBalloonTip(WelcomeTimeout);
                        openFreenetMenuItem_Click(null, new OpenArgs { ShowSlow = false });
                        break;
                }
            }
        }

        private class OpenArgs : EventArgs
        {
            public bool ShowSlow { get; set; }
        }

        private void downloadsMenuItem_Click(object sender = null, EventArgs e = null)
        {
            Process.Start("explorer.exe", _node.DownloadsDir);
        }
    }
}
