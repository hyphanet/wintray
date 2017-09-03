using System;
using System.Windows.Forms;

namespace FreenetTray
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            // migrate settings from older config files for previous assembly versions
            FreenetTray.Properties.Settings.Default.Upgrade();

            FNLog.Initialize();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new CommandsMenu());
        }
    }
}
