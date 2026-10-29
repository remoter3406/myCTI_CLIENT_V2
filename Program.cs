using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using myCTI2;

public static class Program
{
    [STAThread]
    public static void Main(string[] cmdArgs)
    {
        string appProcessName = Path.GetFileNameWithoutExtension(Application.ExecutablePath);
        Process[] RunningProcesses = Process.GetProcessesByName(appProcessName);

        if (LocalReg.Load("copyStart", "") == "1")
        {
            // Neustart aus Documents\CTI nach dem Kopieren
            LocalReg.Save("copyStart", "0");
            Application.Run(new TrayContext());
        }
        else if (RunningProcesses.Length <= 1)
        {
            if (Application.ExecutablePath.ToLower().IndexOf(@"\cti\") < 0)
                installFromDownload();

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
            // läuft schon: Ziel an die laufende Instanz schicken
            string ziel = cmdArgs.Length > 0 ? cmdArgs[0] : "";
            udpSend("CSTA7" + ziel);
        }
    }

    // Exe liegt nicht im CTI Ordner: Server aus dem eingebetteten Token übernehmen,
    // nach Documents\CTI kopieren und von dort neu starten
    private static void installFromDownload()
    {
        // Token "FKtokenFKstartFK<server>#<ssl>#...FKtokenFKendeFK" wird vom Server beim Download eingesetzt
        System.Resources.ResourceManager rm = new System.Resources.ResourceManager(myCTI2.Properties.Resources.token, Assembly.GetExecutingAssembly());
        string token = VB.Mid(rm.BaseName, 16, 40);
        int pos = token.IndexOf("#");
        if (pos <= 0)
            return;

        string tmp = VB.Mid(token, pos + 1);
        LocalReg.Save("server", VB.Left(token, pos).Replace(":80", "").Replace(":443", ""));
        pos = tmp.IndexOf("#");
        if (pos > 0)
            LocalReg.Save("ssl", VB.Left(tmp, pos));

        string ctiDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Documents\\CTI";
        string target = ctiDir + "\\myCTI.exe";
        try
        {
            Directory.CreateDirectory(ctiDir);
            // vorhandene (ältere) Version überschreiben
            File.Copy(Application.ExecutablePath, target, true);
        }
        catch (Exception ex)
        {
            TOOL.errorx("installFromDownload copy error: " + ex.Message);
            return;
        }
        LocalReg.Save("copyStart", "1");
        Process.Start(target, "");
        System.Environment.Exit(0);
    }

    private static void udpSend(string text)
    {
        try
        {
            using (UdpClient udpClient = new UdpClient())
            {
                udpClient.Connect(IPAddress.Loopback, 31312);
                byte[] bytCommand = Encoding.Default.GetBytes(text);
                udpClient.Send(bytCommand, bytCommand.Length);
            }
        }
        catch
        {
        }
    }
}
