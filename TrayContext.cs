using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Management;
using System.Media;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Windows.Forms;
using static System.Net.WebRequestMethods;
using File = System.IO.File;

namespace myCTI2
{
    class TrayContext : ApplicationContext
    {
        #region " DLL "
        [DllImport("shell32.dll", EntryPoint = "ShellExecute")]
        public static extern long ShellExecute(int hwnd, string cmd, string file, string param1, string param2, int swmode);

        private const int APPCOMMAND_VOLUME_MUTE = 0x80000;
        private const int WM_APPCOMMAND = 0x319;

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessageW(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        enum WTS_INFO_CLASS
        {
            WTSInitialProgram,
            WTSApplicationName,
            WTSWorkingDirectory,
            WTSOEMId,
            WTSSessionId,
            WTSUserName,
            WTSWinStationName,
            WTSDomainName,
            WTSConnectState,
            WTSClientBuildNumber,
            WTSClientName,
            WTSClientDirectory,
            WTSClientProductId,
            WTSClientHardwareId,
            WTSClientAddress,
            WTSClientDisplay,
            WTSClientProtocolType

        }

        [System.Runtime.InteropServices.DllImport("Wtsapi32.dll")]
        private static extern bool WTSQuerySessionInformation(System.IntPtr hServer, int sessionId, WTS_INFO_CLASS wtsInfoClass, out System.IntPtr ppBuffer, out uint pBytesReturned);

        #endregion

        #region " Storage "
        delegate void MySubDelegate(string text);
        private System.Windows.Forms.Timer myTimer = new System.Windows.Forms.Timer();
        private static bool red = false;
        private static bool noConnection = false;
        private static string zeit = "";
        private NotifyIcon Tray;
        private ContextMenuStrip MainMenu;
        private ToolStripMenuItem mnuDisplayForm;
        private ToolStripMenuItem mnuCTSetPosition;
        private ToolStripMenuItem mnuAUSetPosition;
        private ToolStripMenuItem mnuPositionForm;
        private ToolStripMenuItem mnuSettingsForm;
        private ToolStripSeparator mnuSep1;
        private ToolStripSeparator mnuSep2;
        private ToolStripSeparator mnuSep3;
        private ToolStripMenuItem mnuExit;
        private static myUdpClient udpClient;
        private static CTIMain CT;
        private static CTIausgabe AU;
        private static CTIausgabe AU2;
        private static CTIausgabe AU3;

        private ShutDownDummy SD;

        private string server;
        private string user;
        private string password;
        private string ssl;
        private int userx = -1;
        private string status;
        private string keycodeRelease;
        private string company = "";
        private string[] belegt = new string[4];
        private bool blinken;
        private string navigate = null;
        private string CTI_NST;
        private bool dbgTest = false;
        private bool webOK = true;
        private bool defCred = false;
        private string udpToLong = "";
        private bool isNavigated = false;
        private string RDP = "";
        #endregion

        #region " Constructor "
        public TrayContext()
        {
            init();
        }
        
        public void init()
        {
            myTimer.Tick += new EventHandler(myTimer_Tick);
            myTimer.Interval = 500;

            vbaRegOwnExePathSave();
            // dbgTest = True

            user = RegLoad("user", "");
            password = RegLoad("password", "");
            ssl = RegLoad("ssl", "http");
            server = RegLoad("server", "");
            keycodeRelease = RegLoad("kecodeRelease", "29");

            checkCred();

            //CommandLine = string.Join(" ", Environment.GetCommandLineArgs().Skip(1).ToArray());

            try
            {
                udpClient = new myUdpClient();
                udpClient.UDPreceive += udpClient_UDP_receive;
                udpClient.start(31312);
            }
            catch (Exception ex) {debugerror("trayApp NEW() setUDP error: " + ex.Message);}

            try {Set_CT_pos();} catch (Exception ex2) {debugerror("trayApp NEW() Set_CT_pos() error: " + ex2.Message);}

            try {Set_AU_pos();} catch (Exception ex2) {debugerror("trayApp NEW() Set_AU_pos() error: " + ex2.Message);}

            try {Set_ShutdownDummy();} catch (Exception ex2) {debugerror("trayApp NEW() Set_ShutdownDummy() error: " + ex2.Message);}

            try {SystemEvents.SessionSwitch += checkLockUnlock;} catch (Exception ex2) {debugerror("trayApp NEW() SystemEvents.SessionSwitch error: " + ex2.Message);}

            Init_contex_menu();
            Init_tray();

            setAudio();
 
            Tray.Visible = true;
            string x = RegLoad("udp_window_x1", "0");
            string y = RegLoad("udp_window_y1", "0");
            string s = RegLoad("server", "0");
            int x1 = VB.Val(x);
            int y1 = VB.Val(y);
               debugtrace(" Init server=" + s + " udp_window_x1=" +x + "|" + x1.ToString() + " udp_window_y1=" +y + "|" + y1.ToString());

            if ( x1 == 0 & y1 == 0)
            {
                MessageBox.Show("Neue myCTI Version!!!!\nAchtung bitte erst die folgenden 3 Anruf Popups positionieren!!!, dann erst die Popups schliessen!");
                AU3.ShowPosition();
                AU2.ShowPosition();
                AU.ShowPosition();
            }
            myTimer.Enabled = true;

 
        }

        private void Init_tray()
        {
            Tray = new NotifyIcon();
            Tray.Icon = Resource1.myCTI;
            Tray.ContextMenuStrip = MainMenu;
            Tray.Text = "myCTI application";
            Tray.DoubleClick += new EventHandler(Tray_DoubleClick);
        }
        private void Init_contex_menu()
        {
            mnuDisplayForm = new ToolStripMenuItem("myCTI anzeigen");
            mnuCTSetPosition = new ToolStripMenuItem("CTI position speichern");
            mnuAUSetPosition = new ToolStripMenuItem("Popup position speichern");
            mnuSep1 = new ToolStripSeparator();
            mnuSep2 = new ToolStripSeparator();
            mnuSep3 = new ToolStripSeparator();
            mnuPositionForm = new ToolStripMenuItem("Positionen speichern");
            mnuSettingsForm = new ToolStripMenuItem("Einstellungen");

            mnuExit = new ToolStripMenuItem("Exit");
            MainMenu = new ContextMenuStrip();
            MainMenu.Items.AddRange(new ToolStripItem[] { mnuDisplayForm, mnuSep1, mnuPositionForm, mnuSep2, mnuSettingsForm, mnuSep3, mnuExit });
 
            mnuPositionForm.DropDownItems.Add(mnuCTSetPosition);
            mnuPositionForm.DropDownItems.Add(mnuAUSetPosition);

            mnuDisplayForm.Click += new EventHandler(mnuDisplayForm_Click);
            mnuCTSetPosition.Click += new EventHandler(mnuCTSetPosition_Click);
            mnuAUSetPosition.Click += new EventHandler(mnuAUSetPosition_Click);
            mnuSettingsForm.Click += new EventHandler(mnuSettingsForm_Click);
            mnuExit.Click += new EventHandler(mnuExit_Click);
        }

        public void checkCred()
        {
            if (server == "")
            {
                InputBox serv = new InputBox("Servereingabe", "IP/Name", RegLoad("server", ""));
                serv.ShowDialog();
                if (serv.DialogResult == DialogResult.OK)
                {
                    RegSave("server", serv.Prompt);
                    server = serv.Prompt;
                }
            }

            if (user != "" & password == "")
            {
                InputBox mpassword = new InputBox("Passworteingabe", "Neues Passwort", "");
                mpassword.ShowDialog();
                if (mpassword.DialogResult == DialogResult.OK)
                {
                    RegSave("password", mpassword.Prompt);
                    password = mpassword.Prompt;
                }
            }
            if (server == "" | (user != "" & password == ""))
            {
                TOOL.debugx("Exit server=" + server + " user=" + user + " pwOK=" + (password!="").ToString());

                System.Environment.Exit(0);
            }

            /*
            string getUser = "";
            int wait = 0;
            do
            {
                getUser = getWeb("SetUDP?status=off", true);
                if (webOK == true)
                    break;
                else
                {
                    wait++;
                    if (wait > 6)
                        break;
                    Thread.Sleep(10000);
                }
            }
            while (true);// prüfe nur auf erreichbarkeit
            */

            setUserCredentials();

        }
        private void setUserCredentials()
        {
            string getUser = "";

            string userbak = user;
            user = "";
            defCred = true;
            getUser = getWeb("SetUDP?status=off", true);
            if (VB.Val(getUser) > 0)
            {
                // NTLM Authentifizierung erfolgreich
                password = "";
                user = RegLoadUserName(getUser);
            }
            else
            {
                defCred = false;
                user = userbak;
            }
        }
        public void getFile(string path, string filename)
        {
            System.Net.WebClient myWebClient = new System.Net.WebClient(); // the webclient
            try
            {
                myWebClient.DownloadFile("http://" + server + "/autoupdate/" + filename, path + filename);
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText(Application.StartupPath + @"\error.txt", DateTime.Now.ToLongTimeString() + " getFile error: " + ex.Message);
            }
        }
        
        private bool AcceptCert(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }
        
        private void Set_CT_pos()
        {
            if (CT == null)
                CT = new CTIMain();

            if (webOK == false)
                return;
            userx = VB.Val(getWeb("SetUDP?status=on" + status + "&version=" + CT.Text));
            System.IO.File.AppendAllText(Application.StartupPath + @"\trace.txt","\r\n" + DateTime.Now.ToLongTimeString() + " version=" + CT.Text + "\r\n\r\n");

            if (webOK == false) userx = -1;

            CT.Navigated += new CTIMain.NavigatedEventHandler(CT_Navigated);
            CT.NoValidUser += new CTIMain.NoValidUserEventHandler(CT_NoValidUser);
            CT.DialNumber += new CTIMain.DialNumberEventHandler(CT_DialNumber);
            CT.SpeakerButton += new CTIMain.SpeakerButtonEventHandler(CT_SpeakerButton);
            CT.SearchNumber += new CTIMain.SearchNumberEventHandler(CT_SearchNumber);
            CT.ReleaseButton += new CTIMain.ReleaseButtonEventHandler(CT_ReleaseButton);
            CT.ZeitplanButton += new CTIMain.ZeitplanButtonEventHandler(CT_ZeitplanButton);
            CT.KampagneChecked += new CTIMain.KampagneCheckedEventHandler(CT_KampagneChecked);
            CT.SecondPhoneChecked += new CTIMain.SecondPhoneCheckedEventHandler(CT_SecondPhoneChecked);

            CTI_NST = RegLoadOwnNst();
            CT.setNew(defCred, server, user, password, ssl, userx.ToString());
            CT.Show();
            CT.Hide();

            CT.Left = VB.Val(RegLoad("window_x", "100"));
            CT.Top = VB.Val(RegLoad("window_y", "100"));
            CT.Width = VB.Val(RegLoad("window_w", "500"));
            CT.Height = VB.Val(RegLoad("window_h", "500"));
            try
            {
                if (RegLoad("window_m", "") == "Maximized")
                    CT.WindowState = FormWindowState.Maximized;
                else
                    CT.WindowState = FormWindowState.Normal;
            }
            catch
            {
            }

            company = RegLoadCompany();

            if (RegLoadLostCall() != "")
                blinken = true;

            CT.setZPButton(getZeitplan());
            CT.setKampagneButton(RegLoadServer("kampagneduwa"));
            RegSaveServer("kampagneactive", "0");
            CT.setSecondPhoneButton(RegLoadServer("secondphoneduwa"));
            RegSaveServer("secondphoneactive", "0");
            string pcname = System.Windows.Forms.SystemInformation.ComputerName;
            RegSaveServer("PcName", pcname);
            ManagementObjectSearcher mSearcher = new ManagementObjectSearcher("SELECT SerialNumber, SMBIOSBIOSVersion, ReleaseDate FROM Win32_BIOS");
            ManagementObjectCollection collection = mSearcher.Get();
            foreach (ManagementObject obj in collection)
            {
                string lblBiosSerial = (string)obj["SerialNumber"];
                RegSaveServer("PcSerial", lblBiosSerial);
            }
        }
        private void Set_AU_pos()
        {
            string def1 = RegLoadServer("deflect1");
            string def2 = RegLoadServer("deflect2");
            string def3 = RegLoadServer("deflect3");

            if (AU == null)
                AU = new CTIausgabe(1,def1,def2,def3);
            if (AU2 == null)
                AU2 = new CTIausgabe(2,def1,def2,def3);
            if (AU3 == null)
                AU3 = new CTIausgabe(3,def1,def2,def3);

            AU.SendeBUTTON += new CTIausgabe.SendeBUTTONEventHandler(AUSGABE_SendeBUTTON1);
            AU2.SendeBUTTON += new CTIausgabe.SendeBUTTONEventHandler(AUSGABE_SendeBUTTON2);
            AU3.SendeBUTTON += new CTIausgabe.SendeBUTTONEventHandler(AUSGABE_SendeBUTTON3);

            AU.Show();
            AU.Hide();
            AU2.Show();
            AU2.Hide();
            AU3.Show();
            AU3.Hide();
            int x1 = VB.Val(RegLoad("udp_window_x1", "0"));
            int y1 = VB.Val(RegLoad("udp_window_y1", "0"));
            System.IO.File.AppendAllText(Application.StartupPath + @"\trace.txt", DateTime.Now.ToLongTimeString() + " Set_AU_Pos udp_window_x1=" + x1.ToString() + " udp_window_y1=" + y1.ToString() + "\r\n");

            if (x1 != 0 & y1 != 0)
                AU.Location = new Point(x1, y1);
            int x2 = VB.Val(RegLoad("udp_window_x2", "0"));
            int y2 = VB.Val(RegLoad("udp_window_y2", "0"));
            if (x2 != 0 & y2 != 0)
                AU2.Location = new Point(x2, y2);
            int x3 = VB.Val(RegLoad("udp_window_x3", "0"));
            int y3 = VB.Val(RegLoad("udp_window_y3", "0"));
            if (x3 != 0 & y3 != 0)
                AU3.Location = new Point(x3, y3);
        }
        private void Set_ShutdownDummy()
        {
            // unsichtbare Form für Shutdown Event beim herunterfahren
            if (SD == null)
                SD = new ShutDownDummy();

            SD.DialHotkey += new ShutDownDummy.DialHotkeyEventHandler(SD_DialHotkey);
            SD.WindowsShutDown += new ShutDownDummy.WindowsShutDownEventHandler(SD_WindowsShutDown);
            SD.SpaceHotkey += new ShutDownDummy.SpaceHotkeyEventHandler(SD_SpaceHotkey);
            SD.ShowInTaskbar = false;
            SD.Show();
            SD.Hide();
            SD.Left = -50;
            SD.Top = -50;
            SD.Show();
            SD.timer1.Enabled = true;
        }

        private void setAudio()
        {
            try
            {
                string audiofile = RegLoadStyle();
                if (audiofile == "") audiofile = "default";
                TOOL.audio = getWeb("css/" + audiofile + ".wav");
                if (TOOL.audio.IndexOf("FILE NOT FOUND") >= 0) TOOL.audio = "";
            }
            catch
            { }
            try
            {
                string audiofile = RegLoadStyle();
                if (audiofile == "") audiofile = "default";
                TOOL.ringAudio = getWeb("css/" + audiofile + "_ring.wav");
                if (TOOL.ringAudio.IndexOf("FILE NOT FOUND") >= 0) TOOL.ringAudio = "";
            }
            catch
            { }
        }

        private void debugtrace(string msg)
        {
            System.IO.File.AppendAllText(Application.StartupPath + @"\trace.txt", DateTime.Now.ToLongTimeString() + " " + msg + "\r\n");
        }
        private void debugerror(string msg)
        {
            System.IO.File.AppendAllText(Application.StartupPath + @"\error.txt", DateTime.Now.ToLongTimeString() + " " + msg + "\r\n");
        }

        #endregion

        #region " Event UDP / LockUnlock"
        public void checkLockUnlock(object sender, SessionSwitchEventArgs e)
        {
            try
            {
                if (e.Reason == SessionSwitchReason.SessionLock)
                {
                    getWeb("SetUDP?status=locked" + status);
                    CT.locked = true;
                }
                else
                {
                    getWeb("SetUDP?status=unlocked" + status);
                    CT.locked = false;
                }
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText(Application.StartupPath + @"\error.txt", DateTime.Now.ToLongTimeString() + " checkLockUnlock error: " + ex.Message);
            }
        }

        private void udpClient_UDP_receive(object sender, myEventArgs e)
        {
            string text = e.Text;

            TOOL.debugx(0, "udpClient_UDP_receive len=" + text.Length.ToString() + " udp=" + VB.Left(text, 60));

            if (text.Length == 60001)
            {
                udpToLong = udpToLong + VB.Left(text, 60000);
            }
            else
            {
                if (udpToLong != "")
                {
                    text = udpToLong + text;
                    udpToLong = "";
                }

                try
                {
                    MySubDelegate msd = ReceiveUDP;
                    msd.Invoke(text);
                }
                catch (Exception ex)
                {
                    System.IO.File.AppendAllText(Application.StartupPath + @"\error.txt", DateTime.Now.ToLongTimeString() + " udpClient_UDP_receive error: " + ex.Message);
                }
            }
        }
        #endregion

        #region " Event Timer "
        private void myTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (zeit != VB.Left(DateTime.Now.ToLongTimeString(), 7))
                {
                    //Alle 10sek
                    zeit = VB.Left(DateTime.Now.ToLongTimeString(),7);
                    checkServer();
                    checkRdp();

                }

                checkBlinken();

                if (navigate != null)
                {
                    CT.navigate(navigate);
                    navigate = null;
                }
            }
            catch (AccessViolationException ex)
            {
                debugerror(" myTimer_Tick error: " + ex.Message);
            }
            catch
            {
            }
        }
        
        private void checkServer()
        {
            if (!PingHost(server))
            {
                blinken = false;
                Tray.Icon = Resource1.myCTI2;
                red = false;
                noConnection = true;
            }
            else
            {
                Tray.Icon = Resource1.myCTI;
                red = false;
                noConnection = false;
            }
        }
        private void checkRdp()
        {
            string rdpName = isRDP();
            if (rdpName != "")
                CT.RDPon();
            else
                CT.RDPoff();

            if (RDP != rdpName)
            {
                string erg = getWeb("SetUDP?RDP=" + rdpName);
                if (erg.Length > 0 & erg.Length < 3)
                {
                    RDP = rdpName;
                    System.IO.File.AppendAllText(Application.StartupPath + @"\trace.txt", DateTime.Now.ToLongTimeString() + " myTimer_Tick SetUDP?RDP=" + erg + "  rdpname=" + RDP);
                }
                else
                {
                    System.IO.File.AppendAllText(Application.StartupPath + @"\error.txt", DateTime.Now.ToLongTimeString() + " myTimer_Tick SetUDP?RDP=" + erg);
                }
            }
        }
        private void checkBlinken()
        {
            if (blinken == true)
            {
                if (red == false)
                {
                    Tray.Icon = Resource1.myCTI2;
                    red = true;
                }
                else
                {
                    Tray.Icon = Resource1.myCTI;
                    red = false;
                }
            }
            else if (red == true)
            {
                Tray.Icon = Resource1.myCTI;
                red = false;
            }
        }
        #endregion

        #region " Event Tray "
        private void Tray_DoubleClick(object sender, System.EventArgs e)
        {
            if (webOK == false)
            {
                ShowSettings();
                return;
            }
            CT.OnTop();
            string link = getLostCallUrl();
            if (link != "" | !isNavigated) CT.navigate(link);
            TOOL.playAudio();
        }
        private void mnuDisplayForm_Click(object sender, System.EventArgs e)
        {
            if (webOK == false)
            {
                ShowSettings();
                return;
            }
            CT.OnTop();
            CT.navigate("");
            TOOL.playAudio();
        }
        private void mnuExit_Click(object sender, System.EventArgs e)
        {
            ShutDown();
        }
        private void mnuCTSetPosition_Click(object sender, System.EventArgs e)
        {
            string state = CT.WindowState.ToString();
            RegSave("window_m", state);
            RegSave("window_x", CT.Location.X.ToString());
            RegSave("window_y", CT.Location.Y.ToString());
            if (state != "Maximized")
            {
                RegSave("window_w", CT.Width.ToString());
                RegSave("window_h", CT.Height.ToString());
            }
        }
        private void mnuAUSetPosition_Click(object sender, System.EventArgs e)
        {
            AU3.ShowPosition();
            AU2.ShowPosition();
            AU.ShowPosition();
        }
        private void mnuSettingsForm_Click(object sender, System.EventArgs e)
        {
            ShowSettings();
            CT.setKampagneButton(RegLoadServer("kampagneduwa"));
            setAudio();
            if (CT.WindowState != FormWindowState.Minimized)
            {
                CT.navigate(getLostCallUrl());
            }
            TOOL.playAudio();
        }
        #endregion

        #region " Event Form Shutdown "
        private void SD_WindowsShutDown(object sender, EventArgs e)
        {
            ShutDown();
        }
        private void SD_DialHotkey(object sender, EventArgs e)
        {
            try
            {
                SendKeys.Send("^c");
                for (int a = 1; a <= 10; a++)
                {
                    Application.DoEvents();
                    Thread.Sleep(20);
                }
                string nr = Clipboard.GetText();
                dial(nr);
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText(Application.StartupPath + @"\error.txt", DateTime.Now.ToLongTimeString() + " SD_Hotkey error: " + ex.Message);
            }
        }
        private void SD_SpaceHotkey(object sender, EventArgs e)
        {
            AU.Visible = false;
            belegt[1] = "";
            AU2.Visible = false;
            belegt[2] = "";
            AU3.Visible = false;
            belegt[3] = "";
        }

        #endregion

        #region " Event Form CTI Main"
        private void CT_Navigated(object sender, myEventArgs e)
        {
            isNavigated = true;
            try
            {
                if (e.Text.ToUpper().IndexOf("MONITOR") >= 0)
                {
                    blinken = false;
                    getWeb("sendudp?cmd=5&user=" + userx.ToString());
                }
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText(Application.StartupPath + @"\error.txt", DateTime.Now.ToLongTimeString() + " CT_Navigated error: " + ex.Message);
            }
        }
        private void CT_DialNumber(object sender, myEventArgs e)
        {
            try
            {
                dial(e.Text);
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText(Application.StartupPath + @"\error.txt", DateTime.Now.ToLongTimeString() + " CT_DialNumber error: " + ex.Message);
            }
        }
        private void CT_ReleaseButton(object sender, EventArgs e)
        {
            try
            {
                getWeb("cti.pl?button=" + keycodeRelease);
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText(Application.StartupPath + @"\error.txt", DateTime.Now.ToLongTimeString() + " CT_ReleaseButton error: " + ex.Message);
            }
        }
        private void CT_SpeakerButton(object sender, EventArgs e)
        {
            try
            {
                getWeb("cti.pl?button=21");
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText(Application.StartupPath + @"\error.txt", DateTime.Now.ToLongTimeString() + " CT_SpeakerButton error: " + ex.Message);
            }
        }
        private void CT_SearchNumber(object sender, myEventArgs e)
        {
            try
            {
                getWeb("sendudp?cmd=6&user=" + user + "&text=" + e.Text);
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText(Application.StartupPath + @"\error.txt", DateTime.Now.ToLongTimeString() + " CT_SearchNumber error: " + ex.Message);
            }
        }
        private void CT_NoValidUser(object sender, myEventArgs e)
        {
              System.IO.File.AppendAllText(Application.StartupPath + @"\error.txt", DateTime.Now.ToLongTimeString() + " TrayContext CT_NoValidUser user= " + e.Text);
        }
        private void CT_ZeitplanButton(object sender, EventArgs e)
        {
            bool on = getZeitplan();
            on = setZeitplan(!on);
            CT.setZPButton(on);
        }
        private void CT_KampagneChecked(object sender, bool e)
        {
            if(e)
                RegSaveServer("kampagneactive", "1");
            else
                RegSaveServer("kampagneactive", "0");
        }
        private void CT_SecondPhoneChecked(object sender, bool e)
        {
            if (e)
                RegSaveServer("secondphoneactive", "1");
            else
                RegSaveServer("secondphoneactive", "0");
        }
        #endregion

        #region " EVENT FORM AUSGABE "
        private void AUSGABE_SendeBUTTON1(object sender, myEventArgs e)
        {
            string def = e.Text;
            if(!string.IsNullOrEmpty(def))
            {
                if (def.LastIndexOf("I") < 0) def = "I" + def;
                getWeb("cti.pl?deflect=" + URLEncode(def) + "&xid=" + e.Text2 + "&nst=" + e.Text3);
            }
        }
        private void AUSGABE_SendeBUTTON2(object sender, myEventArgs e)
        {
            string def = e.Text;
            if (!string.IsNullOrEmpty(def))
            {
                if (def.LastIndexOf("I") < 0) def = "I" + def;
                getWeb("cti.pl?deflect=" + URLEncode(def) + "&xid=" + e.Text2 + "&nst=" + e.Text3);
            }
        }
        private void AUSGABE_SendeBUTTON3(object sender, myEventArgs e)
        {
            string def = e.Text;
            if (!string.IsNullOrEmpty(def))
            {
                if (def.LastIndexOf("I") < 0) def = "I" + def;
                getWeb("cti.pl?deflect=" + URLEncode(def) + "&xid=" + e.Text2 + "&nst=" + e.Text3);
            }
        }
        #endregion

        #region " Sonstige "
        protected void ReceiveUDP(string text)
        {
            try
            {
                if (VB.Left(text, 4) == "CSTA")
                {
                    text = VB.Mid(text, 4);
                    string cmd = VB.Left(text, 1);
                    if (cmd == "5")
                    {
                        string blink = "";
                        int pos = text.IndexOf("|", 2);
                        if (pos > 0)
                        {
                            pos = text.IndexOf("|", pos + 1);
                            if (pos > 0)
                            {
                                pos = text.IndexOf("|", pos + 1);
                                blink = VB.Mid(text, pos + 1, 1);
                            }
                            if (blink == "1")
                                blinken = true;
                            if (blink == "0")
                                blinken = false;
                        }
                    }
                    else if (cmd == "7")
                    {
                        if (webOK == false)
                            return;
                        text = convertUTF2HTML(text);
                        text = VB.Mid(text, 1);
                        bool back = false;
                        if (text != "")
                        {
                            if (VB.Left(text, 1) == "2")
                            {
                                back = true;
                                text = VB.Mid(text, 1);
                            }
                        }

                        if (back == false)
                        {
                            CT.OnTop();
                            navigate = text; //workaround with timer
                            TOOL.playAudio();
                        }
                        else
                        {
                            vbaRegResultSave("");
                            string tmp = getWeb(text);
                            vbaRegResultSave(tmp);
                        }
                    }
                    else if (cmd == "8")
                    {
                        if (Debugger.IsAttached == false)
                        {
                            //start startGPO.exe
                            string startexe = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Documents\\CTI\\myStartGPO.exe";
                            if (File.Exists(startexe))
                            {
                                ShellExecute(0, "open", startexe, "", "", 5);
                            }
                        }
                    }
                    //else if (cmd == "9")
                        //IM_ausgabe(text); entfernt
                    else
                    {
                        string id = "";
                        int pos = text.IndexOf("|", 2);
                        if (pos > 0)
                        {
                            pos = text.IndexOf("|", pos + 1);
                            if (pos > 0)
                            {
                                pos = text.IndexOf("|", pos + 1);
                                id = VB.Mid(text, pos + 1);
                                pos = id.IndexOf("|");
                                if (pos >= 1)
                                    id = VB.Left(id, pos);
                                else
                                    id = "";
                            }
                        }

                        if (AU.Visible == false)
                            belegt[1] = "";
                        if (AU2.Visible == false)
                            belegt[2] = "";
                        if (AU3.Visible == false)
                            belegt[3] = "";
                        if (belegt[1] != "")
                            Debug.Print(id.ToString());

                        if (belegt[1] == id)
                            AU.ausgabe(text);
                        else if (belegt[2] == id)
                            AU2.ausgabe(text);
                        else if (belegt[3] == id)
                            AU3.ausgabe(text);
                        else if (belegt[1] == "")
                        {
                            AU.ausgabe(text);
                            belegt[1] = id;
                        }
                        else if (belegt[2] == "")
                        {
                            AU2.ausgabe(text);
                            belegt[2] = id;
                        }
                        else if (belegt[3] == "")
                        {
                            AU3.ausgabe(text);
                            belegt[3] = id;
                        }
                    }
                }
                else
                {
                    TOOL.debugx(0, "udpClient_UDP_receive fehler kein CSTA CMD!");
                }
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText(Application.StartupPath + @"\error.txt", DateTime.Now.ToLongTimeString() + " ReceiveUDP error: " + ex.Message);
            }
        }
        private void ShowSettings()
        {
            Settings us = new Settings(userx, defCred, company.ToUpper());
            us.ShowDialog();
            if (user != RegLoad("user", "") | password != RegLoad("password", "") | ssl != RegLoad("ssl", "http") | server != RegLoad("server", ""))
            {
                user = RegLoad("user", "");
                password = RegLoad("password", "");
                ssl = RegLoad("ssl", "http");
                server = RegLoad("server", "");
                setUserCredentials();
                string getuser = getWeb("SetUDP?status=on" + status + "&version=" + CT.Text);
                userx = VB.Val(getuser);
                if (webOK == false)
                {
                    userx = -1;
                }
                CT.setNew(defCred, server, user, password, ssl, userx.ToString());
            }
        }
        private void dial(string number)
        {
            string num = "";
            if (number != "")
            {
                if (VB.Left(number, 1) == "*" | VB.Left(number, 1) == "#" | VB.Left(number, 1) == "I")
                {
                    num = number;
                    getWeb("cti.pl?dial=" + URLEncode(num));
                }
                else
                {
                    num = Regex.Replace(number, @"[^\d+\-()]", "");
                    if (Regex.Replace(num, @"[^\d]", "") != "")
                        getWeb("cti.pl?dial=" + URLEncode(num));
                    else
                    {
                    }
                }
            }

            CT.textBox1.Text = num;
        }
        private void ShutDown()
        {
            getWeb("SetUDP?status=off" + status);
            CT.destroy();
            AU.destroy();
            AU2.destroy();
            AU3.destroy();
            udpClient.stopsvr();
            Tray.Visible = false;
            System.Environment.Exit(0);
        }
        private string getWeb(string url, bool noErrFile = false)
        {
            TOOL.debugx(1, "getWeb url=" + url);
            ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AcceptCert);
            HttpWebRequest request;
            CredentialCache mycache = new CredentialCache();
            url = ssl + "://" + server + "/" + url;
            request = (HttpWebRequest)WebRequest.Create(url);
            if (defCred == true)
            {
                request.UseDefaultCredentials = true;
                //request.Credentials = CredentialCache.DefaultNetworkCredentials;
                //NetworkCredential x = (NetworkCredential)request.Credentials;
            }
            else
            {
                mycache.Add(new Uri(url), "NTLM", new NetworkCredential(user, password));
                request.Credentials = mycache;
            }
            request.AllowAutoRedirect = true;
            request.Timeout = 5000;

            HttpWebResponse response;
            string responseFromServer = "";
            try
            {
                response = (HttpWebResponse)request.GetResponse();
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream, System.Text.Encoding.Default);
                responseFromServer = reader.ReadToEnd();
                reader.Close();
                dataStream.Close();
                response.Close();
                webOK = true;
            }
            catch (Exception ex)
            {
                webOK = true;
                if (ex.Message.ToLower().IndexOf("timeout") >= 0)
                    webOK = false;
                if (ex.Message.ToLower().IndexOf("remotename") >= 0)
                    webOK = false;

                if (noErrFile == false)
                    System.IO.File.AppendAllText(Application.StartupPath + @"\error.txt", DateTime.Now.ToLongTimeString() + " GetURL error:" + ex.Message + "\r");
            }
            if (responseFromServer.Length > 20)
                TOOL.debugx(1, "getWeb defCred=" + defCred.ToString() + " responselen=" + responseFromServer.Length.ToString());
            else
                TOOL.debugx(1, "getWeb defCred=" + defCred.ToString() + " response=" + responseFromServer);

            //TOOL.debugx(2, "getWeb defCred=" + defCred.ToString() + " response=" + responseFromServer);

            return responseFromServer;
        }
        private bool PingHost(string nameOrAddress)
        {
            bool pingable = false;
            Ping pinger = null;

            try
            {
                pinger = new Ping();
                PingReply reply = pinger.Send(nameOrAddress);
                pingable = reply.Status == IPStatus.Success;
            }
            catch (PingException)
            {
                // Discard PingExceptions and return false;
            }
            finally
            {
                if (pinger != null)
                {
                    pinger.Dispose();
                }
            }

            return pingable;
        }


        private string URLPathEncode(string text)
        {
            int i = 0;
            short C = 0;
            if (text == "")
                return "";
            for (i = text.Length - 1; i >= 0; i += -1)
            {
                C = VB.Asc(VB.Mid(text, i, 1));
                // do nothing
                if ((C >= 97 & C <= 122) | (C >= 45 & C <= 58) | (C >= 63 & C <= 90) | C == 33 | C == 36 | C == 42 | C == 95 | C == 126)
                {
                }
                else
                    text = VB.Left(text, i) + ("%" + C.ToString("X2")) + VB.Mid(text, i + 1);
            }
            return text;
        }
        private string URLEncode(string text)
        {
            string tmp = URLPathEncode(text);
            return tmp.Replace("%20", "+");

        }
        private void RegSave(string var, string para)
        {
            RegistryKey MeinKey;
            MeinKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\AB Server", true);
            MeinKey.SetValue(var, para);
            MeinKey.Close();
        }
        private void vbaRegOwnExePathSave()
        {
            try
            {
                RegistryKey MeinKey;
                if (regSubKeyexists(@"SOFTWARE\VB and VBA Program Settings\AB Server") == false)
                    MeinKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\VB and VBA Program Settings\AB Server");
                if (regSubKeyexists(@"SOFTWARE\VB and VBA Program Settings\AB Server\General") == false)
                    MeinKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\VB and VBA Program Settings\AB Server\General");
                MeinKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\VB and VBA Program Settings\AB Server\General", true);
                MeinKey.SetValue("ownExePath", Application.ExecutablePath);
                MeinKey.Close();
            }
            catch (Exception ex)
            {
                TOOL.debugx(2, ex.Message);
            }
        }
        private void vbaRegResultSave(string text)
        {
            try
            {
                RegistryKey MeinKey;
                MeinKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\VB and VBA Program Settings\AB Server\General", true);
                MeinKey.SetValue("Result", text);
                MeinKey.Close();
            }
            catch (Exception ex)
            {
                TOOL.debugx(2, ex.Message);
            }
        }
        private bool regSubKeyexists(string Subkey)
        {
            RegistryKey myKey = Registry.CurrentUser.OpenSubKey(Subkey);
            if (myKey == null)
                return false;
            else
                return true;
        }
        private string RegLoad(string var, string defaultx = "")
        {
            RegistryKey MeinKey;
            MeinKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\AB Server", true);
            if (MeinKey == null)
                MeinKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\AB Server");
            return MeinKey.GetValue(var, defaultx).ToString();
        }
        private string RegLoadServer(string key)
        {
            string var = @"CTI\" + "user" + userx.ToString() + @"\CTI" + key;
            string tmp = getWeb("action?cmd=getreg&var=" + HttpUtility.UrlEncode(var, System.Text.Encoding.Default));
            if (VB.Right(tmp, 2) == "\r\n")
                tmp = VB.Left(tmp, tmp.Length - 2);
            return tmp;
        }
        private string RegLoadCompany()
        {
            string var = @"SETTINGS\Company";
            string tmp = getWeb("action?cmd=getreg&var=" + HttpUtility.UrlEncode(var));
            if (VB.Right(tmp, 2) == "\r\n")
                tmp = VB.Left(tmp, tmp.Length - 2);
            return tmp;
        }
        private string RegLoadLostCall()
        {
            string var = @"CTI\User" + userx.ToString() + @"\CTILostCall";
            string tmp = getWeb("action?cmd=getreg&var=" + HttpUtility.UrlEncode(var));
            if (VB.Right(tmp, 2) == "\r\n")
                tmp = VB.Left(tmp, tmp.Length - 2);
            return tmp;
        }
        private string RegLoadOwnNst()
        {
            string var = @"SECURITY\User" + userx.ToString() + @"\MonitorDefNST";
            string tmp = getWeb("action?cmd=getreg&var=" + HttpUtility.UrlEncode(var));
            if (VB.Right(tmp, 2) == "\r\n")
                tmp = VB.Left(tmp, tmp.Length - 2);
            return tmp;
        }
        private string RegLoadSipNst()
        {
            string var = @"SECURITY\User" + userx.ToString() + @"\SipNst";
            string tmp = getWeb("action?cmd=getreg&var=" + HttpUtility.UrlEncode(var));
            if (VB.Right(tmp, 2) == "\r\n")
                tmp = VB.Left(tmp, tmp.Length - 2);
            return tmp;
        }
        private string RegLoadSipPwd()
        {
            string var = @"SECURITY\User" + userx.ToString() + @"\SipPwd";
            string tmp = getWeb("action?cmd=getreg&var=" + HttpUtility.UrlEncode(var));
            if (VB.Right(tmp, 2) == "\r\n")
                tmp = VB.Left(tmp, tmp.Length - 2);
            return tmp;
        }
        private string RegLoadUserName(string userID)
        {
            string var = @"ACCOUNTS\User" + userID;
            string tmp = getWeb("action?cmd=getreg&var=" + HttpUtility.UrlEncode(var));
            if (VB.Right(tmp, 2) == "\r\n")
                tmp = VB.Left(tmp, tmp.Length - 2);
            return tmp;
        }
        private string RegLoadStyle()
        {
            string var = "security\\" + "user" + userx.ToString() + "\\style";
            string tmp = getWeb("action?cmd=getreg&var=" + HttpUtility.UrlEncode(var)).Replace("\n", "").Replace("\r", "");
            return tmp;
        }
        private void RegSaveServer(string key, string wert)
        {
            string var = @"CTI\" + "user" + userx.ToString() + @"\CTI" + key;
            getWeb("action?cmd=setreg&var=" + HttpUtility.UrlEncode(var, System.Text.Encoding.Default) + "&wert=" + HttpUtility.UrlEncode(wert, System.Text.Encoding.Default));
        }
        private bool PWcheck(string text)
        {
            int i = 0;
            if (string.IsNullOrEmpty(text))
            {
                MessageBox.Show("Passwort kann nicht leer sein");
                return false;
            }
            else
            {
                for (i = 0; i < text.Length; i++)
                {
                    if (VB.Asc(VB.Mid(text, i, 1)) > 127 | VB.Mid(text, i, 1) == "/")
                    {
                        MessageBox.Show("Sonderzeichen " + VB.Mid(text, i, 1) + " kann nicht verwendet werden");
                        return false;
                    }
                }
                return true;
            }
        }
        private string PWencode(string text)
        {
            int i = 0;
            string tmp = "";
            if (string.IsNullOrEmpty(text))
                return "";
            else
            {
                byte[] bytCommand = new byte[] { };
                bytCommand = Encoding.Default.GetBytes(text);
                int x = bytCommand.Length;
                for (i = 0; i < x; i++)
                {
                    byte C = bytCommand[i];
                    tmp = tmp + C.ToString("X2");
                }
                return tmp;
            }
        }

        private string convertUTF2HTML(string unicodeString)
        {
            unicodeString = unicodeString.Replace("Ä", "%C4");
            unicodeString = unicodeString.Replace("Ö", "%D6");
            unicodeString = unicodeString.Replace("Ü", "%DC");
            unicodeString = unicodeString.Replace("ä", "%E4");
            unicodeString = unicodeString.Replace("ö", "%F6");
            unicodeString = unicodeString.Replace("ü", "%FC");
            unicodeString = unicodeString.Replace("ß", "%DF");
            return unicodeString;
        }
        private string getLostCallUrl()
        {
            int pos;
            string tmp;
            DateTime tmpdate = new DateTime(2099,1,1,12,0,0);
            string filter = "";
            string lost = RegLoadLostCall();

            do
            {
                pos = lost.IndexOf("|");
                if (pos < 0)
                    break;
                tmp = VB.Left(lost, pos);
                lost = VB.Mid(lost, pos + 1);
                string nst;
                DateTime dat;
                pos = tmp.IndexOf(",");
                if (pos > 0)
                {
                    nst = VB.Left(tmp, pos);
                    //TODO? Convert string wie VB
                    dat = Convert.ToDateTime(VB.Mid(tmp, pos + 1));
                    if (dat < tmpdate)
                        tmpdate = dat;
                    filter = filter + nst + ",";
                }
            }
            while (true);
            if (filter != "")
            {
                tmpdate = tmpdate.AddMinutes(-3);
                return "Monitor.PL?datum=" + tmpdate.ToBinary().ToString() + "&filter=" + filter;
            }
            else
            {
                return RegLoad("DoubleClickLink", "");
                //return "Kontakte.html";
            }
        }

        private bool getZeitplan()
        {
            string ret = getWeb("cti.pl?zeitplan=get");
            if (ret == "on") return true;
            return false;
        }
        private bool setZeitplan(bool on)
        {
            string onOff = "off";
            if (on) onOff = "on";
            string ret= getWeb("cti.pl?zeitplan=" + onOff);
            if (ret == "on") return true;
            return false;
        }
        /*
        private void mnuPasswordChangeForm_Click(object sender, System.EventArgs e)
        {
            InputBox mpassword = new InputBox("Passworteingabe", "Altes Passwort", "", true);
            string pwneu;
            mpassword.ShowDialog();
            if (mpassword.DialogResult == DialogResult.OK)
            {
                string alt = mpassword.Prompt;
                InputBox mpassword1 = new InputBox("Passworteingabe", "Neues Passwort", "", true);
                mpassword1.ShowDialog();
                if (mpassword1.DialogResult == DialogResult.OK)
                {
                    string neu = mpassword1.Prompt;
                    InputBox mpassword2 = new InputBox("Passworteingabe", "Neues Passwort wiederholen", "", true);
                    mpassword2.ShowDialog();
                    if (mpassword2.DialogResult == DialogResult.OK)
                    {
                        pwneu = mpassword2.Prompt;
                        if (neu == pwneu)
                        {
                            if (PWcheck(neu) == true)
                            {
                                string erg = getWeb("cpw?alt=" + PWencode(alt) + "&neu=" + PWencode(neu));
                                if (VB.Left(erg, 4) == "true")
                                {
                                    MessageBox.Show("Passwort erfolgreich geändert");
                                    RegSave("password", pwneu);
                                    password = pwneu;
                                    CT.setNew(NTLM, server, user, password, ssl, userx.ToString());
                                }
                                else
                                    MessageBox.Show("Passwort konnte nicht geändert werden");
                            }
                        }
                    }
                }
            }
        }
        */

        private string isRDP()
        {
            WindowsPrincipal wp = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            string pcName = GetTerminalServerClientNameWTSAPI();
            return pcName;
        }
        private static string GetTerminalServerClientNameWTSAPI()
        {

            const int WTS_CURRENT_SERVER_HANDLE = -1;

            IntPtr buffer = IntPtr.Zero;
            uint bytesReturned;

            string strReturnValue = "";
            try
            {
                WTSQuerySessionInformation(IntPtr.Zero, WTS_CURRENT_SERVER_HANDLE, WTS_INFO_CLASS.WTSClientName, out buffer, out bytesReturned);
                strReturnValue = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(buffer);
            }

            finally
            {
                buffer = IntPtr.Zero;
            }

            return strReturnValue;
        }

        #endregion
    }

    public class myEventArgs : EventArgs
    {
        private readonly string text;
        private readonly string text2;
        private readonly string text3;

        public myEventArgs(string text)
        {
            this.text = text;
        }
        public myEventArgs(string text, string text2)
        {
            this.text = text;
            this.text2 = text2;
        }
        public myEventArgs(string text, string text2, string text3)
        {
            this.text = text;
            this.text2 = text2;
            this.text3 = text3;
        }
        public string Text
        {
            get { return this.text; }
        }
        public string Text2
        {
            get { return this.text2; }
        }
        public string Text3
        {
            get { return this.text3; }
        }
    }

    public static class AudioManager
    {
        /// <summary>
        /// Gets the mute state of the master volume. 
        /// While the volume can be muted the <see cref="GetMasterVolume"/> will still return the pre-muted volume value.
        /// </summary>
        /// <returns>false if not muted, true if volume is muted</returns>
        public static bool GetMasterVolumeMute()
        {
            IAudioEndpointVolume masterVol = null;
            try
            {
                masterVol = GetMasterVolumeObject();
                if (masterVol == null)
                    return false;

                bool isMuted;
                masterVol.GetMute(out isMuted);
                return isMuted;
            }
            finally
            {
                if (masterVol != null)
                    Marshal.ReleaseComObject(masterVol);
            }
        }

        private static IAudioEndpointVolume GetMasterVolumeObject()
        {
            IMMDeviceEnumerator deviceEnumerator = null;
            IMMDevice speakers = null;
            try
            {
                deviceEnumerator = (IMMDeviceEnumerator)(new MMDeviceEnumerator());
                deviceEnumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, out speakers);

                Guid IID_IAudioEndpointVolume = typeof(IAudioEndpointVolume).GUID;
                object o;
                speakers.Activate(ref IID_IAudioEndpointVolume, 0, IntPtr.Zero, out o);
                IAudioEndpointVolume masterVol = (IAudioEndpointVolume)o;

                return masterVol;
            }
            finally
            {
                if (speakers != null) Marshal.ReleaseComObject(speakers);
                if (deviceEnumerator != null) Marshal.ReleaseComObject(deviceEnumerator);
            }
        }

        #region Abstracted COM interfaces from Windows CoreAudio API

        [ComImport]
        [Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
        internal class MMDeviceEnumerator
        {
        }

        internal enum EDataFlow
        {
            eRender,
            eCapture,
            eAll,
            EDataFlow_enum_count
        }

        internal enum ERole
        {
            eConsole,
            eMultimedia,
            eCommunications,
            ERole_enum_count
        }

        [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IMMDeviceEnumerator
        {
            int NotImpl1();

            [PreserveSig]
            int GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role, out IMMDevice ppDevice);
        }

        [Guid("D666063F-1587-4E43-81F1-B948E807363F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IMMDevice
        {
            [PreserveSig]
            int Activate(ref Guid iid, int dwClsCtx, IntPtr pActivationParams, [MarshalAs(UnmanagedType.IUnknown)] out object ppInterface);
        }


        // http://netcoreaudio.codeplex.com/SourceControl/latest#trunk/Code/CoreAudio/Interfaces/IAudioEndpointVolume.cs
        [Guid("5CDF2C82-841E-4546-9722-0CF74078229A"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IAudioEndpointVolume
        {
            [PreserveSig]
            int NotImpl1();

            [PreserveSig]
            int NotImpl2();

            /// <summary>
            /// Gets a count of the channels in the audio stream.
            /// </summary>
            /// <param name="channelCount">The number of channels.</param>
            /// <returns>An HRESULT code indicating whether the operation passed of failed.</returns>
            [PreserveSig]
            int GetChannelCount(
                [Out] [MarshalAs(UnmanagedType.U4)] out UInt32 channelCount);

            /// <summary>
            /// Sets the master volume level of the audio stream, in decibels.
            /// </summary>
            /// <param name="level">The new master volume level in decibels.</param>
            /// <param name="eventContext">A user context value that is passed to the notification callback.</param>
            /// <returns>An HRESULT code indicating whether the operation passed of failed.</returns>
            [PreserveSig]
            int SetMasterVolumeLevel(
                [In] [MarshalAs(UnmanagedType.R4)] float level,
                [In] [MarshalAs(UnmanagedType.LPStruct)] Guid eventContext);

            /// <summary>
            /// Sets the master volume level, expressed as a normalized, audio-tapered value.
            /// </summary>
            /// <param name="level">The new master volume level expressed as a normalized value between 0.0 and 1.0.</param>
            /// <param name="eventContext">A user context value that is passed to the notification callback.</param>
            /// <returns>An HRESULT code indicating whether the operation passed of failed.</returns>
            [PreserveSig]
            int SetMasterVolumeLevelScalar(
                [In] [MarshalAs(UnmanagedType.R4)] float level,
                [In] [MarshalAs(UnmanagedType.LPStruct)] Guid eventContext);

            /// <summary>
            /// Gets the master volume level of the audio stream, in decibels.
            /// </summary>
            /// <param name="level">The volume level in decibels.</param>
            /// <returns>An HRESULT code indicating whether the operation passed of failed.</returns>
            [PreserveSig]
            int GetMasterVolumeLevel(
                [Out] [MarshalAs(UnmanagedType.R4)] out float level);

            /// <summary>
            /// Gets the master volume level, expressed as a normalized, audio-tapered value.
            /// </summary>
            /// <param name="level">The volume level expressed as a normalized value between 0.0 and 1.0.</param>
            /// <returns>An HRESULT code indicating whether the operation passed of failed.</returns>
            [PreserveSig]
            int GetMasterVolumeLevelScalar(
                [Out] [MarshalAs(UnmanagedType.R4)] out float level);

            /// <summary>
            /// Sets the volume level, in decibels, of the specified channel of the audio stream.
            /// </summary>
            /// <param name="channelNumber">The channel number.</param>
            /// <param name="level">The new volume level in decibels.</param>
            /// <param name="eventContext">A user context value that is passed to the notification callback.</param>
            /// <returns>An HRESULT code indicating whether the operation passed of failed.</returns>
            [PreserveSig]
            int SetChannelVolumeLevel(
                [In] [MarshalAs(UnmanagedType.U4)] UInt32 channelNumber,
                [In] [MarshalAs(UnmanagedType.R4)] float level,
                [In] [MarshalAs(UnmanagedType.LPStruct)] Guid eventContext);

            /// <summary>
            /// Sets the normalized, audio-tapered volume level of the specified channel in the audio stream.
            /// </summary>
            /// <param name="channelNumber">The channel number.</param>
            /// <param name="level">The new master volume level expressed as a normalized value between 0.0 and 1.0.</param>
            /// <param name="eventContext">A user context value that is passed to the notification callback.</param>
            /// <returns>An HRESULT code indicating whether the operation passed of failed.</returns>
            [PreserveSig]
            int SetChannelVolumeLevelScalar(
                [In] [MarshalAs(UnmanagedType.U4)] UInt32 channelNumber,
                [In] [MarshalAs(UnmanagedType.R4)] float level,
                [In] [MarshalAs(UnmanagedType.LPStruct)] Guid eventContext);

            /// <summary>
            /// Gets the volume level, in decibels, of the specified channel in the audio stream.
            /// </summary>
            /// <param name="channelNumber">The zero-based channel number.</param>
            /// <param name="level">The volume level in decibels.</param>
            /// <returns>An HRESULT code indicating whether the operation passed of failed.</returns>
            [PreserveSig]
            int GetChannelVolumeLevel(
                [In] [MarshalAs(UnmanagedType.U4)] UInt32 channelNumber,
                [Out] [MarshalAs(UnmanagedType.R4)] out float level);

            /// <summary>
            /// Gets the normalized, audio-tapered volume level of the specified channel of the audio stream.
            /// </summary>
            /// <param name="channelNumber">The zero-based channel number.</param>
            /// <param name="level">The volume level expressed as a normalized value between 0.0 and 1.0.</param>
            /// <returns>An HRESULT code indicating whether the operation passed of failed.</returns>
            [PreserveSig]
            int GetChannelVolumeLevelScalar(
                [In] [MarshalAs(UnmanagedType.U4)] UInt32 channelNumber,
                [Out] [MarshalAs(UnmanagedType.R4)] out float level);

            /// <summary>
            /// Sets the muting state of the audio stream.
            /// </summary>
            /// <param name="isMuted">True to mute the stream, or false to unmute the stream.</param>
            /// <param name="eventContext">A user context value that is passed to the notification callback.</param>
            /// <returns>An HRESULT code indicating whether the operation passed of failed.</returns>
            [PreserveSig]
            int SetMute(
                [In] [MarshalAs(UnmanagedType.Bool)] Boolean isMuted,
                [In] [MarshalAs(UnmanagedType.LPStruct)] Guid eventContext);

            /// <summary>
            /// Gets the muting state of the audio stream.
            /// </summary>
            /// <param name="isMuted">The muting state. True if the stream is muted, false otherwise.</param>
            /// <returns>An HRESULT code indicating whether the operation passed of failed.</returns>
            [PreserveSig]
            int GetMute(
                [Out] [MarshalAs(UnmanagedType.Bool)] out Boolean isMuted);

            /// <summary>
            /// Gets information about the current step in the volume range.
            /// </summary>
            /// <param name="step">The current zero-based step index.</param>
            /// <param name="stepCount">The total number of steps in the volume range.</param>
            /// <returns>An HRESULT code indicating whether the operation passed of failed.</returns>
            [PreserveSig]
            int GetVolumeStepInfo(
                [Out] [MarshalAs(UnmanagedType.U4)] out UInt32 step,
                [Out] [MarshalAs(UnmanagedType.U4)] out UInt32 stepCount);

        }

        #endregion
    }

    public class ShortcutCreator
    {
        // COM-Schnittstellen Definitionen (Win32)
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("000214F9-0000-0000-C000-000000000046")]
        private interface IShellLink
        {
            void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, IntPtr pfd, int fFlags);
            void GetIDList(out IntPtr ppidl);
            void SetIDList(IntPtr pidl);
            void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);
            void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
            void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
            void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
            void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
            void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
            void GetHotkey(out short pwHotkey);
            void SetHotkey(short wHotkey);
            void GetShowCmd(out int piShowCmd);
            void SetShowCmd(int iShowCmd);
            void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);
            void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
            void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, int dwReserved);
            void Resolve(IntPtr hwnd, int fFlags);
            void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
        }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("0000010b-0000-0000-C000-000000000046")]
        private interface IPersistFile
        {
            void GetClassID(out Guid pClassID);
            void IsDirty();
            void Load([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, int dwMode);
            void Save([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, bool fRemember);
            void SaveCompleted([MarshalAs(UnmanagedType.LPWStr)] string pszFileName);
            void GetCurFile([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder ppszFileName);
        }

        // ShellLink CoClass
        [ComImport]
        [Guid("00021401-0000-0000-C000-000000000046")]
        private class ShellLink { }

        public static void Create(string linkPath, string targetPath, string description, string workingDirectory)
        {
            // 1. Instanz erstellen
            var shellLink = (IShellLink)new ShellLink();

            // 2. Eigenschaften setzen
            shellLink.SetPath(targetPath);
            shellLink.SetDescription(description);
            shellLink.SetWorkingDirectory(workingDirectory);

            // 3. Als .lnk Datei speichern
            var persistFile = (IPersistFile)shellLink;
            persistFile.Save(linkPath, true);
        }
    }

}
