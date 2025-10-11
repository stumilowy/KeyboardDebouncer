using KeyboardFix;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;



internal class TrayAppContext : ApplicationContext
{
    private NotifyIcon trayIcon;
    private LogsWindow logsWindow;
    private KeyboardDebauncer keyboardDebauncer;

    public TrayAppContext(LogsWindow logsWindow, KeyboardDebauncer keyboardDebauncer)
    {
        this.logsWindow = logsWindow;
        this.keyboardDebauncer = keyboardDebauncer;
        trayIcon = new NotifyIcon()
        {
            Icon = new Icon("assets/logo.ico"),
            Text = "Keyboard Hook Running",
            Visible = true
        };

        // Create a context menu
        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add("Show Console", null, ShowLogs);

        // Add the Mills Chooser
        var millsChooser = new ToolStripMenuItem("Mills Chooser");
        for (int i = 10; i <= 200; i += 10)
        {
            var item = new ToolStripMenuItem($"{i} ms");
            if(i== KeyboardDebauncer.KEY_PRESS_THRESHOLD_MS)
            {
                item.Checked = true;
            }   
            item.Click += MillsChooser_Click;
            millsChooser.DropDownItems.Add(item);
        }
        contextMenu.Items.Add(millsChooser);

        contextMenu.Items.Add("Exit", null, Exit);

        trayIcon.ContextMenuStrip = contextMenu;
    }

    private void MillsChooser_Click(object sender, EventArgs e)
    {
        var item = (ToolStripMenuItem)sender;
        var value = int.Parse(item.Text.Replace(" ms", ""));
        keyboardDebauncer.SetKeyPressThreshold(value);

        // Uncheck all items
        foreach (ToolStripMenuItem subItem in ((ToolStripMenuItem)item.OwnerItem).DropDownItems)
        {
            subItem.Checked = false;
        }

        // Check the clicked item
        item.Checked = true;
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
