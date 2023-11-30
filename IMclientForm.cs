using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace myCTI2
{
    public partial class IMclientForm : Form
    {
        #region " Storage "
        [DllImport("User32.dll")]
        public static extern long GetForegroundWindow();

        [DllImport("User32.dll")]
        public static extern bool SetForegroundWindow(IntPtr Hwnd);
        
        public event SendeIMUpdateEventHandler SendeIMUpdate;
        public delegate void SendeIMUpdateEventHandler(object sender, EventArgs e);

        //private bool strg;
        private bool shutdown;
        private const int maxuser = 99;
        private string[] status = new string[maxuser + 1];
        private string[] username = new string[maxuser + 1];
        private string[] fullname = new string[maxuser + 1];
        private int[] fullnameuser = new int[maxuser + 1];
        private string[] groupname = new string[maxuser + 1];
        private string[] groups = new string[maxuser + 1];
        private FormWindowState wstate;
        #endregion

        #region " Form "
        public IMclientForm()
        {
            InitializeComponent();
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
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

        public void showWindow()
        {
            TopMost = true;
            this.WindowState = wstate;
            Show();
            TopMost = false;
            SetForegroundWindow(this.Handle);
        }
        public void setIMlist(string text)
        {
            string tmp = "";
            int x;
            richTextBox1.Clear();
            groups = new string[maxuser + 1];
            for (x = 0; x <= maxuser; x++)
            {
                username[x] = "";
                fullname[x] = "";
                status[x] = "";
                fullnameuser[x] = 0;
                groupname[x] = "";
                groups[x] = "";
            }

            try
            {
                do
                {
                    x = text.IndexOf(VB.Chr(13));
                    if (x > 0)
                    {
                        tmp = VB.Left(text, x);
                        text = VB.Mid(text, x + 2);
                    }
                    else
                    {
                        tmp = text;
                        text = "";
                    }
                    // tmp = akt zeile
                    x = tmp.IndexOf("|");
                    if (x < 0)
                        break;
                    int u = VB.Val(VB.Left(tmp, x));
                    fullnameuser[u] = u;
                    tmp = VB.Mid(tmp, x + 1);
                    x = tmp.IndexOf("|");
                    if (x < 0)
                        break;
                    username[u] = VB.Left(tmp, x);
                    tmp = VB.Mid(tmp, x + 1);
                    x = tmp.IndexOf("|");
                    if (x < 0)
                        break;
                    fullname[u] = VB.Left(tmp, x);
                    tmp = VB.Mid(tmp, x + 1);
                    x = tmp.IndexOf("|");
                    if (x < 0)
                        status[u] = tmp;
                    else
                    {
                        status[u] = VB.Left(tmp, x);
                        groupname[u] = VB.Mid(tmp, x + 1);
                        groups[u] = VB.Mid(tmp, x + 1);
                    }
                }
                while (true);
            }
            catch (Exception ex)
            {
                File.AppendAllText(Application.StartupPath + @"\error.txt", DateTime.Now.ToLongTimeString() + " setIMList error: " + ex.Message);
            }

            groups = groups.Distinct().ToArray();
            Array.Sort(groups);

            // sort fullname and fullnameuser
            Array.Sort(fullname, fullnameuser);
            try
            {
                for (int g = 1; g <= groups.Length - 1; g++)
                {
                    richTextBox1.SelectionColor = Color.Cyan;
                    richTextBox1.AppendText("\t" + "\r\n");
                    richTextBox1.AppendText("\t" + groups[g] + "\r\n");

                    for (int f = 1; f <= maxuser; f++)
                    {
                        int u = fullnameuser[f];
                        if (u > 0)
                        {
                            if (status[u] != "")
                            {
                                if (groupname[u] == groups[g])
                                {
                                    try
                                    {
                                        richTextBox1.Select(richTextBox1.TextLength, 0);
                                        if (status[u] == "off")
                                            richTextBox1.SelectionColor = Color.Red;
                                        if (status[u] == "on")
                                            richTextBox1.SelectionColor = Color.Green;
                                        if (status[u] == "unlocked")
                                            richTextBox1.SelectionColor = Color.Green;
                                        if (status[u] == "locked")
                                            richTextBox1.SelectionColor = Color.DarkOrange;
                                        if (status[u] == "rdpOn")
                                            richTextBox1.SelectionColor = Color.DarkSeaGreen;
                                        if (status[u] == "rdpUnlocked")
                                            richTextBox1.SelectionColor = Color.DarkSeaGreen;
                                        if (status[u] == "rdpLocked")
                                            richTextBox1.SelectionColor = Color.Orange;
                                        if (status[u] == "offNoStatus")
                                            richTextBox1.SelectionColor = Color.Black;
                                        if (status[u] == "onNoStatus")
                                            richTextBox1.SelectionColor = Color.Black;
                                        if (status[u] == "unlockedNoStatus")
                                            richTextBox1.SelectionColor = Color.Black;
                                        if (status[u] == "lockedNoStatus")
                                            richTextBox1.SelectionColor = Color.Black;
                                        if (status[u] == "offNoStatusGrp")
                                            richTextBox1.SelectionColor = Color.DarkGray;
                                        if (status[u] == "onNoStatusGrp")
                                            richTextBox1.SelectionColor = Color.DarkGray;
                                        if (status[u] == "unlockedNoStatusGrp")
                                            richTextBox1.SelectionColor = Color.DarkGray;
                                        if (status[u] == "lockedNoStatusGrp")
                                            richTextBox1.SelectionColor = Color.DarkGray;
                                    }
                                    catch (Exception ex)
                                    {
                                        File.AppendAllText(Application.StartupPath + @"\error.txt", DateTime.Now.ToLongTimeString() + " setIMList Colorerror: " + ex.Message);
                                    }
                                    if (fullname[f] != "")
                                        richTextBox1.AppendText(fullname[f] + "\r\n");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(Application.StartupPath + @"\error.txt", DateTime.Now.ToLongTimeString() + " setIMList error: " + ex.Message);
            }
        }
        public void destroy()
        {
            shutdown = true;
            this.Close();
        }
        #endregion

        #region " Form Events "
        private void IMclientForm_Shown(object sender, EventArgs e)
        {
            SendeIMUpdate?.Invoke(this, e);
        }
        private void IMclientForm_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState != FormWindowState.Minimized)
                wstate = this.WindowState;
        }
        #endregion

        #region " Timer "
        private void Timer1_Tick(object sender, EventArgs e)
        {
            SendeIMUpdate?.Invoke(this, e);
        }
        #endregion

    }
}

