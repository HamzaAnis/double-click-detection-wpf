using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using Gma.System.MouseKeyHook;

namespace DoubleClickDetection
{
    public partial class Form1 : Form
    {
        double mouseVerticalPosition;
        double mouseHorizontalPosition;
        DateTime clickedTime;
        bool IsSecondClick = false;
        //System.Diagnostics.Stopwatch watch;
        Rectangle rectangle;
        bool isStart = false;
        UInt32 doubleClickTime;
        [DllImport("user32.dll")]
        static extern uint GetDoubleClickTime();
        WMPLib.WindowsMediaPlayer wplayer;
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Unsubscribe();
            Application.Exit();
        }
        public Form1()
        {
            InitializeComponent();
            handlerinit();
        }

        public string ResizeMode { get; private set; }
        private void handlerinit()
        {
            wplayer = new WMPLib.WindowsMediaPlayer();
            this.MaximizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            SubscribeGlobal();
            rectangle = Rectangle.Empty;
            doubleClickTime = GetDoubleClickTime();
        }


        private void optionToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
        protected override void OnMouseDown(MouseEventArgs e)
        {

        }

        private void locationSelectToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            rectangle = SnippingTool.Snip();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (rectangle == Rectangle.Empty)
            {
                MessageBox.Show("Please select the area to detect!!");

            }
            else if (button2.Text.Equals("Start detecting"))
            {
                button2.Text = "Stop detecting";
                isStart = true;
                notifyIcon1.Visible = true;
                //notifyIcon1.ShowBalloonTip(1000, "Double Click Detection", "Detection Started!!!", ToolTipIcon.Info);
            }
            else
            {
                stopDetecting();
            }
        }

        private void showToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Pressed");
            this.showT();
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            stopDetecting();
        }
        void stopDetecting()
        {
            button2.Text = "Start detecting";
            isStart = false;
            notifyIcon1.ShowBalloonTip(1000, "Double Click Detection", "Detection Stopped!!!", ToolTipIcon.Info);

        }
        private void hideToolStripMenuItem_Click(object sender, EventArgs e)
        {
            hideT();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (WindowState != FormWindowState.Normal)
            {
                this.hideT();
            }
        }

        public void hideT()
        {
            this.Hide();
            ShowIcon = false;
            notifyIcon1.Visible = true;
            ShowInTaskbar = false;
            notifyIcon1.ShowBalloonTip(1000, "Double Click Detection", "Application Minimized", ToolTipIcon.Info);
            contextMenuStrip1.ShowItemToolTips = true;
        }

        public void showT()
        {
            this.Show();
            ShowIcon = true;
            ShowInTaskbar = true;
            WindowState = FormWindowState.Normal;
        }
        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.showT();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Unsubscribe();
            Application.Exit();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox1 ab = new AboutBox1();
            ab.Show();
        }

        private void showToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            this.showT();
        }

        private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.hideT();
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }
        private void SubscribeGlobal()
        {
            Unsubscribe();
            Subscribe(Hook.GlobalEvents());
        }

        private IKeyboardMouseEvents m_Events;

        private void Subscribe(IKeyboardMouseEvents events)
        {
            m_Events = events;
            m_Events.MouseDoubleClick += OnMouseDoubleClick;

        }
        private void OnMouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (isStart)
            {
                if (e.Location.X <= rectangle.Location.X + rectangle.Width && e.Location.X > rectangle.Location.X && e.Location.Y > rectangle.Location.Y && e.Location.Y <= rectangle.Location.Y + rectangle.Height)
                {
                    try
                    {
                        {
                            wplayer.URL = @"Sound.mp3";
                            wplayer.controls.play();
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Please place Sound.mp3 in same location as this folder.");
                    }
                }
            }
        }
        private void Unsubscribe()
        {
            if (m_Events == null) return;
            m_Events.MouseDoubleClick -= OnMouseDoubleClick;

            m_Events.Dispose();
            m_Events = null;
        }

    }

    public partial class SnippingTool : Form
    {
        public static Rectangle Snip()
        {
            var rc = Screen.PrimaryScreen.Bounds;
            using (Bitmap bmp = new Bitmap(rc.Width, rc.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb))
            {
                using (Graphics gr = Graphics.FromImage(bmp))
                    gr.CopyFromScreen(0, 0, 0, 0, bmp.Size);
                using (var snipper = new SnippingTool(bmp))
                {
                    if (snipper.ShowDialog() == DialogResult.OK)
                    {
                        return snipper.rcSelect;
                    }
                }
                return Rectangle.Empty;
            }
        }

        public SnippingTool(Image screenShot)
        {
            this.BackgroundImage = screenShot;
            this.ShowInTaskbar = false;
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.DoubleBuffered = true;
        }
        public Image Image { get; set; }

        private Rectangle rcSelect = new Rectangle();
        private Point pntStart;

        protected override void OnMouseDown(MouseEventArgs e)
        {


            // Start the snip on mouse down
            if (e.Button != MouseButtons.Left) return;
            pntStart = e.Location;
            rcSelect = new Rectangle(e.Location, new Size(0, 0));
            this.Invalidate();
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            // Modify the selection on mouse move
            if (e.Button != MouseButtons.Left) return;
            int x1 = Math.Min(e.X, pntStart.X);
            int y1 = Math.Min(e.Y, pntStart.Y);
            int x2 = Math.Max(e.X, pntStart.X);
            int y2 = Math.Max(e.Y, pntStart.Y);
            rcSelect = new Rectangle(x1, y1, x2 - x1, y2 - y1);
            this.Invalidate();
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            // Complete the snip on mouse-up
            if (rcSelect.Width <= 0 || rcSelect.Height <= 0) return;
            Image = new Bitmap(rcSelect.Width, rcSelect.Height);
            using (Graphics gr = Graphics.FromImage(Image))
            {
                gr.DrawImage(this.BackgroundImage, new Rectangle(0, 0, Image.Width, Image.Height),
                    rcSelect, GraphicsUnit.Pixel);
            }
            DialogResult = DialogResult.OK;
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            // Draw the current selection
            using (Brush br = new SolidBrush(Color.FromArgb(120, Color.White)))
            {
                int x1 = rcSelect.X; int x2 = rcSelect.X + rcSelect.Width;
                int y1 = rcSelect.Y; int y2 = rcSelect.Y + rcSelect.Height;
                e.Graphics.FillRectangle(br, new Rectangle(0, 0, x1, this.Height));
                e.Graphics.FillRectangle(br, new Rectangle(x2, 0, this.Width - x2, this.Height));
                e.Graphics.FillRectangle(br, new Rectangle(x1, 0, x2 - x1, y1));
                e.Graphics.FillRectangle(br, new Rectangle(x1, y2, x2 - x1, this.Height - y2));
            }
            using (Pen pen = new Pen(Color.Red, 3))
            {
                e.Graphics.DrawRectangle(pen, rcSelect);
            }
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Allow canceling the snip with the Escape key
            if (keyData == Keys.Escape) this.DialogResult = DialogResult.Cancel;
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }

}
