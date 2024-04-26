using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using Microsoft.Web.WebView2.Core;
using System.Drawing;
using System.Threading;

namespace myCTI2
{
    public partial class CTIMain : Form
    {
        #region " DLL "
        // The FindWindow API
        [DllImport("User32.dll")]
        public static extern Int32 FindWindow(string lpClassName, string lpWindowName);
        [DllImport("User32.dll")]
        public static extern bool SetForegroundWindow(IntPtr Hwnd);
        #endregion

        #region " Storage "

        public event ShowIMEventHandler ShowIM;
        public delegate void ShowIMEventHandler(object sender, EventArgs e);

        public event DialNumberEventHandler DialNumber;
        public delegate void DialNumberEventHandler(object sender, myEventArgs e);

        public event SpeakerButtonEventHandler SpeakerButton;
        public delegate void SpeakerButtonEventHandler(object sender, EventArgs e);

        public event ReleaseButtonEventHandler ReleaseButton;
        public delegate void ReleaseButtonEventHandler(object sender, EventArgs e);

        public event ZeitplanButtonEventHandler ZeitplanButton;
        public delegate void ZeitplanButtonEventHandler(object sender, EventArgs e);

        public event SearchNumberEventHandler SearchNumber;
        public delegate void SearchNumberEventHandler(object sender,myEventArgs e);

        public event NavigatedEventHandler Navigated;
        public delegate void NavigatedEventHandler(object sender, myEventArgs e);

        public event NoValidUserEventHandler NoValidUser;
        public delegate void NoValidUserEventHandler(object sender, myEventArgs e);

        public event KampagneCheckedEventHandler KampagneChecked;
        public delegate void KampagneCheckedEventHandler(object sender, bool e);

        public event SecondPhoneCheckedEventHandler SecondPhoneChecked;
        public delegate void SecondPhoneCheckedEventHandler(object sender, bool e);

        private FormWindowState wstate;
        private bool NTLM;
        private string user;
        private string password;
        private string server;
        private string ssl;
        private bool shutdown;
        private int tix;
        private string userx;
        private bool zeitplan;
        public bool locked;
        private string kampagne;
        private string sPhone;
        #endregion

        #region " Form "
        public CTIMain()
        {
            InitializeComponent();
            init();
        }

        private void init()
        {
            string appdir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            Directory.SetCurrentDirectory(appdir);
            try
            {
                webView2.CoreWebView2InitializationCompleted += WebView2_CoreWebView2InitializationCompleted;
                webView2.EnsureCoreWebView2Async();
                
            }
            catch (Exception ex)
            {
                TOOL.errorx("CTIMain init error: " + ex.Message );
                TOOL.errorx("appdir=" + appdir);
            }
        }

        private void Form1_Load(System.Object sender, System.EventArgs e)
        {
            try
            {
                navigate("Kontakte.html");
            }
            catch (Exception ex)
            {
                File.AppendAllText(Application.StartupPath + @"\error.txt", DateTime.Now.ToLongTimeString() + " CTI MAIN Form1_Load error:" + ex.Message + "\r");
            }
        }


        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                if (shutdown == true)
                    return;
                if (e.CloseReason != CloseReason.WindowsShutDown)
                {
                    e.Cancel = true;
                    if (this.WindowState != FormWindowState.Minimized)
                        this.Hide();
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(Application.StartupPath + @"\error.txt", DateTime.Now.ToLongTimeString() + " CTI MAIN OnFormClosing error:" + ex.Message + "\r");
            }
        }

        public void setNew(bool CTIntlm, string CTIserver, string CTIuser, string CTIpassword, string CTIssl, string CTIuserX)
        {
            try
            {
                server = CTIserver; user = CTIuser; password = CTIpassword; ssl = CTIssl; userx = CTIuserX; NTLM = CTIntlm;

                /*
                GeckoPreferences.User["network.negotiate-auth.trusted-uris"] = server;
                GeckoPreferences.User["network.negotiate-auth.delegation-uris"] = server;
                GeckoPreferences.User["network.automatic-ntlm-auth.trusted-uris"] = server;

                GeckoPreferences.User["devtools.debugger.remote-enabled"] = true;
                //GeckoPreferences.User["moz:webdriverClick"] = false;
                */
            }
            catch (Exception ex)
            {
                File.AppendAllText(Application.StartupPath + @"\error.txt", DateTime.Now.ToLongTimeString() + " CTI MAIN setNew error:" + ex.Message + "\r");
            }
        }
        public void OnTop()
        {
            try
            {
                this.TopMost = true;
                this.WindowState = wstate;
                this.Show();
                this.TopMost = false;
                SetForegroundWindow(this.Handle);
            }
            catch (Exception ex)
            {
                File.AppendAllText(Application.StartupPath + @"\error.txt", DateTime.Now.ToLongTimeString() + " CTI MAIN OnTop error:" + ex.Message + "\r");
            }
        }
        public void destroy()
        {
            try
            {
                shutdown = true;
                this.Close();
            }
            catch (Exception ex)
            {
                File.AppendAllText(Application.StartupPath + @"\error.txt", DateTime.Now.ToLongTimeString() + " CTI MAIN destroy error:" + ex.Message + "\r");
            }
        }
        public void RDPon()
        {
            labelRDP.Visible = true;
        }
        public void RDPoff()
        {
            labelRDP.Visible = false;
        }
        #endregion

        #region " Public "
        public void navigate(string url, bool newWindow = false)
        {
             try
            {
                if (VB.Val(userx) == 0)
                {
                    webView2.CoreWebView2.NavigateToString("<h1>Keine Anmeldung mit myCTI Benutzer '" + user + "' erfolgt</h1><BR>Ggf. nochmal Kontakte/Monitor aufrufen oder Username/Password ist falsch oder Probleme mit dem myCTI Server liegen vor");
                    NoValidUser?.Invoke(this, new myEventArgs(url));
                }
                else
                {
                    url = ssl + "://" + server + "/" + url;
                    if (newWindow == false)
                    {
                        TOOL.debugx("navigate2 url=" + url);
                        Application.DoEvents();
                        if (webView2 != null && webView2.CoreWebView2 != null)
                        {
                            webView2.CoreWebView2.Navigate(url);
                        }
                    }
                    else
                    {
                        Microsoft.Web.WebView2.WinForms.WebView2 browser = new Microsoft.Web.WebView2.WinForms.WebView2();
                        Form f = new Form();
                        f.Icon = this.Icon;
                        f.Size = new Size(1500, 900);
                        f.Controls.Add(browser);
                        f.Controls[0].Size = new Size(1484, 900);
                        browser.CoreWebView2.Navigate(url);
                        f.Show();
                    }
                }
            }
            catch (Exception ex)
            {
                TOOL.errorx("Fehler CT.Navigate user=" + user + " url=" + url + "\r\n" + ex.Message);
            }
        }
        public void setZPButton(bool onOff)
        {
            zeitplan = onOff;
            if (zeitplan)
            {
                buttonZeitplan.Image = Resource1.clockon;
                buttonZeitplan.Refresh();
            }
            else
            {
                buttonZeitplan.Image = Resource1.clockoff;
                buttonZeitplan.Refresh();
            }
        }
        public void setKampagneButton(string kampagneDuwa)
        {
            kampagne = kampagneDuwa;
            if (kampagne !="")
            {
                checkBoxKampagne.Visible=true;
                checkBoxKampagne.Text="Kampagne " + kampagneDuwa;
                checkBoxKampagne.Refresh();
            }
            else
            {
                checkBoxKampagne.Visible = false;
                checkBoxKampagne.Refresh();
            }
        }
        public void setSecondPhoneButton(string secondPhoneDuwa)
        {
            sPhone = secondPhoneDuwa;
            if (sPhone != "")
            {
                checkBoxSecondPhone.Visible = true;
                checkBoxSecondPhone.Text = "Wähle mit " + sPhone;
                checkBoxSecondPhone.Refresh();
            }
            else
            {
                checkBoxSecondPhone.Visible = false;
                checkBoxSecondPhone.Refresh();
            }
        }

        #endregion


        #region " Form Events "
        private void CTIMain_SizeChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.WindowState != FormWindowState.Minimized)
                    wstate = this.WindowState;
            }
            catch (Exception ex)
            {
                File.AppendAllText(Application.StartupPath + @"\error.txt", DateTime.Now.ToLongTimeString() + " CTI MAIN CTIMain_SizeChanged error:" + ex.Message + "\r");
            }
        }
        private void CTIMain_KeyDown(object sender, KeyEventArgs e)
        {
            Debug.Print("key=" + e.KeyCode.ToString());
        }

        private void textBox1_KeyPress_1(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                DialNumber?.Invoke(this, new myEventArgs(textBox1.Text));
                textBox1.Text = "";
            }
        }
        private void buttonShowNumber_Click(object sender, EventArgs e)
        {
            // Nr anzeigen
            DialNumber?.Invoke(this, new myEventArgs("#86"));
        }
        private void buttonHideNumber_Click(object sender, EventArgs e)
        {
            // Nr. unterdrücken
            DialNumber?.Invoke(this, new myEventArgs("*86"));
        }
        private void buttonHome_Click(object sender, EventArgs e)
        {
            navigate("");
        }
        private void buttonBack_Click(object sender, EventArgs e)
        {
            // zurück
           webView2.GoBack();
        }
        private void buttonForward_Click(object sender, EventArgs e)
        {
            // vor
          webView2.GoForward();
        }
        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            // refresh
           webView2.Reload();
        }
        private void buttonSpeaker_Click(object sender, EventArgs e)
        {
            SpeakerButton?.Invoke(this, e);
        }
        private void buttonHookOn_Click(object sender, EventArgs e)
        {
            ReleaseButton?.Invoke(this, e);
        }
        private void buttonIM_Click(object sender, EventArgs e)
        {
            ShowIM?.Invoke(this, e);
        }
        private void buttonSearch_Click(object sender, EventArgs e)
        {
            InputBox nummer = new InputBox("Rufnummer eingeben", "Namenssuche", "", false);
            nummer.ShowDialog();
            if (nummer.DialogResult == DialogResult.OK)
                SearchNumber?.Invoke(this, new myEventArgs(nummer.Prompt));
        }
        private void buttonHelp_Click(object sender, EventArgs e)
        {
            navigate("autoupdate/help.htm", true);
        }
        private void buttonZeitplan_Click(object sender, EventArgs e)
        {
            ZeitplanButton?.Invoke(this, e);
            navigate("TIMES.PL?cti=1&plan=" + userx + "&von=&bis=&profil=");
        }
        private void checkBoxKampagne_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxKampagne.Checked)
                KampagneChecked?.Invoke(this, true);
            else
                KampagneChecked?.Invoke(this, false);
        }
        private void checkBoxSecondPhone_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxSecondPhone.Checked)
                SecondPhoneChecked?.Invoke(this, true);
            else
                SecondPhoneChecked?.Invoke(this, false);
        }
        #endregion

        #region " CoreWebView2 "
        private void WebView2_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            webView2.CoreWebView2.ServerCertificateErrorDetected += CoreWebView2_ServerCertificateErrorDetected;
            webView2.CoreWebView2.ContainsFullScreenElementChanged += CoreWebView2_ContainsFullScreenElementChanged;
            webView2.CoreWebView2.BasicAuthenticationRequested += CoreWebView2_BasicAuthenticationRequested;
            webView2.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;
            webView2.CoreWebView2.NavigationStarting += CoreWebView2_NavigationStarting;
            webView2.CoreWebView2.ClientCertificateRequested += CoreWebView2_ClientCertificateRequested;
            webView2.CoreWebView2.PermissionRequested += CoreWebView2_PermissionRequested;

        }
        private void CoreWebView2_ServerCertificateErrorDetected(object sender, CoreWebView2ServerCertificateErrorDetectedEventArgs e)
        {
            CoreWebView2Certificate certificate = e.ServerCertificate;
            e.Action = CoreWebView2ServerCertificateErrorAction.AlwaysAllow;
        }
 
        private void CoreWebView2_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            try
            {
                if (e.HttpStatusCode == 0) //navigate to html String
                {
                    Debug.Print("Navigation html String:" + e.HttpStatusCode.ToString());
                }
                else if (e.HttpStatusCode == 401) 
                {
                    Debug.Print("NavigationError:" + e.HttpStatusCode.ToString());
                    webView2.CoreWebView2.NavigateToString("<h1>Http Fehler '" + e.HttpStatusCode.ToString() + "'</h1><BR>Username/Password sind falsch oder ggf. Servername statt IP Adresse eintragen bei Single Sign On (SSO mit Windows Anmeldung) ");

                }
                else if (e.HttpStatusCode != 200)
                {
                    Debug.Print("NavigationError:" + e.HttpStatusCode.ToString());
                    webView2.CoreWebView2.NavigateToString("<h1>Http Fehler '" + e.HttpStatusCode.ToString() + "'</h1>");
                }
                else
                {
                    Debug.Print("Navigated:" + webView2.Source.ToString());
                    Navigated?.Invoke(this, new myEventArgs(webView2.Source.ToString()));
                }
                
            }
            catch (Exception ex)
            {
                File.AppendAllText(Application.StartupPath + @"\error.txt", DateTime.Now.ToLongTimeString() + " CTI MAIN GeckoWebBrowser1_Navigated error:" + ex.Message + "\r");
            }
        }

        private void CoreWebView2_BasicAuthenticationRequested(object sender, CoreWebView2BasicAuthenticationRequestedEventArgs e)
        {
            // wird nicht bei SSO aufgerufen, kann also immer ausgefüllt werden
            if (user != "")
            {
                e.Response.UserName = user;
                e.Response.Password = password;
            }
        }
        private void CoreWebView2_PermissionRequested(object sender, CoreWebView2PermissionRequestedEventArgs e)
        {
        }
        private void CoreWebView2_ClientCertificateRequested(object sender, CoreWebView2ClientCertificateRequestedEventArgs e)
        {
        }
        private void CoreWebView2_ContainsFullScreenElementChanged(object sender, object e)
        {
        }
        private void CoreWebView2_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
        }


        #endregion

        #region " Timer "
        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {   if (locked) tix = 89; //nach sperre sofort aktiv
                if (!locked) tix = tix + 1; //bei sperre nicht 
                if (tix > 90)
                {
                    tix = 1;
                    if (this.WindowState == FormWindowState.Maximized | this.WindowState == FormWindowState.Normal)
                    {
                        string url = webView2.Source.ToString();
                        if (VB.Left(url.ToUpper(), 11) == "/MONITOR.PL")
                            webView2.Reload();
                    }
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(Application.StartupPath + @"\error.txt", DateTime.Now.ToLongTimeString() + " CTI MAIN Timer1_Tick error:" + ex.Message + "\r");
            }
        }



        #endregion


    }


}
