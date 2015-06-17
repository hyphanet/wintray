# Freenet Tray Application

<a href="https://scan.coverity.com/projects/5458">
  <img alt="Coverity Scan Build Status"
       src="https://scan.coverity.com/projects/5458/badge.svg"/>
</a>

This is a replacement for the AutoHotKey tray application. It aims to have more robust localization support, not be false-positived by overzelous antivirus hueristics that hate scripting languages, and have a few more features: setting which browser to open and hiding the tray icon.

## Screenshots

![Tray icon](Screenshots/icon.png)

![Running](Screenshots/running_menu.png)

![Stopped](Screenshots/stopped_menu.png)

![Preferences window](Screenshots/preferences.png)

It uses .NET 3.5 because it is [distributed with 7](http://msdn.microsoft.com/en-us/library/bb822049%28v=vs.110%29.aspx), which is [still supported](http://windows.microsoft.com/en-us/windows/lifecycle) and has a significant market share unlike Vista. 3.0 doesn't include some useful things. Existing installs can continue to use the old application.

TODO:

* Can the ntservice parts of wrapper.conf be removed?
* Installer should set language in freenet.ini to the one it was told to use.
* Allow one instance open at a time. If another instance is given a command line command pass it to the existing instance.

Menu items | command line options:

Command line options are executed from left to right, so `-othercommand -hide` is useful to perform an action and exit the tray application.

## First run | -welcome

This shows a balloon tip about using the tray and opens Freenet like -open.

## Open Freenet | -open

Open a browser in privacy mode to Freenet, if possible. The default preference is [same as AHK app], but a specific browser or command can be set as well. If Freenet is not running it is started.

TODO: Should this be "Open Freenet dashboard" instead?

## Start Freenet | -start

Start Freenet.

If this is done as part of startup it fails with a wrapper pipe error. Is this due to when startup runs? Or did it just take too long for some reason? Can't reproduce.
	Also on... shutdown? FATAL  | wrapper  | 2014/05/18 16:11:12 | ERROR: Could not write pid file freenet.pid: The process cannot access the file because it is being used by another process. (0x20)

## Stop Freenet | -stop

Stop Freenet.

## View logs | -logs

Open `wrapper.log` in notepad.

## Preferences | -preferences

Set the browser to use, and whether to start the icon or start Freenet on startup.

## Hide icon | -hide

Hide the icon by closing the tray application. This option is not shown when Freenet is running.

## Exit | -exit

Stop Freenet if it is running and close the tray application.
