using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace KeyboardFix
{
    public partial class LogsWindow : Form
    {
        private RichTextBox richTextBox1;

        private bool dragging = false;
        private Point dragCursorPoint;
        private Point dragStartPoint;

        private const int MAX_LOG_LINES = 1000;

        public LogsWindow()
        {
            InitializeComponent();
            MainForm_Load();
            this.Text = "Logs";
            this.BackColor = Color.FromArgb(45, 45, 48);
            this.ForeColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Icon = new Icon("assets/logo.ico");

            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.LogsWindow_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.LogsWindow_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.LogsWindow_MouseUp);
        }

        private void LogsWindow_MouseDown(object sender, MouseEventArgs e)
        {
            dragging = true;
            dragCursorPoint = Cursor.Position;
            dragStartPoint = this.Location;
        }

        private void LogsWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                Point dif = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
                this.Location = Point.Add(dragStartPoint, new Size(dif));
            }
        }

        private void LogsWindow_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }

        private void MainForm_Load()
        {
            richTextBox1.ReadOnly = true;
            // Example log data
            AppendLog("Application started...",Color.LightGreen);
            AppendLog("Loading configuration..", Color.LightGreen);
            AppendLog("Ready!", Color.LightGreen);
        }

        public void AppendLog(string message, Color color = default(Color))
        {
            // If the color is not specified, or is the default empty Color, set it to Black
            if (color == default(Color))
            {
                color = Color.White;
            }
            string[] lines = richTextBox1.Lines;
            // Check if the number of lines exceeds the limit
            if (lines.Length > MAX_LOG_LINES)
            {
                lines = lines.Skip(MAX_LOG_LINES/2).ToArray();
                richTextBox1.Lines = lines;

            }

            // Set the selection color
            this.richTextBox1.SelectionColor = color;

            // Append the text
            this.richTextBox1.AppendText($"{DateTime.Now:HH:mm:ss} - {message}\n");

            // Reset the selection color to black to avoid issues with subsequent text
            this.richTextBox1.SelectionColor = Color.White;

            // Scroll to the end of the text
            richTextBox1.SelectionStart = richTextBox1.TextLength;
            richTextBox1.ScrollToCaret();
        }

        public void UpdateTotalBlocksLabel(int blockedCount)
        {
            totalBlocksLabel.Text = $"Total Blocks: {blockedCount}";
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
            base.OnFormClosing(e);
        }

        private void InitializeComponent()
        {
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.totalBlocksLabel = new System.Windows.Forms.Label();
            this.closeButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // richTextBox1
            // 
            this.richTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.richTextBox1.ForeColor = System.Drawing.Color.White;
            this.richTextBox1.Location = new System.Drawing.Point(12, 12);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(351, 336);
            this.richTextBox1.TabIndex = 0;
            this.richTextBox1.Text = "";
            // 
            // totalBlocksLabel
            // 
            this.totalBlocksLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.totalBlocksLabel.AutoSize = true;
            this.totalBlocksLabel.Font = new System.Drawing.Font(SystemFonts.MenuFont.FontFamily, 16F);
            this.totalBlocksLabel.Location = new System.Drawing.Point(12, 358);
            this.totalBlocksLabel.Name = "totalBlocksLabel";
            this.totalBlocksLabel.Size = new System.Drawing.Size(154, 50);
            this.totalBlocksLabel.TabIndex = 1;
            this.totalBlocksLabel.Text = "Total Blocks: 0";
            // 
            // closeButton
            // 
            this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.closeButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.closeButton.ForeColor = System.Drawing.Color.White;
            this.closeButton.Location = new System.Drawing.Point(200, 361);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(150, 25);
            this.closeButton.TabIndex = 2;
            this.closeButton.Text = "Close Panel";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // LogsWindow
            // 
            this.ClientSize = new System.Drawing.Size(375, 399);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.totalBlocksLabel);
            this.Controls.Add(this.closeButton);
            this.Name = "Logs Panel";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private System.Windows.Forms.Label totalBlocksLabel;
        private System.Windows.Forms.Button closeButton;
    }
}
