using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;



internal class TrayAppContext : ApplicationContext
{
    private NotifyIcon trayIcon;

    public TrayAppContext()
    {
        trayIcon = new NotifyIcon()
        {
            Icon = SystemIcons.Information, // You can use a custom .ico file
            Text = "Keyboard Hook Running",
            Visible = true
        };

        // Create a context menu
        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add("Show Console", null, ShowConsole);
        contextMenu.Items.Add("Exit", null, Exit);

        trayIcon.ContextMenuStrip = contextMenu;
    }

    private void ShowConsole(object sender, EventArgs e)
    {
        var handle = GetConsoleWindow();
        ShowWindow(handle, SW_SHOW);
    }

    private void Exit(object sender, EventArgs e)
    {
        trayIcon.Visible = false;
        Application.Exit();
    }

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    private const int SW_SHOW = 5;
}
