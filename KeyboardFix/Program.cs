using KeyboardFix;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

using System.Windows.Forms;

namespace KeyboardLogger
{
    class Program
    {
        public static void Main()
        {
            LogsWindow logWindow = new LogsWindow();
            KeyboardDebauncer keyboardDebauncer = new KeyboardDebauncer(logWindow);

            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);

            // The Application.Run call is what keeps the application alive.
            // It will continue to run as long as the ApplicationContext object exists.
            Application.Run(new TrayAppContext(logWindow, keyboardDebauncer));
        }

    }
}