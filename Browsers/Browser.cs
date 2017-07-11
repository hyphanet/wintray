using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace FreenetTray.Browsers {
    interface IBrowser {
        /*
         * Return true if the URI was opened in privacy mode.
         * Return false otherwise.
         */
        bool Open(Uri target);

        /*
         * Return true if pages can be opened in privacy mode.
         * Return false otherwise.
         */
        bool IsAvailable();

        string GetName();
    }

    class Browser: IBrowser {

        protected class TupleList<T1, T2>: List<Tuple<T1, T2>> {
            public void Add(T1 item, T2 item2) {
                Add(new Tuple<T1, T2>(item, item2));
            }
        }

        // whether the browser is actually installed
        protected bool _isInstalled;

        // used to determine whether the installed version is current enough
        protected bool _isUsable;

        // the display name of the browser, e.g. Firefox
        protected string _name;

        // the version of the installed browser
        protected Version _version;

        // the path to the .exe, used to launch the browser
        protected string _path;

        // any arguments needed when launching the browser, for example private browser switches
        protected string _args;

        protected static readonly RegistryView[] RegistryViews = {
            RegistryView.Registry64,
            RegistryView.Registry32,
        };

        public bool IsAvailable() {
            return IsInstalled() && IsUsable();
        }

        public bool IsInstalled() {
            return _isInstalled;
        }

        public bool IsUsable() {
            return _isUsable;
        }

        public string GetName() {
            return _name;
        }

        public string GetLaunchPath() {
            return _path;
        }

        public bool Open(Uri target) {
            if (!IsAvailable()) {
                return false;
            }

            Process.Start(_path, _args + target);
            return true;
        }

    }
}
