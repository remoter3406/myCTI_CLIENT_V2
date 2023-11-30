// Use only ONE of these Main methods.
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Runtime.ExceptionServices;
using Microsoft.Win32;
using System.Windows.Forms;
using System.Threading;
using myCTI2;
using System.Net;
using System.Net.Sockets;
using System.Security;

public static class Program
{
    [STAThread]
    public static void Main(string[] cmdArgs)
    {
        string appProcessName = Path.GetFileNameWithoutExtension(Application.ExecutablePath);
        Process[] RunningProcesses = Process.GetProcessesByName(appProcessName);

        RegistryKey MeinKey;
        MeinKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\AB Server", true);
        if (MeinKey == null)
            MeinKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\AB Server");
        if (MeinKey.GetValue("copyStart", "").ToString() == "1")
        {
            MeinKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\AB Server", true);
            MeinKey.SetValue("copyStart", "0");
            MeinKey.Close();
            //Application.EnableVisualStyles();
            Application.Run(new TrayContext());
        }
        else if (RunningProcesses.Length <= 1)
        {
            string netz = Application.ExecutablePath.ToLower();
            int pos = netz.IndexOf(@"\cti\");
            if (pos < 0)
            {
                // TOOL.debugx("Datei liegt nicht im CTI Ordner pos=" + pos.ToString() + " netz=" + netz)
                System.Resources.ResourceManager rm = new System.Resources.ResourceManager(myCTI2.Properties.Resources.token, Assembly.GetExecutingAssembly());
                string token = VB.Mid(rm.BaseName, 16, 40);
                pos = token.IndexOf("#");
                if (pos > 0)
                {
                    string tmp = VB.Mid(token, pos + 1);
                    token = VB.Left(token, pos).Replace(":80", "").Replace(":443", "");
                    // copy exe and set server
                    // TOOL.debugx("Set Server: " + token)
                    MeinKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\AB Server", true);
                    MeinKey.SetValue("server", token);
                    MeinKey.Close();
                    pos = tmp.IndexOf("#");
                    if (pos > 0)
                    {
                        // TOOL.debugx("Set SSL: " + Left(tmp, pos - 1))
                        MeinKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\AB Server", true);
                        MeinKey.SetValue("ssl", VB.Left(tmp, pos));
                        MeinKey.Close();
                    }
                    string myDoc = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Documents";
                    try
                    {
                        Directory.CreateDirectory(myDoc + "\\CTI");
                    }
                    catch
                    {
                    }
                    bool copyOK = false;
                    try
                    {
                        File.Copy(Application.ExecutablePath, myDoc + "\\CTI\\myCTI.exe");
                        copyOK = true;
                    }
                    catch
                    {
                    }
                    if (copyOK == true)
                    {
                        // TOOL.debugx("Set copyStart")
                        MeinKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\AB Server", true);
                        MeinKey.SetValue("copyStart", "1");
                        MeinKey.Close();
                        Process.Start(myDoc + "\\CTI\\myCTI.exe", "");
                        System.Environment.Exit(0);
                    }
                }
            }


            // Application.EnableVisualStyles()

            try
            {
                File.Delete(Application.StartupPath + @"\trace.txt");
            }
            catch
            {
            }
            try
            {
                File.Delete(Application.StartupPath + @"\error.txt");
            }
            catch
            {
            }

            Application.Run(new TrayContext());

        }
        else
        {
            string ziel = "";
            try
            {
                ziel = cmdArgs[0];
            }
            catch
            {
            }
            udpSend("CSTA7" + ziel);
        }
    }

    private static void udpSend(string text)
    {
        try
        {
            UdpClient udpClient = new UdpClient();
            byte[] bytCommand = new byte[] { };
            int pRet;
            IPAddress GLOIP = IPAddress.Parse("127.0.0.1");
            int GLOINTPORT = 31312;
            udpClient.Connect(GLOIP, GLOINTPORT);
            bytCommand = Encoding.Default.GetBytes(text);
            pRet = udpClient.Send(bytCommand, bytCommand.Length);
        }
        catch
        {
        }
    }
}


