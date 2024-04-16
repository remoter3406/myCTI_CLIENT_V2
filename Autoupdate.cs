using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualBasic;
namespace myCTI2
{

    public class Autoupdate
    {
        
        public bool AutoUpdate(string RemotePath, bool erzwinge = false)
        {
            string Key = "&**#@!"; // any unique sequence of characters
                                   // the file with the update information
            string sfile = "myCTI.exe";
            // the Assembly name 
            string AssemblyName = Application.StartupPath + @"\" + System.Reflection.Assembly.GetEntryAssembly().GetName().Name + ".exe";
            // where are the files for a specific system
            string RemoteUri = RemotePath + "/";
            // clean up the command line getting rid of the key
            string CommandLine = string.Join(" ", Environment.GetCommandLineArgs().Skip(1).ToArray()).Replace(Key, "");
            // Verify if was called by the autoupdate
            if (string.Join(" ", Environment.GetCommandLineArgs().Skip(1).ToArray()).IndexOf(Key) >= 0 & erzwinge == false)
            {
                //File.AppendAllText(Application.StartupPath & "\trace.txt", Now & " Autoupdate delete file erzwinge=" & vbCrLf & erzwinge.ToString() & vbCrLf, True)
                //File.AppendAllText(Application.StartupPath & "\trace.txt", Now & " commandline=" & Microsoft.VisualBasic.Command() & vbCrLf, True)
                try
                {
                    // try to delete the AutoUpdate program, 
                    // since it is not needed anymore
                    System.IO.File.Delete(Application.StartupPath + @"\autoupdate.exe");
                }
                catch
                {
                }
                // return false means that no update is needed
                return false;
            }
            else
            {
                // was called by the user
                bool ret = false; // Default - no update needed
                string Contents = "";
                string Contents2 = "";
                                                                               // File.AppendAllText(Application.StartupPath & "\trace.txt", Now & " Autoupdate read files" & vbCrLf, True)
                try
                {
                    try
                    {
                        Contents = TOOL.DownloadWebpage(RemoteUri + sfile);
                    }
                    catch (Exception ex)
                    {
                        File.AppendAllText(Application.StartupPath + @"\error.txt", DateTime.Now.ToLongTimeString() + " There was a problem runing the Auto Update (read file from web) ." + "\r" + ex.Message + "\r");
                    }

                    try
                    {
                        Contents2 = TOOL.File2String(AssemblyName);
                    }
                    catch (Exception ex)
                    {
                        File.AppendAllText(Application.StartupPath + @"\error.txt", DateTime.Now.ToLongTimeString() + " There was a problem runing the Auto Update (read selfexe) ." + "\r" + ex.Message + "\r");
                    }

                    if (Contents != Contents2 & Contents != "")
                    {
                        TOOL.DownloadFile(RemoteUri + "geckofx64.zip", Application.StartupPath + @"\geckofx64.zip",1000);
                        TOOL.DownloadFile(RemoteUri + "autoupdate.exe", Application.StartupPath + @"\autoupdate.exe",1000);

                        string arg = Application.ExecutablePath + "|" + RemoteUri + "|" + sfile + "|" + Key + "|" + string.Join(" ", Environment.GetCommandLineArgs().Skip(1).ToArray());
                        Process.Start(Application.StartupPath + @"\autoupdate.exe", arg);
                        ret = true;
                    }
                }
                catch (Exception ex)
                {
                    File.AppendAllText(Application.StartupPath + @"\error.txt", DateTime.Now + " There was a problem runing the Auto Update." + "\r" + ex.Message + "\r");
                    try
                    {
                        System.IO.File.Delete(Application.StartupPath + @"\autoupdate.exe");
                    }
                    catch
                    {
                    }
                    ret = false;
                }
                return ret;
            }
        }
    }
}