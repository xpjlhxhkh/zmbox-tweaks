using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

class CoinForm : Form
{
    public CoinForm()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
    }
}

class CornerCoin
{
    [DllImport("user32.dll")] static extern bool SetParent(IntPtr child, IntPtr parent);
    [DllImport("user32.dll")] static extern int SetWindowLong(IntPtr h, int idx, int val);
    [DllImport("user32.dll")] static extern int GetWindowLong(IntPtr h, int idx);
    [DllImport("user32.dll")] static extern bool SetWindowPos(IntPtr h, IntPtr after, int x, int y, int cx, int cy, uint flags);
    [DllImport("user32.dll")] static extern bool GetClientRect(IntPtr h, out RECT r);
    [DllImport("user32.dll")] static extern bool IsWindow(IntPtr h);

    public struct RECT { public int Left, Top, Right, Bottom; }

    const int GWL_EXSTYLE = -20;
    const int WS_EX_TRANSPARENT = 0x20;
    const int WS_EX_NOACTIVATE = 0x08000000;
    const int DrawSize = 16;
    const int WindowSize = DrawSize + 6;

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

    static Image[] LoadFrames()
    {
        string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "coin16");
        if (!Directory.Exists(dir)) return new Image[0];
        var files = Directory.GetFiles(dir, "frame_*.png");
        Array.Sort(files, StringComparer.OrdinalIgnoreCase);
        var list = new Image[files.Length];
        for (int i = 0; i < files.Length; i++)
        {
            using (var img = Image.FromFile(files[i]))
                list[i] = new Bitmap(img);
        }
        return list;
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

        var frames = LoadFrames();
        if (frames.Length == 0) return;

        var form = new CoinForm();
        form.FormBorderStyle = FormBorderStyle.None;
        form.StartPosition = FormStartPosition.Manual;
        form.Size = new Size(WindowSize, WindowSize);
        form.ShowInTaskbar = false;
        form.TopMost = false;
        form.BackColor = KeyColor;
        form.TransparencyKey = KeyColor;

        var t0 = DateTime.Now;
        form.Paint += delegate(object s, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.Clear(KeyColor);
            double ms = (DateTime.Now - t0).TotalMilliseconds;
            int idx = (int)(Math.Floor(ms / 90.0)) % frames.Length;
            int bob = (int)Math.Round(1.0 * Math.Sin(ms / 160.0));
            int x = (WindowSize - DrawSize) / 2;
            int y = (WindowSize - DrawSize) / 2 + bob;
            g.DrawImage(frames[idx], x, y, DrawSize, DrawSize);
        };

        form.Show();
        IntPtr h = form.Handle;
        SetParent(h, parent);
        int ex = GetWindowLong(h, GWL_EXSTYLE);
        SetWindowLong(h, GWL_EXSTYLE, ex | WS_EX_TRANSPARENT | WS_EX_NOACTIVATE);

        int lastW = -1, lastH = -1;
        Action place = delegate()
        {
            if (!IsWindow(parent)) return;
            RECT cr;
            GetClientRect(parent, out cr);
            lastW = cr.Right; lastH = cr.Bottom;
            int px = Math.Max(0, cr.Right - WindowSize - 1);
            int py = Math.Max(0, cr.Bottom - WindowSize - 1);
            SetWindowPos(h, IntPtr.Zero, px, py, WindowSize, WindowSize, 0x0014);
        };
        place();

        var timer = new System.Windows.Forms.Timer();
        timer.Interval = 16;
        timer.Tick += delegate(object s, EventArgs e)
        {
            if (!IsWindow(parent))
            {
                IntPtr np = FindBoxWindow();
                if (np != IntPtr.Zero)
                {
                    parent = np;
                    SetParent(h, parent);
                    int ex2 = GetWindowLong(h, GWL_EXSTYLE);
                    SetWindowLong(h, GWL_EXSTYLE, ex2 | WS_EX_TRANSPARENT | WS_EX_NOACTIVATE);
                    place();
                }
                else
                {
                    form.Close();
                    return;
                }
            }
            else
            {
                RECT cr;
                GetClientRect(parent, out cr);
                if (cr.Right != lastW || cr.Bottom != lastH) place();
            }
            form.Invalidate();
        };
        timer.Start();

        Application.Run(form);
    }
}
