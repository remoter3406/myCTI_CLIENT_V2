using System;
using System.Text;
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
        private CtiServer web;
        #endregion

        #region " Form "
        public Settings(int Xuser, CtiServer web, string company)
        {
            InitializeComponent();

            this.web = web;
            userx = Xuser;
            user = LocalReg.Load("user", "");
            password = LocalReg.Load("password", "");
            ssl = LocalReg.Load("ssl", "http");
            server = LocalReg.Load("server", "");
            keycode = LocalReg.Load("keycodeRelease", "29");
            hotkey = LocalReg.Load("hotkey", "Y");
            cykeyfire = LocalReg.Load("CyKeyfire", "1");
            cymonitor = LocalReg.Load("CyMonitor", "");
            doubleClickLink = LocalReg.Load("DoubleClickLink", "");

            if (VB.Left(company, 3) != "KTC")
            {
                groupBoxVDS.Visible = false;
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
                string styles = web.Get("getstylelist").Replace("\r","");
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
                textBoxZiel1.Text = web.Get("cti.pl?myprofile=1&myziel=get").Replace("\r\n","");
                textBoxZiel2.Text = web.Get("cti.pl?myprofile=2&myziel=get").Replace("\r\n", "");
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

                web.Get("cti.pl?myprofile=1&myziel=" + TOOL.UrlEnc(textBoxZiel1.Text));
                web.Get("cti.pl?myprofile=2&myziel=" + TOOL.UrlEnc(textBoxZiel2.Text));
                RegSaveXtra("kampagneduwa", textBoxKampagne.Text);
                RegSaveXtra("secondphoneduwa", textBoxSecondPhone.Text);

                RegSaveXtra("deflect1", textBoxDeflect1.Text);
                RegSaveXtra("deflect2", textBoxDeflect2.Text);
                RegSaveXtra("deflect3", textBoxDeflect3.Text);
            }
            LocalReg.Save("keycodeRelease", keycodeTrennen.Text);
            LocalReg.Save("hotkey", hotkeyDial.Text.ToUpper());
            LocalReg.Save("DoubleClickLink", textBoxDoubleKlick.Text);

            if (checkBoxVDS.Checked == true)
                LocalReg.Save("CyKeyfire", "1");
            else
                LocalReg.Save("CyKeyfire", "0");

            if (radioButtonVDS1.Checked == true)
                LocalReg.Save("CyMonitor", "0");
            else if (radioButtonVDS2.Checked == true)
                LocalReg.Save("CyMonitor", "1");
            else if (radioButtonVDS3.Checked == true)
                LocalReg.Save("CyMonitor", "2");
            else
                LocalReg.Save("CyMonitor", "");

            LocalReg.Save("server", textBoxServerIP.Text);
            if (radioButtonHttps.Checked == true)
                LocalReg.Save("ssl", "https");
            else
                LocalReg.Save("ssl", "http");

            LocalReg.Save("user", textBoxUser.Text);
            if (textBoxPassword.Text != new string('*', textBoxPassword.Text.Length))
                    LocalReg.Save("password", textBoxPassword.Text);

            Cursor.Current = Cursors.Default;

            this.Close();
        }
        private void btAbbrechen_Click(System.Object sender, System.EventArgs e)
        {
            this.Close();
        }
        #endregion

        #region " Server Registry "
        // Werte werden UTF-8 kodiert gesendet (wie bisher in diesem Dialog)
        public string RegLoadXtra(string key)
        {
            return web.GetReg(@"CTI\user" + userx.ToString() + @"\CTI" + key);
        }
        public void RegSaveXtra(string key, string wert)
        {
            web.SetReg(@"CTI\user" + userx.ToString() + @"\CTI" + key, wert, Encoding.UTF8);
        }
        public string RegLoadStyle()
        {
            return web.GetReg(@"security\user" + userx.ToString() + @"\style").Replace("\n", "").Replace("\r", "");
        }
        public void RegSaveStyle(string wert)
        {
            web.SetReg(@"security\user" + userx.ToString() + @"\style", wert, Encoding.UTF8);
        }
        #endregion
    }
}
