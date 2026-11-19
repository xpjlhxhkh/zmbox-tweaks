using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

class StartBox
{
    [STAThread]
    static void Main(string[] args)
    {
        string dir = AppDomain.CurrentDomain.BaseDirectory;
        string root = Path.GetFullPath(Path.Combine(dir, ".."));
        string coin = Path.Combine(dir, "cornercoin.exe");
        string fps = Path.Combine(dir, "fpslabel.exe");
        string app = null;
        try
        {
            foreach (var f in Directory.GetFiles(root, "*.exe"))
            {
                string name = Path.GetFileName(f);
                if (string.Equals(name, "cornercoin.exe", StringComparison.OrdinalIgnoreCase)) continue;
                if (string.Equals(name, "fpslabel.exe", StringComparison.OrdinalIgnoreCase)) continue;
                if (string.Equals(name, "StartBox.exe", StringComparison.OrdinalIgnoreCase)) continue;
                app = f;
                break;
            }
        }
        catch { }

        if (app == null)
        {
            MessageBox.Show("未找到造梦盒子主程序。", "造梦盒子", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            bool coinRunning = false;
            foreach (var p in Process.GetProcessesByName("cornercoin")) { coinRunning = true; break; }
            if (!coinRunning && File.Exists(coin))
                Process.Start(new ProcessStartInfo(coin) { UseShellExecute = true, WorkingDirectory = dir });
        }
        catch { }

        try
        {
            bool fpsRunning = false;
            foreach (var p in Process.GetProcessesByName("fpslabel")) { fpsRunning = true; break; }
            if (!fpsRunning && File.Exists(fps))
                Process.Start(new ProcessStartInfo(fps) { UseShellExecute = true, WorkingDirectory = dir });
        }
        catch { }

        try
        {
            var si = new ProcessStartInfo(app) { UseShellExecute = true, WorkingDirectory = root };
            if (args != null && args.Length > 0) si.Arguments = string.Join(" ", args);
            Process.Start(si);
        }
        catch (Exception ex)
        {
            MessageBox.Show("启动失败：" + ex.Message, "造梦盒子", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
