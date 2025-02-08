using System;
using System.Management;
using System.Windows.Forms;

class TrackAndAlert{
    static void Main(){
        ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT BatteryStatus, EstimatedChargeRemaining FROM Win32_Battery");
        foreach (ManagementObject queryObj in searcher.Get())
        {
            if((ushort)queryObj["EstimatedChargeRemaining"]<31)MessageBox.Show("Your battery is below 32!", "Please charge!");
            else if((ushort)queryObj["EstimatedChargeRemaining"]>97)MessageBox.Show("Your battery is above 97!", "STOP!");
        }
    }
}