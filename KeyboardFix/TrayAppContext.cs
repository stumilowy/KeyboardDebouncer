using KeyboardFix;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;



internal class TrayAppContext : ApplicationContext
{
    private NotifyIcon trayIcon;
    private LogsWindow logsWindow;

    public TrayAppContext(LogsWindow logsWindow)
    {
        this.logsWindow = logsWindow;   
        trayIcon = new NotifyIcon()
        {
            Icon = new Icon("assets/logo.ico"),
            Text = "Keyboard Hook Running",
            Visible = true
        };

        // Create a context menu
        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add("Open Logs Panle", null, ShowLogs);
        contextMenu.Items.Add("Exit", null, Exit);

        trayIcon.ContextMenuStrip = contextMenu;
    }
    private void ShowLogs(object sender, EventArgs e)
    {
        logsWindow.Show();
        logsWindow.BringToFront();
    }
    private void Exit(object sender, EventArgs e)
    {
        trayIcon.Visible = false;
        Application.Exit();
    }
}
