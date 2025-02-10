using System;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Management;
using System.Threading;
using System.Windows.Forms;

class Program
{
    static NotifyIcon trayIcon = null!;
    static System.Windows.Forms.Timer timer = null!;
    static int lastBatteryLevel = -1;

    static void Main()
    {
        trayIcon = new NotifyIcon();
        trayIcon.Visible = true;
        trayIcon.Text = "Battery Tracker";

        ContextMenuStrip menu = new ContextMenuStrip();
        menu.Items.Add("Exit", null, (s, e) => Application.Exit());
        trayIcon.ContextMenuStrip = menu;

        UpdateBatteryStatus();

        timer = new System.Windows.Forms.Timer();
        timer.Interval = 30000; 
        timer.Tick += (s, e) => UpdateBatteryStatus();
        timer.Start();

        Application.Run();
    }

    static void UpdateBatteryStatus()
    {
        try
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT EstimatedChargeRemaining FROM Win32_Battery");
            foreach (ManagementObject queryObj in searcher.Get())
            {
                int batteryLevel = Convert.ToInt32(queryObj["EstimatedChargeRemaining"]);

                if (lastBatteryLevel != -1 && batteryLevel != lastBatteryLevel)
                {
                    AnimateBatteryChange(lastBatteryLevel, batteryLevel);
                }
                else
                {
                    trayIcon.Icon = GenerateBatteryIcon(batteryLevel);
                }

                lastBatteryLevel = batteryLevel;

                if (batteryLevel < 32)
                {
                    trayIcon.BalloonTipTitle = "Battery Low!";
                    trayIcon.BalloonTipText = $"Battery is at {batteryLevel}%. Please charge!";
                    trayIcon.ShowBalloonTip(2000);
                }
                else if (batteryLevel > 97)
                {
                    trayIcon.BalloonTipTitle = "Battery High!";
                    trayIcon.BalloonTipText = $"Battery is at {batteryLevel}%. Unplug the charger!";
                    trayIcon.ShowBalloonTip(2000);
                }
            }
        }
        catch (Exception ex)
        {
            trayIcon.Text = "Battery Info Error!";
            trayIcon.BalloonTipText = "Could not fetch battery status.";
            trayIcon.ShowBalloonTip(1000);
            Console.WriteLine(ex.Message);
        }
    }

    static void AnimateBatteryChange(int oldLevel, int newLevel)
    {
        int steps = 5;
        int diff = newLevel - oldLevel;

        for (int i = 1; i <= steps; i++)
        {
            int intermediate = oldLevel + diff * i / steps;
            trayIcon.Icon = GenerateBatteryIcon(intermediate);
            Application.DoEvents();
            Thread.Sleep(100);
        }
    }

    static Icon GenerateBatteryIcon(int percentage)
    {
        using (Bitmap bitmap = new Bitmap(32, 32))
        using (Graphics g = Graphics.FromImage(bitmap))
        {
            Color backgroundColor = Color.White;
            if (percentage < 32)
                backgroundColor = Color.Red;
            else if (percentage > 97)
                backgroundColor = Color.Green;

            g.Clear(backgroundColor);
            g.TextRenderingHint = TextRenderingHint.AntiAlias;

            using (Font font = new Font("Arial", 23, FontStyle.Bold))
            using (Brush brush = new SolidBrush(Color.Black))
            {
                string text = percentage.ToString();
                SizeF textSize = g.MeasureString(text, font);
                float x = (bitmap.Width - textSize.Width) / 2;
                float y = (bitmap.Height - textSize.Height) / 2;
                g.DrawString(text, font, brush, new PointF(x, y));
            }

            return Icon.FromHandle(bitmap.GetHicon());
        }
    }
}
