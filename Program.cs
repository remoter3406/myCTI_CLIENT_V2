using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using myCTI2;

public static class Program
{
    [STAThread]
    public static void Main(string[] cmdArgs)
    {
        // Visual Styles, High-DPI und Standardschrift aus der Projektdatei (ApplicationHighDpiMode usw.)
        ApplicationConfiguration.Initialize();

        if (LocalReg.Load("copyStart", "") == "1")
        {
            // Neustart aus Documents\CTI nach dem Kopieren; der kopierende Prozess
            // hält den Mutex evtl. noch kurz, deshalb warten statt prüfen
            LocalReg.Save("copyStart", "0");
            acquireSingleInstance(5000);
            Application.Run(new TrayContext());
        }
        else if (acquireSingleInstance(0) && !otherProcessRunning())
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

    // Einzelinstanz rechnerweit über einen Mutex, unabhängig vom Dateinamen der Exe
    // (z.B. "myCTI (1).exe" nach erneutem Download). Muss bis Programmende referenziert bleiben.
    private static Mutex instanceMutex;

    private static bool acquireSingleInstance(int waitMs)
    {
        try
        {
            instanceMutex = new Mutex(false, @"Global\myCTI_Client");
            try
            {
                return instanceMutex.WaitOne(waitMs);
            }
            catch (AbandonedMutexException)
            {
                // Vorgänger wurde beendet ohne freizugeben (z.B. nach dem Kopieren)
                return true;
            }
        }
        catch (UnauthorizedAccessException)
        {
            // Mutex gehört einer Instanz eines anderen Benutzers
            return false;
        }
    }

    // zusätzlich: ältere Versionen ohne Mutex unter gleichem Prozessnamen erkennen
    private static bool otherProcessRunning()
    {
        string appProcessName = Path.GetFileNameWithoutExtension(Application.ExecutablePath);
        return Process.GetProcessesByName(appProcessName).Length > 1;
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
                byte[] bytCommand = TOOL.Ansi.GetBytes(text);
                udpClient.Send(bytCommand, bytCommand.Length);
            }
        }
        catch
        {
        }
    }
}
