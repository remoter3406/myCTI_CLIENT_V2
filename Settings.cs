using Microsoft.Win32;
using System;
using System.IO;
using System.Net;
using System.Web;
using System.Windows.Forms;

namespace myCTI2
{
    public partial class Settings : Form
    {
        #region " Storage "
        private int userx;
        private string server;
        private string user;
        private string password;
        private string ssl;
        private string keycode;
        private string hotkey;
        private string cykeyfire;
        private string cymonitor;
        private string doubleClickLink;
        private bool isNTLM;
        #endregion

        #region " Form "
        public Settings(int Xuser, bool NTLM, string company)
        {
            InitializeComponent();

            isNTLM = NTLM;
            userx = Xuser;
            user = RegLoad("user", "");
            password = RegLoad("password", "");
            ssl = RegLoad("ssl", "http");
            server = RegLoad("server", "");
            keycode = RegLoad("keycodeRelease", "29");
            hotkey = RegLoad("hotkey", "Y");
            cykeyfire = RegLoad("CyKeyfire", "1");
            cymonitor = RegLoad("CyMonitor", "");
            doubleClickLink = RegLoad("DoubleClickLink", "");

            if (VB.Left(company, 3) != "KTC")
            {
                groupBoxVDS.Visible = false;
            }
            if (isNTLM)
            {
                textBoxUser.Visible = false;
                textBoxPassword.Visible = false;
                labelUser.Visible = false;
                labelPassword.Visible = false;
            }

            textBoxServerIP.Text = server;
            textBoxUser.Text = user;
            textBoxPassword.Text = new string('*', password.Length);
            if (ssl == "https")
                radioButtonHttps.Checked = true;
            else
                radioButtonHttp.Checked = true;

            keycodeTrennen.Text = keycode;
            hotkeyDial.Text = hotkey;
            textBoxDoubleKlick.Text = doubleClickLink;

            if (cykeyfire == "1")
                checkBoxVDS.Checked = true;
            else
                checkBoxVDS.Checked = false;

            if (cymonitor == "0")
                radioButtonVDS1.Checked = true;
            else if (cymonitor == "1")
                radioButtonVDS2.Checked = true;
            else if (cymonitor == "2")
                radioButtonVDS3.Checked = true;
            else
                radioButtonVDSaus.Checked = true;

            if (userx == 0)
            {
                groupBoxServer.Visible = false;
                labelErrorUser.Visible = true;
            }
            else if (userx == -1)
            {
                groupBoxServer.Visible = false;
                labelError.Visible = true;
            }
            else
            {
                string labtext = ssl + "://" + server + "/";
                Label2.Text = labtext;
                textBox1.Text = RegLoadXtra("text1");
                textBox2.Text = RegLoadXtra("url1");
                textBox3.Text = RegLoadXtra("text2");
                textBox4.Text = RegLoadXtra("url2");
                textBox5.Text = RegLoadXtra("text3");
                textBox6.Text = RegLoadXtra("url3");
                textBox7.Text = RegLoadXtra("text4");
                textBox8.Text = RegLoadXtra("url4");
                textBox9.Text = RegLoadXtra("text5");
                textBox10.Text = RegLoadXtra("url5");
                textBox11.Text = RegLoadXtra("text6");
                textBox12.Text = RegLoadXtra("url6");
                string styles = getWeb("getstylelist").Replace("\r","");
                string style = RegLoadStyle();
                if (style == "") style = "Default";
                string[] sx = styles.Split('\n');
                for (int x = 0; x < sx.Length; x++)
                {
                    string tmp = sx[x].Replace(".css","");
                    if (tmp.Length > 5) tmp = tmp.Substring(5);
                    else if (tmp.Length == 5) tmp = "Default";
                    if (tmp != "") comboBoxStyle.Items.Add(tmp);
                }
                comboBoxStyle.Text = style;
                textBoxZiel1.Text = getWeb("cti.pl?myprofile=1&myziel=get").Replace("\r\n","");
                textBoxZiel2.Text = getWeb("cti.pl?myprofile=2&myziel=get").Replace("\r\n", "");
                textBoxKampagne.Text = RegLoadXtra("kampagneduwa");
                textBoxSecondPhone.Text = RegLoadXtra("secondphoneduwa");
                textBoxMailAddress.Text = RegLoadXtra("EmailAdr");
                if (RegLoadXtra("EmailSend") == "1")
                    checkBoxMailActive.Checked = true;
                else
                    checkBoxMailActive.Checked = false;

                textBoxDeflect1.Text = RegLoadXtra("deflect1");
                textBoxDeflect2.Text = RegLoadXtra("deflect2");
                textBoxDeflect3.Text = RegLoadXtra("deflect3");
            }
        }

        private void btOK_Click(System.Object sender, System.EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            if (userx > 0)
            {
                RegSaveXtra("text1", textBox1.Text);
                RegSaveXtra("url1", textBox2.Text);
                RegSaveXtra("text2", textBox3.Text);
                RegSaveXtra("url2", textBox4.Text);
                RegSaveXtra("text3", textBox5.Text);
                RegSaveXtra("url3", textBox6.Text);
                RegSaveXtra("text4", textBox7.Text);
                RegSaveXtra("url4", textBox8.Text);
                RegSaveXtra("text5", textBox9.Text);
                RegSaveXtra("url5", textBox10.Text);
                RegSaveXtra("text6", textBox11.Text);
                RegSaveXtra("url6", textBox12.Text);
                RegSaveXtra("EmailAdr", textBoxMailAddress.Text);
                if (checkBoxMailActive.Checked)
                    RegSaveXtra("EmailSend", "1");
                else
                    RegSaveXtra("EmailSend", "0");
                /*
                if (checkBoxExternalCallsToDB.Checked == true)
                    RegSaveXtra("OnlyExternalCallsToDB", "1");
                else
                    RegSaveXtra("OnlyExternalCallsToDB", "0");
                */
                string style = comboBoxStyle.Text.Replace("Default","");
                RegSaveStyle(style);

                getWeb("cti.pl?myprofile=1&myziel=" + textBoxZiel1.Text);
                getWeb("cti.pl?myprofile=2&myziel=" + textBoxZiel2.Text);
                RegSaveXtra("kampagneduwa", textBoxKampagne.Text);
                RegSaveXtra("secondphoneduwa", textBoxSecondPhone.Text);

                RegSaveXtra("deflect1", textBoxDeflect1.Text);
                RegSaveXtra("deflect2", textBoxDeflect2.Text);
                RegSaveXtra("deflect3", textBoxDeflect3.Text);
            }
            RegSave("keycodeRelease", keycodeTrennen.Text);
            RegSave("hotkey", hotkeyDial.Text.ToUpper());
            RegSave("DoubleClickLink", textBoxDoubleKlick.Text);

            if (checkBoxVDS.Checked == true)
                RegSave("CyKeyfire", "1");
            else
                RegSave("CyKeyfire", "0");

            if (radioButtonVDS1.Checked == true)
                RegSave("CyMonitor", "0");
            else if (radioButtonVDS2.Checked == true)
                RegSave("CyMonitor", "1");
            else if (radioButtonVDS3.Checked == true)
                RegSave("CyMonitor", "2");
            else
                RegSave("CyMonitor", "");

            RegSave("server", textBoxServerIP.Text);
            if (radioButtonHttps.Checked == true)
                RegSave("ssl", "https");
            else
                RegSave("ssl", "http");

            if (!isNTLM)
            {
                RegSave("user", textBoxUser.Text);
                if (textBoxPassword.Text != new string('*', textBoxPassword.Text.Length))
                    RegSave("password", textBoxPassword.Text);
            }
            Cursor.Current = Cursors.Default;

            this.Close();
        }
        private void btAbbrechen_Click(System.Object sender, System.EventArgs e)
        {
            this.Close();
        }
        #endregion

        #region " WebClient "
        private string getWeb(string url, bool noErrFile = false)
        {
            // debugx(1, "getWeb url=" + url)
            ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AcceptCert);
            HttpWebRequest request;
            CredentialCache mycache = new CredentialCache();
            url = ssl + "://" + server + "/" + url;
            request = (HttpWebRequest)WebRequest.Create(url);
            if (isNTLM == true)
                request.Credentials = CredentialCache.DefaultNetworkCredentials;
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
            }
            // webOK = True
            catch (Exception ex)
            {
                if (ex.Message.IndexOf("Timeout") >= 0)
                {
                }
                if (noErrFile == false)
                    File.AppendAllText(Application.StartupPath + @"\error.txt", DateTime.Now.ToLongTimeString() + " GetURL error:" + ex.Message + "\r");
            }
            // debugx(2, "getWeb response=" + responseFromServer)

            return responseFromServer;
        }
        private bool AcceptCert(object sender, System.Security.Cryptography.X509Certificates.X509Certificate cert, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors errors)
        {
            return true;
        }
        #endregion

        #region " Registry "
        private void RegSave(string var, string para)
        {
            RegistryKey MeinKey;
            MeinKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\AB Server", true);
            MeinKey.SetValue(var, para);
            MeinKey.Close();
        }
        private string RegLoad(string var, string defaultx = "")
        {
            RegistryKey MeinKey;
            MeinKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\AB Server", true);
            if (MeinKey == null)
                MeinKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\AB Server");
            return MeinKey.GetValue(var, defaultx).ToString();
        }
        public string RegLoadXtra(string key)
        {
            string var = @"CTI\" + "user" + userx.ToString() + @"\CTI" + key;
            string tmp = getWeb("action?cmd=getreg&var=" + HttpUtility.UrlEncode(var));
            if (VB.Right(tmp, 2) == "\r\n")
                tmp = VB.Left(tmp, tmp.Length - 2);
            return tmp;
        }
        public void RegSaveXtra(string key, string wert)
        {
            string var = @"CTI\" + "user" + userx.ToString() + @"\CTI" + key;
            getWeb("action?cmd=setreg&var=" + HttpUtility.UrlEncode(var) + "&wert=" + HttpUtility.UrlEncode(wert));
        }
        public string RegLoadStyle()
        {
            string var = "security\\" + "user" + userx.ToString() + "\\style";
            string tmp = getWeb("action?cmd=getreg&var=" + HttpUtility.UrlEncode(var)).Replace("\n","").Replace("\r", "");
            return tmp;
        }
        public void RegSaveStyle(string wert)
        {
            string var = "security\\" + "user" + userx.ToString() + "\\style";
            getWeb("action?cmd=setreg&var=" + HttpUtility.UrlEncode(var) + "&wert=" + HttpUtility.UrlEncode(wert));
        }
        #endregion
    }
}