using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Media;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace myCTI2
{
    public static class VB
    {
        public static short Asc(string String)
        {
            return Encoding.Default.GetBytes(String)[0];
        }
        public static string Chr(int CharCode)
        {
            if (CharCode > 255)
                throw new ArgumentOutOfRangeException("CharCode", CharCode, "CharCode must be between 0 and 255.");
            return Encoding.Default.GetString(new[] { (byte)CharCode });
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
                   ? value
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
        public static string ToUTF8(string iso)
        {
            Encoding utf_8 = Encoding.UTF8;
            byte[] utf8Bytes = System.Text.Encoding.UTF8.GetBytes(iso);
            string s_unicode2 = System.Text.Encoding.Default.GetString(utf8Bytes);
            return s_unicode2;
        }
        public static string ToISO(string utf8)
        {
            Encoding utf_8 = Encoding.UTF8;
            byte[] isoBytes = System.Text.Encoding.Default.GetBytes(utf8);
            string s_iso2 = System.Text.Encoding.UTF8.GetString(isoBytes);
            return s_iso2;
        }
    }

    public static class TOOL
    {
        const int trace = 1;

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
                byte[] byteArray = Encoding.Default.GetBytes(audiostring);
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

        internal static string DownloadWebpage(string URL, int timeout = 500)
        {
            System.IO.Stream IoStream = null;
            System.IO.StreamReader StrRead = null;
            string web = "";
            try
            {
                System.Net.HttpWebRequest Request = (HttpWebRequest)WebRequest.Create(URL);
                Request.Timeout = timeout;
                
                Request.UserAgent = "myGPS";
                IoStream = Request.GetResponse().GetResponseStream();
                StrRead = new System.IO.StreamReader(IoStream, System.Text.Encoding.Default);
                web = StrRead.ReadToEnd();
                StrRead.Close();
                IoStream.Close();
            }
            catch (Exception ex)
            {
                errorx("DownloadWebpage ERROR Url=" + URL);
                errorx("DownloadWebpage ERROR " + ex.Message);
            }
            return web;
        }
        internal static void String2File(string data, string file)
        {
            byte[] tmparray;
            tmparray = Encoding.Default.GetBytes(data);
            FileStream writex = new FileStream(file, FileMode.Create);
            writex.Write(tmparray, 0, tmparray.Length);
            writex.Close();
        }
        public static string File2String(string file)
        {
            int a = 0;
            int dateilänge = 0;
            string data = null;
            byte[] tmparray = null;
            FileStream readx = new FileStream(file, FileMode.Open);
            dateilänge = Convert.ToInt32(readx.Length);
            tmparray = new byte[dateilänge];
            a = readx.Read(tmparray, 0, dateilänge);
            readx.Close();
            data = Encoding.Default.GetString(tmparray);
            return data;
        }

        internal static void DownloadFile(string URL, string file, int timeout = 500)
        {
            string data = "";
            data= DownloadWebpage(URL, timeout);
            String2File(data, file);
        }

        internal static void debugx(string text)
        {
            debugx(0, text);
        }
        internal static void debugx(int tr, string text)
        {
            if ((tr & trace) > 0 | (tr == 0))
            {
                try
                {
                    File.AppendAllText(Application.StartupPath + @"\trace.txt", DateTime.Now.ToLongTimeString() + ":" + text + "\r\n");
                }
                catch
                {
                }
            }
        }

        internal static void errorx(string text)
        {
                 try
                {
                    File.AppendAllText(Application.StartupPath + @"\error.txt", DateTime.Now.ToLongTimeString() + ":" + text + "\r\n");
                }
                catch
                {
                }
        }

        #endregion
    }
}