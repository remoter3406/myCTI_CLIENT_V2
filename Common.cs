using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Web;
using System.Windows.Forms;

namespace myCTI2
{
    public static class VB
    {
        public static short Asc(string String)
        {
            return TOOL.Ansi.GetBytes(String)[0];
        }
        public static int Val(string String)
        {
            try
            { return Convert.ToInt32(String); }
            catch
            { return 0; }
        }
        public static string Left(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            maxLength = Math.Abs(maxLength);

            return (value.Length <= maxLength
                   ? value
                   : value.Substring(0, maxLength)
                   );
        }
        public static string Mid(this string value, int begin, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            maxLength = Math.Abs(maxLength);
            if (begin >= value.Length) return "";
            return (value.Length < (begin + maxLength)
                   ? value.Substring(begin)
                   : value.Substring(begin, maxLength)
                   );
        }
        public static string Mid(this string value, int begin)
        {
            if (string.IsNullOrEmpty(value)) return value;
            if (begin >= value.Length) return "";
            return (value.Length <= begin
                   ? ""
                   : value.Substring(begin)
                   );
        }
        public static string Right(this string value, int length)
        {
            if (String.IsNullOrEmpty(value)) return string.Empty;

            return value.Length <= length ? value : value.Substring(value.Length - length);
        }
    }

    // lokale Einstellungen unter HKCU\SOFTWARE\AB Server
    public static class LocalReg
    {
        private const string KeyPath = @"SOFTWARE\AB Server";

        public static string Load(string var, string defaultx = "")
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(KeyPath))
            {
                return key.GetValue(var, defaultx).ToString();
            }
        }
        public static void Save(string var, string para)
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(KeyPath))
            {
                key.SetValue(var, para);
            }
        }
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

    public static class TOOL
    {
        const int trace = 1;
        const long maxLogSize = 5 * 1024 * 1024;

        #region " VARS "
        private const int APPCOMMAND_VOLUME_MUTE = 0x80000;
        private const int WM_APPCOMMAND = 0x319;

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessageW(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        private static SoundPlayer player;
        public static string audio = "";
        public static string ringAudio = "";

        private static bool isUnMute = false;
        private static readonly object logLock = new object();

        // ANSI-Codepage des Systems (z.B. 1252). Unter .NET Framework war das Encoding.Default,
        // unter .NET ist Encoding.Default UTF-8. Das Serverprotokoll (UDP, HTTP, WAV/Bild als String)
        // braucht ANSI, damit jedes Byte 1:1 einem Zeichen entspricht.
        internal static readonly Encoding Ansi = createAnsi();
        private static Encoding createAnsi()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            return Encoding.GetEncoding(0);
        }
        #endregion

        #region " Internal "
        internal static void playAudio(bool unmute = false)
        {
            if (audio != "") playWav(audio, unmute);
        }
        internal static void playRingAudio(bool unmute = false)
        {
            if (ringAudio != "") playWav(ringAudio, unmute);
        }

        private static void playWav(string audiostring, bool unmute = false)
        {
            try
            {
                byte[] byteArray = TOOL.Ansi.GetBytes(audiostring);
                int byterate = BitConverter.ToInt32(new[] { byteArray[28], byteArray[29], byteArray[30], byteArray[31] }, 0);
                int duration = (byteArray.Length - 8) / byterate;


                bool isMuted = AudioManager.GetMasterVolumeMute() | isUnMute;
                if (isMuted & unmute)
                {
                    muteOff();
                }
                try
                {
                    MemoryStream m_AudioStream = new MemoryStream(byteArray);
                    m_AudioStream.Position = 0;
                    if (player != null) player.Stop();
                    player = new SoundPlayer(m_AudioStream);
                    player.Play();
                    Debug.Print(DateTime.Now.ToLongTimeString() + ":" + "Play");
                }
                catch
                { }

                if (isMuted & unmute) mute(duration);

            }
            catch
            { }
        }
        internal static void muteOff()
        {
            isUnMute = true;

            bool isMuted = AudioManager.GetMasterVolumeMute();
            if (isMuted)
            {
                IntPtr handle = FindWindow(null, "myCTI");
                SendMessageW(handle, WM_APPCOMMAND, handle, (IntPtr)APPCOMMAND_VOLUME_MUTE);
                Debug.Print(DateTime.Now.ToLongTimeString() + ":" + "Mute off");
            }
            Application.DoEvents();
            Thread.Sleep(100);
        }
        internal static void mute(int sek)
        {
            Thread _muteThread = new Thread(MuteThread);
            _muteThread.Start(sek);
        }
        internal static void MuteThread(object e)
        {
            int t = (int)e * 1000 + 500;
            Thread.Sleep(t);

            bool isMuted = AudioManager.GetMasterVolumeMute();
            if (!isMuted)
            {
                IntPtr handle = FindWindow(null, "myCTI");
                SendMessageW(handle, WM_APPCOMMAND, handle, (IntPtr)APPCOMMAND_VOLUME_MUTE);
            }
            Debug.Print(DateTime.Now.ToLongTimeString() + ":" + "Mute on");
            isUnMute = false;
        }

        // Version aus AssemblyInfo, z.B. "2.3.0"
        internal static string AppVersion
        {
            get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(3); }
        }

        // URL Parameter kodieren, ANSI wie der Rest der Serverkommunikation
        internal static string UrlEnc(string text)
        {
            return HttpUtility.UrlEncode(text ?? "", TOOL.Ansi);
        }

        // "host:port" -> host, port (ohne Port -> defaultPort)
        internal static void SplitServer(string server, int defaultPort, out string host, out int port)
        {
            host = server ?? "";
            port = defaultPort;
            int pos = host.IndexOf(":");
            if (pos > 0 && host.IndexOf(":", pos + 1) < 0)
            {
                int p = VB.Val(VB.Mid(host, pos + 1));
                if (p > 0) port = p;
                host = VB.Left(host, pos);
            }
        }

        internal static void debugx(string text)
        {
            debugx(0, text);
        }
        internal static void debugx(int tr, string text)
        {
            if ((tr & trace) > 0 | (tr == 0))
                appendLog("trace.txt", text);
        }

        internal static void errorx(string text)
        {
            appendLog("error.txt", text);
        }

        // threadsicher, bei mehr als 5 MB wird die Datei nach *.old verschoben
        private static void appendLog(string file, string text)
        {
            lock (logLock)
            {
                try
                {
                    string path = Path.Combine(Application.StartupPath, file);
                    FileInfo fi = new FileInfo(path);
                    if (fi.Exists && fi.Length > maxLogSize)
                    {
                        File.Delete(path + ".old");
                        File.Move(path, path + ".old");
                    }
                    File.AppendAllText(path, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + text + "\r\n");
                }
                catch
                {
                }
            }
        }

        #endregion
    }
}
