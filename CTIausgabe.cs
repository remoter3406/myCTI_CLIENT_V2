using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace myCTI2
{
    public partial class CTIausgabe : Form
    {
        #region " Storage "
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        [DllImport("User32.dll")]

        public static extern bool SetForegroundWindow(IntPtr Hwnd);
        public event SendeBUTTONEventHandler SendeBUTTON;
        public delegate void SendeBUTTONEventHandler(object sender, myEventArgs e);

        delegate void TextDelegate(string text);
        private bool shutdown;
        private string cmd;
        private int windowNum;
        private bool saveLoc;
        private string def1;
        private string def2;
        private string def3;
        private string xziel;
        private string xid;

        #endregion

        #region " Form "
        public CTIausgabe(int num,string def1, string def2, string def3)
        {
            InitializeComponent();
            windowNum= num;
            this.Text = "Anruf " + windowNum.ToString();
            this.def1 = def1;
            this.def2 = def2;
            this.def3 = def3;
            button1.Text = "Weiterleiten zu " + def1;
            button2.Text = "Weiterleiten zu " + def2;
            button3.Text = "Weiterleiten zu " + def3;
            if (!string.IsNullOrEmpty(def1)) button1.Visible = true; else button1.Visible = false;
            if (!string.IsNullOrEmpty(def2)) button2.Visible = true; else button2.Visible = false;
            if (!string.IsNullOrEmpty(def3)) button3.Visible = true; else button3.Visible = false;

        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            timerRing.Enabled = false;
            if (shutdown == true)
                return;
            if (e.CloseReason != CloseReason.WindowsShutDown)
            {
                e.Cancel = true;
                this.Hide();
            }
        }
        private void CTIausgabe_VisibleChanged(object sender, EventArgs e)
        {
            timerRing.Enabled = false;
        }
        private void CTIausgabe_Load(object sender, EventArgs e)
        {
            if (cmd == "6")
                this.Hide();
        }
 
        #endregion

        #region " Public "
        public void destroy()
        {
            shutdown = true;
            this.Close();
        }
        public void ausgabe(string text)
        {
            if (this.InvokeRequired)
            {
                Application.DoEvents();
                this.Invoke(new Action( () => { this.ausgabe(text); } ));
                return;
            }
            TOOL.debugx("Ausgabe: " + VB.Left(text,60));
            CallRecord call = CtiProtocol.ParseCall(text);
            cmd = call.Cmd;
            xziel = call.Ziel;
            xid = call.Id;
            timer1.Enabled = false;
            if (cmd == "1" | cmd == "2" | cmd == "6")
            {
                if (cmd == "1")
                {
                    timer1.Interval = 90000; //max 90sek bei fehlern
                    timer1.Enabled = true;
                }
                if (cmd == "2")
                {
                    timer1.Interval = 15000; //max 15sek
                    timer1.Enabled = true;
                }
                anrufer.Text = call.Anrufer;
                udpSend31311(cmd + call.Anrufer);
                ziel.Text = call.Ziel;

                Kampagne.Text = call.Kampagne;
                Kampagne.Visible = call.Kampagne != "";

                bool umgeleitet = call.For != call.Ziel;
                ursprung.Text = umgeleitet ? call.For : "";
                ursprung.Visible = umgeleitet;
                labelUrsprung.Visible = umgeleitet;

                anrufer.Visible = call.Anrufer != "";
                label1.Visible = call.Anrufer != "";
                ziel.Visible = call.Ziel != "";
                label2.Visible = call.Ziel != "";

                if (call.Bild != "")
                {
                    try
                    {
                        zeigeImage(new Bitmap(GenerateStreamFromString(call.Bild)));
                    }
                    catch
                    {
                        TOOL.debugx("Ausgabe: Imagefehler len=" + call.Bild.Length.ToString());
                        zeigeImage(Resource1.smilie1);
                    }
                }
                else
                    zeigeImage(Resource1.smilie1);


                TopMost = true;
                IntPtr hwn = GetForegroundWindow();
                Show();
                SetForegroundWindow(hwn);
                timerTopMost.Enabled = true;

                TOOL.playRingAudio(true);
                timerRing.Enabled = true;
                TOOL.debugx("Ausgabe: ende");
            }
            else
            {
                Hide();
                if (call.FieldCount >= 3)
                    udpSend31311(cmd + call.Anrufer);
            }
            Application.DoEvents();
        }
        public void ShowPosition()
        {
            if (this.InvokeRequired)
            {
                Application.DoEvents();
                this.Invoke(new Action( () => ShowPosition()));
                return;
            }
            anrufer.Text = "ABC 11 - Einrichtung SharePoint (Student) Sebastian Kleinfelder";
            ziel.Text = "mir oder was";

            zeigeImage(Resource1.smilie2);

            FormBorderStyle = FormBorderStyle.FixedSingle;
            saveLoc = true;
            this.Show();
            Application.DoEvents();
        }
        #endregion

        #region " Form Click "
        private void CTIausgabe_Click(object sender, EventArgs e)
        {
            klickClose();
        }
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            klickClose();
        }
        private void anrufer_Click(object sender, EventArgs e)
        {
            klickClose();
        }
        private void ursprung_Click(object sender, EventArgs e)
        {
            klickClose();
        }
        private void ziel_Click(object sender, EventArgs e)
        {
            klickClose();
        }
        private void label1_Click(object sender, EventArgs e)
        {
            klickClose();
        }
        private void labelUrsprung_Click(object sender, EventArgs e)
        {
            klickClose();
        }
        private void label2_Click(object sender, EventArgs e)
        {
            klickClose();
        }
        private void klickClose()
        {
            TOOL.debugx("klickClose saveloc=" + saveLoc.ToString() + " windownum=" + windowNum.ToString());

            if (saveLoc)
            {
                saveLoc= false;
                try
                {
                    LocalReg.Save("udp_window_x" + windowNum.ToString(), this.Location.X.ToString());
                    LocalReg.Save("udp_window_y" + windowNum.ToString(), this.Location.Y.ToString());
                }
                catch (Exception ex)
                {
                    TOOL.errorx("CTIausgabe position speichern error: " + ex.Message);
                }
            }
            this.Hide();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            SendeBUTTON?.Invoke(this, new myEventArgs(def1,xid,xziel));
        }
        private void button2_Click(object sender, EventArgs e)
        {
            SendeBUTTON?.Invoke(this, new myEventArgs(def2,xid, xziel));
        }
        private void button3_Click(object sender, EventArgs e)
        {
            SendeBUTTON?.Invoke(this, new myEventArgs(def3, xid, xziel));
        }
        #endregion

        #region " Private "
        private void udpSend31311(string text)
        {
            try
            {
                using (UdpClient udpClient = new UdpClient())
                {
                    udpClient.Connect(IPAddress.Loopback, 31311);
                    byte[] bytCommand = TOOL.Ansi.GetBytes(text);
                    udpClient.Send(bytCommand, bytCommand.Length);
                }
            }
            catch
            {
            }
        }

        private void zeigeImage(Image bild)
        {
            Size imageSize = bild.Size;
            double aspectRatio = imageSize.Height / (double)imageSize.Width;
            if (imageSize.Width == imageSize.Height)
            {
                pictureBox1.Width = 200;
                pictureBox1.Height = 200;
            }
            else if (imageSize.Width < imageSize.Height)
            {
                pictureBox1.Width = Convert.ToInt32(Math.Round(pictureBox1.Height / aspectRatio));
                pictureBox1.Height = 200;
            }
            else if (imageSize.Width > imageSize.Height)
            {
                pictureBox1.Width = 200;
                pictureBox1.Height = Convert.ToInt32(Math.Round(pictureBox1.Width * aspectRatio));
            }
            pictureBox1.Image = bild;
        }
        private Stream GenerateStreamFromString(string s)
        {
            return new MemoryStream(TOOL.Ansi.GetBytes(s));
        }
        #endregion

        #region " Timer "
        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            ausgabe("");
        }
        private void timerRing_Tick(object sender, EventArgs e)
        {
            TOOL.playRingAudio(true);
        }
        private void timerTopMost_Tick(object sender, EventArgs e)
        {
            TopMost = false;
            timerTopMost.Enabled = false;
        }


        #endregion

 
    }
}
