using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

        // HttpClient wird wiederverwendet und nur bei geänderten Verbindungsdaten neu gebaut,
        // weil Anmeldedaten am Handler hängen
        private readonly object clientLock = new object();
        private HttpClient client;
        private string clientKey;

        private HttpClient getClient(out string baseUrl)
        {
            baseUrl = Ssl + "://" + Server + "/";
            string key = baseUrl + "|" + DefCred + "|" + User + "|" + Password;
            lock (clientLock)
            {
                if (client == null || key != clientKey)
                {
                    HttpClientHandler handler = new HttpClientHandler();
                    if (DefCred)
                        handler.UseDefaultCredentials = true;
                    else
                    {
                        CredentialCache cache = new CredentialCache();
                        cache.Add(new Uri(baseUrl), "NTLM", new NetworkCredential(User, Password));
                        handler.Credentials = cache;
                    }
                    handler.AllowAutoRedirect = true;
                    // TODO Zertifikats-Pinning: bisher werden alle Zertifikate akzeptiert
                    handler.ServerCertificateCustomValidationCallback = (msg, cert, chain, errors) => true;

                    HttpClient old = client;
                    client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(5) };
                    clientKey = key;
                    // laufende Anfragen auf dem alten Client nicht abbrechen
                    if (old != null)
                        Task.Delay(TimeSpan.FromSeconds(10)).ContinueWith(_ => old.Dispose());
                }
                return client;
            }
        }

        // synchron, nur für den Start und Dialoge, die das Ergebnis sofort brauchen
        public string Get(string path, bool noErrFile = false)
        {
            string baseUrl;
            HttpClient c = getClient(out baseUrl);
            try
            {
                using (HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, baseUrl + path))
                using (HttpResponseMessage resp = c.Send(req))
                {
                    return readResponse(resp, path, noErrFile);
                }
            }
            catch (Exception ex)
            {
                return handleError(ex, path, noErrFile);
            }
        }

        public async Task<string> GetAsync(string path, bool noErrFile = false)
        {
            string baseUrl;
            HttpClient c = getClient(out baseUrl);
            try
            {
                using (HttpResponseMessage resp = await c.GetAsync(baseUrl + path).ConfigureAwait(false))
                {
                    await resp.Content.LoadIntoBufferAsync().ConfigureAwait(false);
                    return readResponse(resp, path, noErrFile);
                }
            }
            catch (Exception ex)
            {
                return handleError(ex, path, noErrFile);
            }
        }

        private string readResponse(HttpResponseMessage resp, string path, bool noErrFile)
        {
            TOOL.debugx(1, "getWeb url=" + path + " status=" + (int)resp.StatusCode);
            WebOK = true;
            if (!resp.IsSuccessStatusCode)
            {
                // wie bisher: Fehlerstatus liefert "" (z.B. 401 bei falschem Passwort)
                if (noErrFile == false)
                    TOOL.errorx("GetURL error: (" + (int)resp.StatusCode + ") " + resp.ReasonPhrase + " url=" + path);
                return "";
            }
            // Antwort byteweise als ANSI lesen (WAV-Dateien werden als String weitergereicht)
            byte[] data = resp.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
            string text = TOOL.Ansi.GetString(data);
            if (text.Length > 20)
                TOOL.debugx(1, "getWeb defCred=" + DefCred.ToString() + " responselen=" + text.Length.ToString());
            else
                TOOL.debugx(1, "getWeb defCred=" + DefCred.ToString() + " response=" + text);
            return text;
        }

        private string handleError(Exception ex, string path, bool noErrFile)
        {
            // Timeout oder Servername nicht auflösbar -> Server gilt als nicht erreichbar
            SocketException sex = ex.InnerException as SocketException;
            bool timeout = ex is TaskCanceledException || ex is OperationCanceledException;
            bool noHost = sex != null && (sex.SocketErrorCode == SocketError.HostNotFound || sex.SocketErrorCode == SocketError.NoData);
            WebOK = !(timeout || noHost);

            if (noErrFile == false)
                TOOL.errorx("GetURL error: " + (timeout ? "Timeout" : ex.Message) + " url=" + path);
            return "";
        }

        // serverseitige Registry, abschliessendes CRLF wird entfernt
        public string GetReg(string var)
        {
            return trimCrLf(Get(regGetPath(var)));
        }
        public async Task<string> GetRegAsync(string var)
        {
            return trimCrLf(await GetAsync(regGetPath(var)).ConfigureAwait(false));
        }
        public void SetReg(string var, string wert, Encoding encoding = null)
        {
            Get(regSetPath(var, wert, encoding));
        }
        public Task SetRegAsync(string var, string wert, Encoding encoding = null)
        {
            return GetAsync(regSetPath(var, wert, encoding));
        }

        private static string regGetPath(string var)
        {
            return "action?cmd=getreg&var=" + HttpUtility.UrlEncode(var, TOOL.Ansi);
        }
        private static string regSetPath(string var, string wert, Encoding encoding)
        {
            Encoding enc = encoding ?? TOOL.Ansi;
            return "action?cmd=setreg&var=" + HttpUtility.UrlEncode(var, enc) + "&wert=" + HttpUtility.UrlEncode(wert, enc);
        }
        private static string trimCrLf(string tmp)
        {
            if (VB.Right(tmp, 2) == "\r\n")
                tmp = VB.Left(tmp, tmp.Length - 2);
            return tmp;
        }
    }
}
