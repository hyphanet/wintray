using System;
using System.IO;
using System.Linq;

namespace FreenetTray.Browsers
{
    class Chrome: Browser
    {
        /*
         * Google Chrome does not maintain a registry entry with a path to its executable.
         * Usual Google Chrome installation locations:
         * See https://code.google.com/p/selenium/source/browse/java/client/src/org/openqa/selenium/browserlaunchers/locators/GoogleChromeLocator.java#63
         */
        private static readonly string[] Locations = {
                                                 @"%LOCALAPPDATA%\Google\Chrome\Application",
                                                 @"%PROGRAMFILES%\Google\Chrome\Application",
                                                 @"%PROGRAMFILES(X86)%\Google\Chrome\Application",
                                               };

        public Chrome()
        {
            _path = Locations
                .Select(location => Environment.ExpandEnvironmentVariables(location) + @"\chrome.exe")
                .Where(File.Exists)
                .FirstOrDefault();

            _isInstalled = _path != null;

            // we don't know but it's not critical
            _version = new System.Version(0, 0);

            _isUsable = true;

            // See http://peter.sh/experiments/chromium-command-line-switches/
            _args = "--incognito ";

            _name = "Chrome";
        }
    }
}
