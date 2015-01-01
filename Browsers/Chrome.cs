﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace FreenetTray.Browsers
{
    class Chrome : Browser
    {
        /*
         * Google Chrome does not maintain a registry entry with a path to its exutable.
         * Usual Google Chrome installation locations:
         * See https://code.google.com/p/selenium/source/browse/java/client/src/org/openqa/selenium/browserlaunchers/locators/GoogleChromeLocator.java#63
         */
        private readonly string[] Locations = {
                                                @"%LOCALAPPDATA%\Google\Chrome\Application",
                                                @"%PROGRAMFILES%\Google\Chrome\Application",
                                                @"%PROGRAMFILES(X86)%\Google\Chrome\Application",
                                              };

        private readonly string Path;
        private readonly bool IsInstalled;

        public Chrome()
        {
            Path = Locations
                .Select(location => Environment.ExpandEnvironmentVariables(location) + @"\chrome.exe")
                .Where(File.Exists)
                .FirstOrDefault();

            IsInstalled = Path != null;
        }

        public bool Open(Uri target)
        {
            if (!IsAvailable())
            {
                return false;
            }
                // See http://peter.sh/experiments/chromium-command-line-switches/
                Process.Start(Path, "--incognito " + target);
                return true;
        }

        public bool IsAvailable()
        {
            return IsInstalled;
        }
    }
}
