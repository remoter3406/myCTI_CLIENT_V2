using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Management;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace myCTI2
{
    class TrayContext : ApplicationContext
    {
        #region " DLL "
        [DllImport("shell32.dll", EntryPoint = "ShellExecute")]
        public static extern long ShellExecute(int hwnd, string cmd, string file, string param1, string param2, int swmode);

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

        [DllImport("Wtsapi32.dll")]
        private static extern bool WTSQuerySessionInformation(IntPtr hServer, int sessionId, WTS_INFO_CLASS wtsInfoClass, out IntPtr ppBuffer, out uint pBytesReturned);
        [DllImport("Wtsapi32.dll")]
        private static extern void WTSFreeMemory(IntPtr pMemory);
        #endregion

        #region " Storage "
        private System.Windows.Forms.Timer myTimer = new System.Windows.Forms.Timer();
        private static bool red = false;
        private DateTime lastServerCheck = DateTime.MinValue;
        private int serverCheckRunning = 0;
        private Control uiInvoker;
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
        private static CTIausgabe[] AU = new CTIausgabe[3];   // Anruf Popups 1..3
        private string[] belegt = { "", "", "" };             // Anruf-ID je Popup

        private ShutDownDummy SD;

        private CtiServer web = new CtiServer();
        private int userx = -1;
        private string keycodeRelease;
        private bool blinken;
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
            // unsichtbares Control, um UDP Events in den UI Thread zu holen
            uiInvoker = new Control();
            IntPtr h = uiInvoker.Handle;

            CtiServer.AcceptAllCertificates();

            myTimer.Tick += new EventHandler(myTimer_Tick);
            myTimer.Interval = 500;

            vbaRegOwnExePathSave();

            web.User = LocalReg.Load("user", "");
            web.Password = LocalReg.Load("password", "");
            web.Ssl = LocalReg.Load("ssl", "http");
            web.Server = LocalReg.Load("server", "");
            loadKeycodeRelease();

            checkCred();

            try
            {
                udpClient = new myUdpClient();
                udpClient.UDPreceive += udpClient_UDP_receive;
                udpClient.setServer(web.Server);
                udpClient.start(31312);
            }
            catch (Exception ex) { TOOL.errorx("trayApp NEW() setUDP error: " + ex.Message); }

            try { Set_CT_pos(); } catch (Exception ex2) { TOOL.errorx("trayApp NEW() Set_CT_pos() error: " + ex2.Message); }

            try { Set_AU_pos(); } catch (Exception ex2) { TOOL.errorx("trayApp NEW() Set_AU_pos() error: " + ex2.Message); }

            try { Set_ShutdownDummy(); } catch (Exception ex2) { TOOL.errorx("trayApp NEW() Set_ShutdownDummy() error: " + ex2.Message); }

            try { SystemEvents.SessionSwitch += checkLockUnlock; } catch (Exception ex2) { TOOL.errorx("trayApp NEW() SystemEvents.SessionSwitch error: " + ex2.Message); }

            Init_contex_menu();
            Init_tray();

            setAudio();

            Tray.Visible = true;
            string x = LocalReg.Load("udp_window_x1", "0");
            string y = LocalReg.Load("udp_window_y1", "0");
            int x1 = VB.Val(x);
            int y1 = VB.Val(y);
            TOOL.debugx("Init server=" + web.Server + " udp_window_x1=" + x + "|" + x1.ToString() + " udp_window_y1=" + y + "|" + y1.ToString());

            if (x1 == 0 & y1 == 0)
            {
                MessageBox.Show("Neue myCTI Version!!!!\nAchtung bitte erst die folgenden 3 Anruf Popups positionieren!!!, dann erst die Popups schliessen!");
                showPopupPositions();
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
            if (web.Server == "")
            {
                InputBox serv = new InputBox("Servereingabe", "IP/Name", LocalReg.Load("server", ""));
                serv.ShowDialog();
                if (serv.DialogResult == DialogResult.OK)
                {
                    LocalReg.Save("server", serv.Prompt);
                    web.Server = serv.Prompt;
                }
            }

            if (web.User != "" & web.Password == "")
            {
                InputBox mpassword = new InputBox("Passworteingabe", "Neues Passwort", "");
                mpassword.ShowDialog();
                if (mpassword.DialogResult == DialogResult.OK)
                {
                    LocalReg.Save("password", mpassword.Prompt);
                    web.Password = mpassword.Prompt;
                }
            }
            if (web.Server == "" | (web.User != "" & web.Password == ""))
            {
                TOOL.debugx("Exit server=" + web.Server + " user=" + web.User + " pwOK=" + (web.Password != "").ToString());
                System.Environment.Exit(0);
            }

            setUserCredentials();
        }
        private void setUserCredentials()
        {
            // erst Windows Anmeldung (NTLM SSO) probieren, sonst gespeicherter User
            string userbak = web.User;
            web.User = "";
            web.DefCred = true;
            string getUser = web.Get("SetUDP?status=off", true);
            if (VB.Val(getUser) > 0)
            {
                web.Password = "";
                web.User = web.GetReg(@"ACCOUNTS\User" + getUser);
            }
            else
            {
                web.DefCred = false;
                web.User = userbak;
            }
        }

        private void Set_CT_pos()
        {
            if (CT == null)
                CT = new CTIMain();

            if (web.WebOK == false)
                return;
            userx = VB.Val(web.Get("SetUDP?status=on&version=" + TOOL.UrlEnc(CT.Text)));
            TOOL.debugx("version=" + CT.Text);

            if (web.WebOK == false) userx = -1;

            CT.Navigated += new CTIMain.NavigatedEventHandler(CT_Navigated);
            CT.NoValidUser += new CTIMain.NoValidUserEventHandler(CT_NoValidUser);
            CT.DialNumber += new CTIMain.DialNumberEventHandler(CT_DialNumber);
            CT.SpeakerButton += new CTIMain.SpeakerButtonEventHandler(CT_SpeakerButton);
            CT.SearchNumber += new CTIMain.SearchNumberEventHandler(CT_SearchNumber);
            CT.ReleaseButton += new CTIMain.ReleaseButtonEventHandler(CT_ReleaseButton);
            CT.ZeitplanButton += new CTIMain.ZeitplanButtonEventHandler(CT_ZeitplanButton);
            CT.KampagneChecked += new CTIMain.KampagneCheckedEventHandler(CT_KampagneChecked);
            CT.SecondPhoneChecked += new CTIMain.SecondPhoneCheckedEventHandler(CT_SecondPhoneChecked);

            CT.setNew(web.DefCred, web.Server, web.User, web.Password, web.Ssl, userx.ToString());
            CT.Show();
            CT.Hide();

            CT.Left = VB.Val(LocalReg.Load("window_x", "100"));
            CT.Top = VB.Val(LocalReg.Load("window_y", "100"));
            CT.Width = VB.Val(LocalReg.Load("window_w", "500"));
            CT.Height = VB.Val(LocalReg.Load("window_h", "500"));
            try
            {
                if (LocalReg.Load("window_m", "") == "Maximized")
                    CT.WindowState = FormWindowState.Maximized;
                else
                    CT.WindowState = FormWindowState.Normal;
            }
            catch
            {
            }

            if (RegLoadLostCall() != "")
                blinken = true;

            CT.setZPButton(getZeitplan());
            CT.setKampagneButton(RegLoadServer("kampagneduwa"));
            RegSaveServer("kampagneactive", "0");
            CT.setSecondPhoneButton(RegLoadServer("secondphoneduwa"));
            RegSaveServer("secondphoneactive", "0");
            RegSaveServer("PcName", SystemInformation.ComputerName);
            using (ManagementObjectSearcher mSearcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BIOS"))
            {
                foreach (ManagementObject obj in mSearcher.Get())
                {
                    RegSaveServer("PcSerial", (string)obj["SerialNumber"]);
                }
            }
        }
        private void Set_AU_pos()
        {
            string def1 = RegLoadServer("deflect1");
            string def2 = RegLoadServer("deflect2");
            string def3 = RegLoadServer("deflect3");

            for (int i = 0; i < AU.Length; i++)
            {
                if (AU[i] == null)
                    AU[i] = new CTIausgabe(i + 1, def1, def2, def3);
                AU[i].SendeBUTTON += new CTIausgabe.SendeBUTTONEventHandler(AUSGABE_SendeBUTTON);
                AU[i].Show();
                AU[i].Hide();
            }
            for (int i = 0; i < AU.Length; i++)
            {
                int x = VB.Val(LocalReg.Load("udp_window_x" + (i + 1).ToString(), "0"));
                int y = VB.Val(LocalReg.Load("udp_window_y" + (i + 1).ToString(), "0"));
                TOOL.debugx("Set_AU_Pos udp_window_x" + (i + 1).ToString() + "=" + x.ToString() + " udp_window_y" + (i + 1).ToString() + "=" + y.ToString());
                if (x != 0 & y != 0)
                    AU[i].Location = new Point(x, y);
            }
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
        }

        private void setAudio()
        {
            string audiofile = RegLoadStyle();
            if (audiofile == "") audiofile = "default";
            TOOL.audio = loadWav("css/" + audiofile + ".wav");
            TOOL.ringAudio = loadWav("css/" + audiofile + "_ring.wav");
        }
        private string loadWav(string path)
        {
            try
            {
                string wav = web.Get(path);
                if (wav.IndexOf("FILE NOT FOUND") >= 0) return "";
                return wav;
            }
            catch
            {
                return "";
            }
        }
        private void loadKeycodeRelease()
        {
            // Settings speichert "keycodeRelease", ältere Versionen lasen "kecodeRelease"
            keycodeRelease = LocalReg.Load("keycodeRelease", LocalReg.Load("kecodeRelease", "29"));
        }
        private void showPopupPositions()
        {
            for (int i = AU.Length - 1; i >= 0; i--)
                AU[i].ShowPosition();
        }
        #endregion

        #region " Event UDP / LockUnlock"
        public void checkLockUnlock(object sender, SessionSwitchEventArgs e)
        {
            try
            {
                if (e.Reason == SessionSwitchReason.SessionLock)
                {
                    web.Get("SetUDP?status=locked");
                    CT.locked = true;
                }
                else
                {
                    web.Get("SetUDP?status=unlocked");
                    CT.locked = false;
                }
            }
            catch (Exception ex)
            {
                TOOL.errorx("checkLockUnlock error: " + ex.Message);
            }
        }

        // läuft im UDP Thread
        private void udpClient_UDP_receive(object sender, myEventArgs e)
        {
            string text = e.Text;

            TOOL.debugx(0, "udpClient_UDP_receive len=" + text.Length.ToString() + " udp=" + VB.Left(text, 60));

            // Pakete mit genau 60001 Bytes sind Teilstücke einer längeren Nachricht
            if (text.Length == 60001)
            {
                udpToLong = udpToLong + VB.Left(text, 60000);
                return;
            }
            if (udpToLong != "")
            {
                text = udpToLong + text;
                udpToLong = "";
            }

            try
            {
                // Verarbeitung im UI Thread
                uiInvoker.BeginInvoke(new Action<string>(ReceiveUDP), text);
            }
            catch (Exception ex)
            {
                TOOL.errorx("udpClient_UDP_receive error: " + ex.Message);
            }
        }
        #endregion

        #region " Event Timer "
        private void myTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if ((DateTime.Now - lastServerCheck).TotalSeconds >= 10)
                {
                    //Alle 10sek
                    lastServerCheck = DateTime.Now;
                    checkServer();
                    checkRdp();
                }

                checkBlinken();
            }
            catch (Exception ex)
            {
                TOOL.errorx("myTimer_Tick error: " + ex.Message);
            }
        }

        private void checkServer()
        {
            // Prüfung im Hintergrund, damit die Oberfläche bei nicht erreichbarem Server nicht hängt
            if (Interlocked.Exchange(ref serverCheckRunning, 1) == 1)
                return;
            string host;
            int port;
            TOOL.SplitServer(web.Server, web.Ssl == "https" ? 443 : 80, out host, out port);
            ThreadPool.QueueUserWorkItem(_ =>
            {
                bool ok = isServerReachable(host, port);
                try
                {
                    uiInvoker.BeginInvoke(new Action(() =>
                    {
                        setServerState(ok);
                        serverCheckRunning = 0;
                    }));
                }
                catch
                {
                    serverCheckRunning = 0;
                }
            });
        }
        private void setServerState(bool ok)
        {
            if (!ok)
            {
                blinken = false;
                Tray.Icon = Resource1.myCTI2;
            }
            else
            {
                Tray.Icon = Resource1.myCTI;
            }
            red = false;
        }
        private bool isServerReachable(string host, int port)
        {
            // TCP Verbindung zum Webserver statt ICMP Ping (Ping wird oft von Firewalls geblockt)
            try
            {
                using (TcpClient tcp = new TcpClient())
                {
                    IAsyncResult ar = tcp.BeginConnect(host, port, null, null);
                    if (!ar.AsyncWaitHandle.WaitOne(3000))
                        return false;
                    tcp.EndConnect(ar);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
        private void checkRdp()
        {
            string rdpName = GetTerminalServerClientName();
            if (rdpName != "")
                CT.RDPon();
            else
                CT.RDPoff();

            if (RDP != rdpName)
            {
                string erg = web.Get("SetUDP?RDP=" + TOOL.UrlEnc(rdpName));
                if (erg.Length > 0 & erg.Length < 3)
                {
                    RDP = rdpName;
                    TOOL.debugx("myTimer_Tick SetUDP?RDP=" + erg + "  rdpname=" + RDP);
                }
                else
                {
                    TOOL.errorx("myTimer_Tick SetUDP?RDP=" + erg);
                }
            }
        }
        private void checkBlinken()
        {
            if (blinken == true)
            {
                Tray.Icon = red ? Resource1.myCTI : Resource1.myCTI2;
                red = !red;
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
            if (web.WebOK == false)
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
            if (web.WebOK == false)
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
            LocalReg.Save("window_m", state);
            LocalReg.Save("window_x", CT.Location.X.ToString());
            LocalReg.Save("window_y", CT.Location.Y.ToString());
            if (state != "Maximized")
            {
                LocalReg.Save("window_w", CT.Width.ToString());
                LocalReg.Save("window_h", CT.Height.ToString());
            }
        }
        private void mnuAUSetPosition_Click(object sender, System.EventArgs e)
        {
            showPopupPositions();
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
                dial(Clipboard.GetText());
            }
            catch (Exception ex)
            {
                TOOL.errorx("SD_Hotkey error: " + ex.Message);
            }
        }
        private void SD_SpaceHotkey(object sender, EventArgs e)
        {
            for (int i = 0; i < AU.Length; i++)
            {
                AU[i].Visible = false;
                belegt[i] = "";
            }
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
                    web.Get("sendudp?cmd=5&user=" + userx.ToString());
                }
            }
            catch (Exception ex)
            {
                TOOL.errorx("CT_Navigated error: " + ex.Message);
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
                TOOL.errorx("CT_DialNumber error: " + ex.Message);
            }
        }
        private void CT_ReleaseButton(object sender, EventArgs e)
        {
            web.Get("cti.pl?button=" + keycodeRelease);
        }
        private void CT_SpeakerButton(object sender, EventArgs e)
        {
            web.Get("cti.pl?button=21");
        }
        private void CT_SearchNumber(object sender, myEventArgs e)
        {
            web.Get("sendudp?cmd=6&user=" + TOOL.UrlEnc(web.User) + "&text=" + TOOL.UrlEnc(e.Text));
        }
        private void CT_NoValidUser(object sender, myEventArgs e)
        {
            TOOL.errorx("TrayContext CT_NoValidUser user= " + e.Text);
        }
        private void CT_ZeitplanButton(object sender, EventArgs e)
        {
            bool on = getZeitplan();
            on = setZeitplan(!on);
            CT.setZPButton(on);
        }
        private void CT_KampagneChecked(object sender, bool e)
        {
            RegSaveServer("kampagneactive", e ? "1" : "0");
        }
        private void CT_SecondPhoneChecked(object sender, bool e)
        {
            RegSaveServer("secondphoneactive", e ? "1" : "0");
        }
        #endregion

        #region " EVENT FORM AUSGABE "
        // Weiterleiten-Button eines Popups: Text = Ziel, Text2 = Anruf-ID, Text3 = Nebenstelle
        private void AUSGABE_SendeBUTTON(object sender, myEventArgs e)
        {
            string def = e.Text;
            if (!string.IsNullOrEmpty(def))
            {
                if (def.LastIndexOf("I") < 0) def = "I" + def;
                web.Get("cti.pl?deflect=" + CtiProtocol.UrlEncodeDial(def) + "&xid=" + TOOL.UrlEnc(e.Text2) + "&nst=" + TOOL.UrlEnc(e.Text3));
            }
        }
        #endregion

        #region " Sonstige "
        // läuft im UI Thread
        protected void ReceiveUDP(string text)
        {
            try
            {
                if (VB.Left(text, 4) != "CSTA")
                {
                    TOOL.debugx(0, "udpClient_UDP_receive fehler kein CSTA CMD!");
                    return;
                }
                text = VB.Mid(text, 4);
                string cmd = VB.Left(text, 1);
                if (cmd == "5")
                {
                    bool? blink = CtiProtocol.ParseBlink(text);
                    if (blink.HasValue)
                        blinken = blink.Value;
                }
                else if (cmd == "7")
                {
                    if (web.WebOK == false)
                        return;
                    text = convertUTF2HTML(VB.Mid(text, 1));
                    if (VB.Left(text, 1) == "2")
                    {
                        // Abruf für VBA, Ergebnis in die Registry; im Hintergrund, damit die Oberfläche nicht blockiert
                        string url = VB.Mid(text, 1);
                        ThreadPool.QueueUserWorkItem(_ =>
                        {
                            vbaRegResultSave("");
                            vbaRegResultSave(web.Get(url));
                        });
                    }
                    else
                    {
                        CT.OnTop();
                        CT.navigate(text);
                        TOOL.playAudio();
                    }
                }
                else if (cmd == "8")
                {
                    if (Debugger.IsAttached == false)
                    {
                        string startexe = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Documents\\CTI\\myStartGPO.exe";
                        if (File.Exists(startexe))
                            ShellExecute(0, "open", startexe, "", "", 5);
                    }
                }
                else
                {
                    // Anruf Event: gleiches Popup für gleiche Anruf-ID, sonst erstes freies
                    string id = CtiProtocol.ParseCall(text).Id;
                    for (int i = 0; i < AU.Length; i++)
                        if (AU[i].Visible == false)
                            belegt[i] = "";

                    int slot = Array.IndexOf(belegt, id);
                    if (slot < 0)
                        slot = Array.IndexOf(belegt, "");
                    if (slot >= 0)
                    {
                        AU[slot].ausgabe(text);
                        belegt[slot] = id;
                    }
                }
            }
            catch (Exception ex)
            {
                TOOL.errorx("ReceiveUDP error: " + ex.Message);
            }
        }
        private void ShowSettings()
        {
            Settings us = new Settings(userx, web);
            us.ShowDialog();
            loadKeycodeRelease();
            if (SD != null)
                SD.newHotkey();   // geänderte Wähltaste sofort registrieren
            if (web.User != LocalReg.Load("user", "") | web.Password != LocalReg.Load("password", "") | web.Ssl != LocalReg.Load("ssl", "http") | web.Server != LocalReg.Load("server", ""))
            {
                web.User = LocalReg.Load("user", "");
                web.Password = LocalReg.Load("password", "");
                web.Ssl = LocalReg.Load("ssl", "http");
                web.Server = LocalReg.Load("server", "");
                if (udpClient != null)
                    udpClient.setServer(web.Server);
                setUserCredentials();
                userx = VB.Val(web.Get("SetUDP?status=on&version=" + TOOL.UrlEnc(CT.Text)));
                if (web.WebOK == false)
                    userx = -1;
                CT.setNew(web.DefCred, web.Server, web.User, web.Password, web.Ssl, userx.ToString());
            }
        }
        private void dial(string number)
        {
            string num = CtiProtocol.CleanDialNumber(number);
            if (CtiProtocol.IsDialable(num))
                web.Get("cti.pl?dial=" + CtiProtocol.UrlEncodeDial(num));
            CT.textBox1.Text = num;
        }
        private void ShutDown()
        {
            web.Get("SetUDP?status=off");
            CT.destroy();
            for (int i = 0; i < AU.Length; i++)
                AU[i].destroy();
            udpClient.stopsvr();
            Tray.Visible = false;
            System.Environment.Exit(0);
        }

        private void vbaRegOwnExePathSave()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\VB and VBA Program Settings\AB Server\General"))
                {
                    key.SetValue("ownExePath", Application.ExecutablePath);
                }
            }
            catch (Exception ex)
            {
                TOOL.errorx("vbaRegOwnExePathSave error: " + ex.Message);
            }
        }
        private void vbaRegResultSave(string text)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\VB and VBA Program Settings\AB Server\General"))
                {
                    key.SetValue("Result", text);
                }
            }
            catch (Exception ex)
            {
                TOOL.errorx("vbaRegResultSave error: " + ex.Message);
            }
        }

        // serverseitige Einstellungen des aktuellen Benutzers
        private string RegLoadServer(string key)
        {
            return web.GetReg(@"CTI\user" + userx.ToString() + @"\CTI" + key);
        }
        private void RegSaveServer(string key, string wert)
        {
            web.SetReg(@"CTI\user" + userx.ToString() + @"\CTI" + key, wert);
        }
        private string RegLoadLostCall()
        {
            return web.GetReg(@"CTI\User" + userx.ToString() + @"\CTILostCall");
        }
        private string RegLoadStyle()
        {
            return web.GetReg(@"security\user" + userx.ToString() + @"\style").Replace("\n", "").Replace("\r", "");
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
            string url = CtiProtocol.BuildLostCallUrl(RegLoadLostCall());
            if (url != "")
                return url;
            return LocalReg.Load("DoubleClickLink", "");
        }

        private bool getZeitplan()
        {
            return web.Get("cti.pl?zeitplan=get") == "on";
        }
        private bool setZeitplan(bool on)
        {
            return web.Get("cti.pl?zeitplan=" + (on ? "on" : "off")) == "on";
        }

        // Name des RDP Clients, "" wenn lokale Sitzung
        private static string GetTerminalServerClientName()
        {
            const int WTS_CURRENT_SESSION = -1;
            IntPtr buffer = IntPtr.Zero;
            uint bytesReturned;
            try
            {
                if (!WTSQuerySessionInformation(IntPtr.Zero, WTS_CURRENT_SESSION, WTS_INFO_CLASS.WTSClientName, out buffer, out bytesReturned))
                    return "";
                return Marshal.PtrToStringAnsi(buffer) ?? "";
            }
            finally
            {
                if (buffer != IntPtr.Zero)
                    WTSFreeMemory(buffer);
            }
        }
        #endregion
    }
}
