using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

class FpsForm : Form
{
    public FpsForm()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
    }
}

class FpsLabel
{
    [DllImport("user32.dll")] static extern bool SetParent(IntPtr child, IntPtr parent);
    [DllImport("user32.dll")] static extern int SetWindowLong(IntPtr h, int idx, int val);
    [DllImport("user32.dll")] static extern int GetWindowLong(IntPtr h, int idx);
    [DllImport("user32.dll")] static extern bool SetWindowPos(IntPtr h, IntPtr after, int x, int y, int cx, int cy, uint flags);
    [DllImport("user32.dll")] static extern bool IsWindow(IntPtr h);

    const int GWL_EXSTYLE = -20;
    const int WS_EX_TRANSPARENT = 0x20;
    const int WS_EX_NOACTIVATE = 0x08000000;

    static readonly string FpsFile = @"C:\Users\18259\AppData\Local\Temp\zmBoxFps.txt";
    static readonly Color KeyColor = Color.FromArgb(255, 0, 255);

    static IntPtr FindBoxWindow()
    {
        foreach (var p in System.Diagnostics.Process.GetProcesses())
        {
            try { if (!string.IsNullOrEmpty(p.MainWindowTitle) && p.MainWindowTitle.Contains("Duskeye")) return p.MainWindowHandle; }
            catch { }
        }
        return IntPtr.Zero;
    }

    [STAThread]
    static void Main()
    {
        IntPtr parent = IntPtr.Zero;
        for (int i = 0; i < 90 && parent == IntPtr.Zero; i++)
        {
            parent = FindBoxWindow();
            if (parent == IntPtr.Zero) Thread.Sleep(2000);
        }
        if (parent == IntPtr.Zero) return;

        int w = 112, h = 26;
        var form = new FpsForm();
        form.FormBorderStyle = FormBorderStyle.None;
        form.StartPosition = FormStartPosition.Manual;
        form.Size = new Size(w, h);
        form.ShowInTaskbar = false;
        form.TopMost = false;
        form.BackColor = KeyColor;
        form.TransparencyKey = KeyColor;

        string text = "FPS";
        form.Paint += delegate(object s, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(KeyColor);
            g.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
            using (var f = new Font("Microsoft YaHei", 11f, FontStyle.Bold))
            using (var shadow = new SolidBrush(Color.Black))
            using (var white = new SolidBrush(Color.White))
            {
                g.DrawString(text, f, shadow, 7, 3);
                g.DrawString(text, f, white, 6, 2);
            }
        };

        form.Show();
        IntPtr hw = form.Handle;
        SetParent(hw, parent);
        int ex = GetWindowLong(hw, GWL_EXSTYLE);
        SetWindowLong(hw, GWL_EXSTYLE, ex | WS_EX_TRANSPARENT | WS_EX_NOACTIVATE);
        SetWindowPos(hw, IntPtr.Zero, 8, 8, w, h, 0x0014);

        var timer = new System.Windows.Forms.Timer();
        timer.Interval = 300;
        timer.Tick += delegate(object s, EventArgs e)
        {
            if (!IsWindow(parent)) { form.Close(); return; }
            try
            {
                if (File.Exists(FpsFile))
                {
                    string t = File.ReadAllText(FpsFile).Trim();
                    if (t.Length > 0) text = "FPS " + t;
                }
            }
            catch { }
            form.Invalidate();
        };
        timer.Start();

        Application.Run(form);
    }
}
