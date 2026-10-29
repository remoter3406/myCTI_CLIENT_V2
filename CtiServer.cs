using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;

namespace myCTI2
{
    // HTTP Zugriff auf den myCTI Server
    public class CtiServer
    {
        public string Server = "";
        public string Ssl = "http";
        public string User = "";
        public string Password = "";
        public bool DefCred = false;     // Windows Anmeldung (NTLM SSO) statt User/Passwort
        public volatile bool WebOK = true; // false nach Timeout / Name nicht auflösbar

        // prozessweit, einmalig beim Start aufrufen
        public static void AcceptAllCertificates()
        {
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(AcceptCert);
        }
        private static bool AcceptCert(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }

        public string Get(string path, bool noErrFile = false)
        {
            TOOL.debugx(1, "getWeb url=" + path);
            string url = Ssl + "://" + Server + "/" + path;
            bool defCred = DefCred;
            string responseFromServer = "";
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                if (defCred)
                    request.UseDefaultCredentials = true;
                else
                {
                    CredentialCache mycache = new CredentialCache();
                    mycache.Add(new Uri(url), "NTLM", new NetworkCredential(User, Password));
                    request.Credentials = mycache;
                }
                request.AllowAutoRedirect = true;
                request.Timeout = 5000;

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.Default))
                {
                    responseFromServer = reader.ReadToEnd();
                }
                WebOK = true;
            }
            catch (Exception ex)
            {
                // Fehlerantwort (z.B. 401) schliessen, sonst bleibt die Verbindung belegt (max. 2 pro Host)
                WebException wex = ex as WebException;
                if (wex != null && wex.Response != null)
                    wex.Response.Close();

                string msg = ex.Message.ToLower();
                WebOK = !(msg.IndexOf("timeout") >= 0 || msg.IndexOf("remotename") >= 0);

                if (noErrFile == false)
                    TOOL.errorx("GetURL error: " + ex.Message + " url=" + path);
            }
            if (responseFromServer.Length > 20)
                TOOL.debugx(1, "getWeb defCred=" + defCred.ToString() + " responselen=" + responseFromServer.Length.ToString());
            else
                TOOL.debugx(1, "getWeb defCred=" + defCred.ToString() + " response=" + responseFromServer);

            return responseFromServer;
        }

        // serverseitige Registry, abschliessendes CRLF wird entfernt
        public string GetReg(string var)
        {
            string tmp = Get("action?cmd=getreg&var=" + HttpUtility.UrlEncode(var, Encoding.Default));
            if (VB.Right(tmp, 2) == "\r\n")
                tmp = VB.Left(tmp, tmp.Length - 2);
            return tmp;
        }
        public void SetReg(string var, string wert, Encoding encoding = null)
        {
            Encoding enc = encoding ?? Encoding.Default;
            Get("action?cmd=setreg&var=" + HttpUtility.UrlEncode(var, enc) + "&wert=" + HttpUtility.UrlEncode(wert, enc));
        }
    }
}
