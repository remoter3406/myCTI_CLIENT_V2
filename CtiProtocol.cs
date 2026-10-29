using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace myCTI2
{
    // Anrufdatensatz aus einem UDP Paket (ohne "CSTA")
    public class CallRecord
    {
        public string Cmd = "";
        public string Ziel = "";
        public string Anrufer = "";
        public string For = "";
        public string Kampagne = "";
        public string Id = "";
        public string Bild = "";
        public int FieldCount;   // Anzahl der Felder (= Anzahl '|' + 1, max. 5)
    }

    // reine Parser ohne UI, damit sie testbar sind
    public static class CtiProtocol
    {
        // Felder ab Index 2 (Befehlsziffer + ein weiteres Zeichen), max. 5 Felder,
        // das letzte (Bild) kann selbst '|' enthalten
        private static string[] fields(string text)
        {
            return VB.Mid(text ?? "", 2).Split(new[] { '|' }, 5);
        }

        // Format: <cmd><x><ziel>|<anrufer>|<for>|<id>|<bild>
        // Felder werden nur gesetzt, wenn das abschliessende '|' vorhanden ist
        public static CallRecord ParseCall(string text)
        {
            CallRecord r = new CallRecord();
            r.Cmd = VB.Left(text ?? "", 1);
            string[] f = fields(text);
            r.FieldCount = f.Length;
            if (f.Length >= 2) r.Ziel = f[0];
            if (f.Length >= 3) r.Anrufer = f[1];
            if (f.Length >= 4) r.For = f[2];
            if (f.Length >= 5)
            {
                r.Id = f[3];
                r.Bild = f[4];
            }

            // for = <nst>_<kampagne>_<datum>_<info>
            string[] sXFor = r.For.Split('_');
            r.For = sXFor[0];
            if (sXFor.Length >= 4)
                r.Kampagne = "Kampagne " + sXFor[1] + " Rückruf vom " + sXFor[2] + " (" + sXFor[3] + ")";
            return r;
        }

        // Befehl 5: erstes Zeichen des 4. Feldes, "1" = blinken, "0" = aus, sonst null
        public static bool? ParseBlink(string text)
        {
            string[] f = fields(text);
            if (f.Length < 4)
                return null;
            string blink = VB.Left(f[3], 1);
            if (blink == "1") return true;
            if (blink == "0") return false;
            return null;
        }

        // Liste verpasster Anrufe "nst,datum|nst,datum|" -> Monitor URL, "" wenn keine
        // nur mit '|' abgeschlossene Einträge zählen
        public static string BuildLostCallUrl(string lost)
        {
            DateTime tmpdate = new DateTime(2099, 1, 1, 12, 0, 0);
            string filter = "";
            string[] entries = (lost ?? "").Split('|');
            for (int i = 0; i < entries.Length - 1; i++)
            {
                string entry = entries[i];
                int pos = entry.IndexOf(",");
                if (pos <= 0)
                    continue;
                DateTime dat;
                if (!parseServerDate(VB.Mid(entry, pos + 1), out dat))
                {
                    TOOL.errorx("BuildLostCallUrl Datum nicht lesbar: " + entry);
                    continue;
                }
                if (dat < tmpdate)
                    tmpdate = dat;
                filter = filter + VB.Left(entry, pos) + ",";
            }
            if (filter == "")
                return "";
            tmpdate = tmpdate.AddMinutes(-3);
            return "Monitor.PL?datum=" + tmpdate.ToBinary().ToString() + "&filter=" + filter;
        }

        // erst aktuelle Kultur, dann deutsch, dann invariant
        private static bool parseServerDate(string text, out DateTime dat)
        {
            return DateTime.TryParse(text, out dat)
                || DateTime.TryParse(text, new CultureInfo("de-DE"), DateTimeStyles.None, out dat)
                || DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out dat);
        }

        // *, # und I (intern) unverändert, sonst nur Ziffern, + - ( )
        public static string CleanDialNumber(string number)
        {
            if (string.IsNullOrEmpty(number))
                return "";
            if (isSpecialDial(number))
                return number;
            return Regex.Replace(number, @"[^\d+\-()]", "");
        }
        public static bool IsDialable(string num)
        {
            if (string.IsNullOrEmpty(num))
                return false;
            if (isSpecialDial(num))
                return true;
            return Regex.Replace(num, @"[^\d]", "") != "";
        }
        private static bool isSpecialDial(string number)
        {
            string c = VB.Left(number, 1);
            return c == "*" | c == "#" | c == "I";
        }

        // URL Kodierung wie vom Server für dial/deflect erwartet (ANSI, Leerzeichen als +)
        public static string UrlEncodeDial(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "";
            for (int i = text.Length - 1; i >= 0; i--)
            {
                short C = VB.Asc(VB.Mid(text, i, 1));
                if ((C >= 97 & C <= 122) | (C >= 45 & C <= 58) | (C >= 63 & C <= 90) | C == 33 | C == 36 | C == 42 | C == 95 | C == 126)
                {
                    // unverändert
                }
                else
                    text = VB.Left(text, i) + ("%" + C.ToString("X2")) + VB.Mid(text, i + 1);
            }
            return text.Replace("%20", "+");
        }
    }
}
