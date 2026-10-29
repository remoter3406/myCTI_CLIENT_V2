using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace myCTI2
{
    // unsichtbare Form: Windows-Shutdown abfangen und globale Hotkeys registrieren
    public partial class ShutDownDummy : Form
    {
        #region " Storage "
        public event WindowsShutDownEventHandler WindowsShutDown;
        public delegate void WindowsShutDownEventHandler(object sender, EventArgs e);
        public event DialHotkeyEventHandler DialHotkey;
        public delegate void DialHotkeyEventHandler(object sender, EventArgs e);
        public event SpaceHotkeyEventHandler SpaceHotkey;
        public delegate void SpaceHotkeyEventHandler(object sender, EventArgs e);

        [DllImport("user32")]
        private static extern int RegisterHotKey(IntPtr hWnd, int id, uint fsModifier, uint vk);
        [DllImport("user32")]
        private static extern void UnregisterHotKey(IntPtr hWnd, int id);

        private const int WM_HOTKEY = 0x0312;
        private const int Key_STRG = 0x2;

        private const int HOTKEY_DIAL = 4;   // Strg+<Taste aus Einstellungen>: markierten Text wählen
        private const int HOTKEY_SPACE = 5;  // Strg+Leertaste: Popups ausblenden
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
                UnregisterHotKey(this.Handle, HOTKEY_DIAL);
                UnregisterHotKey(this.Handle, HOTKEY_SPACE);
                WindowsShutDown?.Invoke(this, e);
            }
            else
                e.Cancel = true;
        }

        private void newHotkey()
        {
            string shortcut = LocalReg.Load("hotkey", "Y");
            registerHotkey(HOTKEY_DIAL, (uint)VB.Asc(shortcut));
            registerHotkey(HOTKEY_SPACE, (uint)VB.Asc(" "));
        }
        private void registerHotkey(int id, uint vk)
        {
            try
            {
                UnregisterHotKey(this.Handle, id);
                RegisterHotKey(this.Handle, id, Key_STRG, vk);
            }
            catch (Exception ex)
            {
                TOOL.errorx("registerHotkey " + id.ToString() + " error: " + ex.Message);
            }
        }
        #endregion

        #region " Window Messages"
        protected override void WndProc(ref Message m)
        {
            try
            {
                if (m.Msg == WM_HOTKEY)
                {
                    switch ((int)m.WParam)
                    {
                        case HOTKEY_DIAL:
                            DialHotkey?.Invoke(this, EventArgs.Empty);
                            break;
                        case HOTKEY_SPACE:
                            SpaceHotkey?.Invoke(this, EventArgs.Empty);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                TOOL.errorx("ShutDownDummy WndProc error: " + ex.Message);
            }
            base.WndProc(ref m);
        }
        #endregion
    }
}
