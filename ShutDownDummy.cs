using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;
using Microsoft.Win32;
using System.Drawing;

namespace myCTI2
{
    public partial class ShutDownDummy : Form
    {
        #region " Storage "
        public event WindowsShutDownEventHandler WindowsShutDown;
        public delegate void WindowsShutDownEventHandler(object sender, EventArgs e);
        public event DialHotkeyEventHandler DialHotkey;
        public delegate void DialHotkeyEventHandler(object sender, EventArgs e);
        public event SpaceHotkeyEventHandler SpaceHotkey;
        public delegate void SpaceHotkeyEventHandler(object sender, EventArgs e);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, ShowWindowEnum flags);

        private enum ShowWindowEnum
        {
            Hide = 0,
            ShowNormal = 1,
            ShowMinimized = 2,
            ShowMaximized = 3,
            Maximize = 3,
            ShowNormalNoActivate = 4,
            Show = 5,
            Minimize = 6,
            ShowMinNoActivate = 7,
            ShowNoActivate = 8,
            Restore = 9,
            ShowDefault = 10,
            ForceMinimized = 11
        }
        private struct POINTAPI
        {
            public int x;
            public int y;
        }
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        private struct WINDOWPLACEMENT
        {
            public int Length;
            public int flags;
            public int showCmd;
            public POINTAPI ptMinPosition;
            public POINTAPI ptMaxPosition;
            public RECT rcNormalPosition;
        }
        const int WM_KEYDOWN = 0x100;

        [System.Runtime.InteropServices.DllImport("user32")]
        private static extern IntPtr GetWindow(IntPtr hwnd, int wCmd);
        [System.Runtime.InteropServices.DllImport("user32")]
        private static extern int GetWindowPlacement(IntPtr hwnd, ref WINDOWPLACEMENT lpwndpl);
        [System.Runtime.InteropServices.DllImport("user32")]
        private static extern int GetWindowRect(IntPtr hwnd, ref RECT lpRect);
        [System.Runtime.InteropServices.DllImport("user32")]
        private static extern int GetWindowText(IntPtr hwnd, System.Text.StringBuilder lpString, int cch);
        [System.Runtime.InteropServices.DllImport("user32")]
        private static extern int SetWindowPos(IntPtr hwnd, int hWndInsertAfter, int x, int y, int cx, int cy, int wFlags);
        [System.Runtime.InteropServices.DllImport("user32")]
        private static extern bool BringWindowToTop(IntPtr HWnd);
        [System.Runtime.InteropServices.DllImport("user32")]
        private static extern IntPtr GetForegroundWindow();
        [System.Runtime.InteropServices.DllImport("user32")]
        private static extern IntPtr SetForegroundWindow(IntPtr hwnd);
        [System.Runtime.InteropServices.DllImport("user32")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int wMsg, int wParam, uint lParam);
        [System.Runtime.InteropServices.DllImport("user32")]
        private static extern IntPtr PostMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32")]
        private static extern int RegisterHotKey(IntPtr hWnd, int id, uint fsModifier, uint vk);
        [System.Runtime.InteropServices.DllImport("user32")]
        private static extern void UnregisterHotKey(IntPtr hWnd, int id);
        [System.Runtime.InteropServices.DllImport("user32")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, IntPtr dwExtraInfo);

        private bool startflag;
        private const int Key_NONE = 0x0;
        private const int WM_HOTKEY = 0x0312;
        private const int Key_ALT = 0x1;
        private const int Key_STRG = 0x2;
        private const int Key_SHIFT = 0x4;
        private const int Key_WIN = 0x8;

        private static int startCount;
        private static int count;
        #endregion

        #region " Form "
        public ShutDownDummy()
        {
            InitializeComponent();
        }
        private void ShutDownDummy_Load(object sender, EventArgs e)
        {
            newHotkey();
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            Debug.Print(DateTime.Now.ToLongTimeString() + " shutdown dummy OnFormClosing ");
            if (e.CloseReason == CloseReason.WindowsShutDown)
            {
                UnregisterHotKey(this.Handle, 1);
                WindowsShutDown?.Invoke(this, e);
            }
            else
                e.Cancel = true;
        }

        private void newHotkey()
        {
            string shortcut = RegLoad("hotkey", "Y");
            try
            {
                UnregisterHotKey(this.Handle, 1);
            }
            catch
            {
            }
            try
            {
                RegisterHotKey(this.Handle, 1, Key_STRG, (uint)Keys.D1);
            }
            catch
            {
            }
            try
            {
                UnregisterHotKey(this.Handle, 2);
            }
            catch
            {
            }
            try
            {
                RegisterHotKey(this.Handle, 2, Key_STRG, (uint)Keys.D2);
            }
            catch
            {
            }
            try
            {
                UnregisterHotKey(this.Handle, 3);
            }
            catch
            {
            }
            try
            {
                RegisterHotKey(this.Handle, 3, Key_STRG, (uint)Keys.D3);
            }
            catch
            {
            }
            try
            {
                UnregisterHotKey(this.Handle, 4);
            }
            catch
            {
            }
            try
            {
                RegisterHotKey(this.Handle, 4, Key_STRG, (uint)VB.Asc(shortcut));
            }
            catch
            {
            }
            try
            {
                UnregisterHotKey(this.Handle, 5);
            }
            catch
            {
            }
            try
            {
                RegisterHotKey(this.Handle, 5, Key_STRG, (uint)VB.Asc(" "));
            }
            catch
            {
            }
        }
        #endregion

        #region " Window Messages"
        protected override void WndProc(ref Message m)
        {
            try
            {
                switch (m.Msg)
                {
                    case WM_HOTKEY:
                        {
                            switch ((int)m.WParam)
                            {
                                case 1:
                                    {
                                        SetVDS("0");
                                        break;
                                    }

                                case 2:
                                    {
                                        SetVDS("1");
                                        break;
                                    }

                                case 3:
                                    {
                                        SetVDS("99");
                                        break;
                                    }

                                case 4:
                                    {
                                        DialHotkey?.Invoke(this, EventArgs.Empty);
                                        break;
                                    }
                                case 5:
                                    {
                                        SpaceHotkey?.Invoke(this, EventArgs.Empty);
                                        break;
                                    }
                            }

                            break;
                        }
                }
                base.WndProc(ref m);
            }
            catch (Exception ex)
            {
                Debug.Print(DateTime.Now.ToLongTimeString() + " shutdown dummy WndProc =" + ex.Message);
                dbg(ex.Message);
            }
        }
        #endregion

        #region " Timer "
        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                Process bProcess = Process.GetProcessesByName("wfica32").FirstOrDefault();
                if (bProcess == null)
                {
                    count = 0;
                    startCount = 0;
                    return;
                }
                IntPtr handle = bProcess.MainWindowHandle;
                if (handle == (IntPtr)0)
                    return;
                IntPtr child = GetWindow(handle, 5);
                IntPtr foreG = GetForegroundWindow();
                int show = getShowCmd(handle);

                System.Text.StringBuilder Caption = new System.Text.StringBuilder(256);
                GetWindowText(child, Caption, Caption.Capacity);
                string childname = Caption.ToString();
 
                if (startflag == false)
                {
                    startCount = 4; // startflag ist nur beim alleresten Timeraufruf false
                    dbg("First Timercall");
                }

                count = count + 1;
                if (count > 100)
                {
                    count = 0;
                    if (RegLoad("CyKeyfire", "1") == "1")
                    {
                        sendStrgChar(child, "N");
                        Thread.Sleep(300);
                        sendStrgChar(child, "N");
                    }
                }

                startCount = startCount + 1;

                if (VB.Left(childname, 7) == "Desktop") //childname ="" mainwindow="Desk.."?
                    return;

                if (startCount == 3)
                {
                    timer1.Enabled = false;

                    string monitor = RegLoad("CyMonitor", "");
                    if (monitor != "")
                    {
                        dbg("startCount=" + startCount.ToString());
                        // Beep()
                        SetVDS(monitor);
                        dbg("end startCount=" + startCount.ToString());
                    }

                    timer1.Enabled = true;
                }
            }

            catch (Exception ex)
            {
                Debug.Print(DateTime.Now.ToLongTimeString() + " shutdown dummy timer =" + ex.Message);
                dbg(ex.Message);
            }
            startflag = true;
        }
        #endregion

        #region " Send Keys "
        private void sendStrgF10(IntPtr hwnd)
        {
            // strg+F10
            SendMessage(hwnd, 0x100, 0x11, 0x1D0001);
            SendMessage(hwnd, 0x104, 0x79, 0x440001);
           Thread.Sleep(100);
            SendMessage(hwnd, 0x105, 0x79, 0xC0440001);
            SendMessage(hwnd, 0x101, 0x11, 0xC01D0001);
           Thread.Sleep(100);
        }
        private void sendStrgESC(IntPtr hwnd)
        {
            // strg+ESC
            SendMessage(hwnd, 0x100, 0x11, 0x1D0001);
            SendMessage(hwnd, 0xB100, 0x1B, 0x10001);
           Thread.Sleep(100);
            SendMessage(hwnd, 0x105, 0x1B, 0x80010001);
            SendMessage(hwnd, 0x101, 0x11, 0xC01D0001);
           Thread.Sleep(100);
        }
        private void sendStrgChar(IntPtr hwnd, string onechar)
        {
            // strg+C
            SendMessage(hwnd, 0x100, 0x11, 0x1D0001);
            SendMessage(hwnd, 0x100, VB.Asc(onechar), 0x2E0001);
           Thread.Sleep(100);
            SendMessage(hwnd, 0x101, VB.Asc(onechar), 0xC02E0001);
            SendMessage(hwnd, 0x101, 0x11, 0xC01D0001);
           Thread.Sleep(100);
        }
        #endregion

        #region " Send Mouse "
        public void click_max(IntPtr hwnd)
        {
            Point old;
            int x;
            int y;
            RECT rc = new RECT();
            GetWindowRect(hwnd, ref rc);
            x = rc.Right - 75;
            y = rc.Top + 13;
            old = Cursor.Position;
            Cursor.Position = new Point(x, y);
            mouse_event(2, 0, 0, 0, IntPtr.Zero);
            mouse_event(4, 0, 0, 0, IntPtr.Zero);
            Cursor.Position = old;
        }
        public void click_left(IntPtr hwnd, int x, int y, bool fromMid)
        {
            Point old;
            RECT rc = new RECT();
            GetWindowRect(hwnd, ref rc);
            int x1;
            int y1;
            if (fromMid == false)
            {
                if (x < 0)
                    x1 = rc.Right + x;
                else
                    x1 = rc.Left + x;
                if (y < 0)
                    y1 = rc.Bottom + y;
                else
                    y1 = rc.Top + y;
            }
            else
            {
                x1 = rc.Left + ((rc.Right - rc.Left) / 2) + x;
                y1 = rc.Top + ((rc.Bottom - rc.Top) / 2) + y;
            }
            old = Cursor.Position;
            Cursor.Position = new Point(x1, y1);
            mouse_event(2, 0, 0, 0, IntPtr.Zero);
            mouse_event(4, 0, 0, 0, IntPtr.Zero);
        }
        public void click_right(IntPtr hwnd, int x, int y, bool fromMid)
        {
            Point old;
            RECT rc = new RECT();
            GetWindowRect(hwnd, ref rc);
            int x1;
            int y1;
            if (fromMid == false)
            {
                if (x < 0)
                    x1 = rc.Right + x;
                else
                    x1 = rc.Left + x;
                if (y < 0)
                    y1 = rc.Bottom + y;
                else
                    y1 = rc.Top + y;
            }
            else
            {
                x1 = rc.Left + ((rc.Right - rc.Left) / 2) + x;
                y1 = rc.Top + ((rc.Bottom - rc.Top) / 2) + y;
            }
            old = Cursor.Position;
            Cursor.Position = new Point(x1, y1);
            mouse_event(8, 0, 0, 0, IntPtr.Zero);
            mouse_event(16, 0, 0, 0, IntPtr.Zero);
            Cursor.Position = old;
        }
        #endregion

        #region " Set VDS "
        private void SetVDS(string monitor)
        {
            Process bProcess = Process.GetProcessesByName("wfica32").FirstOrDefault();
            IntPtr handle = bProcess.MainWindowHandle;
            IntPtr child = GetWindow(handle, 5);
            IntPtr foreG = GetForegroundWindow();
            int show = getShowCmd(handle);

            if (handle == (IntPtr)0)
                return;
            System.Text.StringBuilder Caption = new System.Text.StringBuilder(256);
            GetWindowText(child, Caption, Caption.Capacity);
            string childname = Caption.ToString();

            BringMainWindowToFront(handle);
            shortwait(4);

            MachKeinVollbild(handle);

            click_right(handle, 10, 10, false);
            shortwait(2); // 
            click_left(handle, 133, 140, false); // sitzungsanzeigemodus
            shortwait(2); // 

            if (VB.Val(monitor) > 9)
            {
                click_left(handle, -68, -20, true); // Breite
                shortwait(2); // 
                SendKeys.Send("{BACKSPACE}{BACKSPACE}{BACKSPACE}{BACKSPACE}" + "2700");
                shortwait(2); // 
                click_left(handle, -68, 90, true); // OK
                shortwait(2); // 
                setScreen(handle, 21, "99");
                shortwait(2);
                click_right(handle, 10, 10, false);
                shortwait(2); // 
                click_left(handle, 133, 160, false); // Fenstergröße
                shortwait(2); // 
                click_left(handle, 350, 180, false); // verkleinern
                shortwait(2); // 
                click_right(handle, 10, 10, false);
                shortwait(2); // 
                click_left(handle, 133, 160, false); // Fenstergröße
                shortwait(2); // 
                click_left(handle, 350, 160, false); // vergrößern
            }
            else
            {
                click_left(handle, -28, -60, true);
                shortwait(2); // 
                click_left(handle, -28, 0, true);
                shortwait(2); // 
                click_left(handle, -68, 90, true);
                shortwait(2); // 
                setScreen(handle, 21, monitor);
                shortwait(5);
                click_max(handle);
            }
        }
        private void setScreen(IntPtr handle, int flags, string monitor)
        {
            int mon = VB.Val(monitor);
            bool both = false;
            if (mon > 9)
            {
                monitor = "0";
                both = true;
            }
            Screen screens = Screen.AllScreens[mon];
            Rectangle bounds = screens.Bounds;
            Rectangle working = screens.WorkingArea;
            int x;
            int y;
            int w;
            int h;

            if (working.Height < bounds.Height & working.Y > 0)
            {
                // Top
                x = mon * Screen.PrimaryScreen.Bounds.Width;
                y = bounds.Height - working.Height;
                w = bounds.Width;
                h = working.Height;
            }
            else if (working.Height < bounds.Height & working.Y == 0)
            {
                // Bottom
                x = mon * (Screen.PrimaryScreen.Bounds.Width);
                y = 0;
                w = bounds.Width;
                h = working.Height;
            }
            else if (working.Width < bounds.Width & working.X > 0)
            {
                // Left
                x = (mon * Screen.PrimaryScreen.Bounds.Width) + bounds.Width - working.Width;
                y = 0;
                w = working.Width;
                h = bounds.Height;
            }
            else if (working.Width < bounds.Width & working.X == 0)
            {
                // Right
                x = mon * (Screen.PrimaryScreen.Bounds.Width);
                y = 0;
                w = working.Width;
                h = bounds.Height;
            }
            else
            {
                // Nothing
                x = mon * (Screen.PrimaryScreen.Bounds.Width);
                y = 0;
                w = bounds.Width;
                h = bounds.Height;
            }

            if (both == true)
            {
                x = 1920 - 780;
                y = 0;
                w = bounds.Width;
                h = bounds.Height;
            }



            dbg("Setwindowspos mon=" + monitor + "  " + x.ToString() + "," + y.ToString() + "," + w.ToString() + "," + h.ToString() + "," + flags.ToString());

            SetWindowPos(handle, 0, x, y, w, h, flags);
        }

        public void waitForMaxWindow(IntPtr hwnd)
        {
            for (var a = 1; a <= 200; a++)
            {
                int max = getShowCmd(hwnd);
                IntPtr fg = GetForegroundWindow();
                if (max == 3 & hwnd == fg)
                    break;
                Application.DoEvents();
               Thread.Sleep(100);
            }
        }
        public void BringMainWindowToFront(IntPtr hwnd)
        {
            ShowWindow(hwnd, ShowWindowEnum.Maximize);
            BringWindowToTop(hwnd);
            SetForegroundWindow(hwnd);
        }
        public int getShowCmd(IntPtr hwnd)
        {
            WINDOWPLACEMENT wp=new WINDOWPLACEMENT();
            wp.Length = System.Runtime.InteropServices.Marshal.SizeOf(wp);
            GetWindowPlacement(hwnd,ref wp);
            return wp.showCmd;
        }
        public int getSize_W(IntPtr hwnd)
        {
            WINDOWPLACEMENT wp = new WINDOWPLACEMENT();
            wp.Length = System.Runtime.InteropServices.Marshal.SizeOf(wp);
            GetWindowPlacement(hwnd,ref wp);
            return wp.rcNormalPosition.Right;
        }
        private int isVollbild()
        {
            Process bProcess = Process.GetProcessesByName("wfica32").FirstOrDefault();
            IntPtr handle = bProcess.MainWindowHandle;
            IntPtr child = GetWindow(handle, 5);
            int ret = 1; // error
            IntPtr foreG = GetForegroundWindow();
            int show = getShowCmd(handle);
            if (handle != (IntPtr)0)
            {
                System.Text.StringBuilder Caption = new System.Text.StringBuilder(256);
                GetWindowText(child, Caption, Caption.Capacity);
                string childname = Caption.ToString();

                WINDOWPLACEMENT wp = new WINDOWPLACEMENT();
                wp.Length = System.Runtime.InteropServices.Marshal.SizeOf(wp);
                GetWindowPlacement(handle,ref wp);
                ret = wp.ptMaxPosition.y;
                dbg("x=" + wp.ptMaxPosition.x.ToString() + "  y=" + wp.ptMaxPosition.y.ToString());
            }
            return ret;
        }
        private void MachKeinVollbild(IntPtr handle)
        {
            IntPtr child = GetWindow(handle, 5);
            setScreen(handle, 16, "1");
            shortwait(4);
            sendStrgF10(child);
            shortwait(4);
            for (int a = 1; a <= 4; a++)
            {
                if (isVollbild() < -1)
                {
                    dbg("send F10");
                    sendStrgF10(child);
                    shortwait(6);
                }
                else
                    break;
            }
            dbg("ende prüfe nochmal");
            isVollbild();
        }
        #endregion

        #region " Sonstiges "
        private string RegLoad(string var, string defaultx = "")
        {
            RegistryKey MeinKey;
            MeinKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\AB Server", true);
            if (MeinKey == null)
                MeinKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\AB Server");
            return MeinKey.GetValue(var, defaultx).ToString();
        }
        private void shortwait(int je100ms = 10)
        {
            for (int a = 1; a <= je100ms; a++)
            {
                Application.DoEvents();
                Thread.Sleep(100);
            }
        }

        private void dbg(string text)
        {
        }
        #endregion

    }
}

