using System;
using System.Globalization;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using myCTI2;

namespace myCTI.Tests
{
    [TestClass]
    public class ParseCallTests
    {
        [TestMethod]
        public void VollstaendigerDatensatz()
        {
            CallRecord r = CtiProtocol.ParseCall("1x0301|0171234|0301|abc123|IMG");
            Assert.AreEqual("1", r.Cmd);
            Assert.AreEqual("0301", r.Ziel);
            Assert.AreEqual("0171234", r.Anrufer);
            Assert.AreEqual("0301", r.For);
            Assert.AreEqual("abc123", r.Id);
            Assert.AreEqual("IMG", r.Bild);
            Assert.AreEqual("", r.Kampagne);
            Assert.AreEqual(5, r.FieldCount);
        }

        [TestMethod]
        public void BildDarfPipeEnthalten()
        {
            CallRecord r = CtiProtocol.ParseCall("1x0301|0171234|0301|abc|IM|G");
            Assert.AreEqual("abc", r.Id);
            Assert.AreEqual("IM|G", r.Bild);
        }

        [TestMethod]
        public void FelderNurMitAbschliessendemPipe()
        {
            CallRecord r = CtiProtocol.ParseCall("3x0301|0171234");
            Assert.AreEqual(2, r.FieldCount);
            Assert.AreEqual("0301", r.Ziel);
            Assert.AreEqual("", r.Anrufer);

            r = CtiProtocol.ParseCall("3x0301|0171234|");
            Assert.AreEqual(3, r.FieldCount);
            Assert.AreEqual("0171234", r.Anrufer);
            Assert.AreEqual("", r.Id);
        }

        [TestMethod]
        public void Kampagne()
        {
            CallRecord r = CtiProtocol.ParseCall("1x0301|0171234|0301_Sommer_01.07.2026_Info|id|");
            Assert.AreEqual("0301", r.For);
            Assert.AreEqual("Kampagne Sommer Rückruf vom 01.07.2026 (Info)", r.Kampagne);
        }

        [TestMethod]
        public void UnvollstaendigeKampagneOhneException()
        {
            CallRecord r = CtiProtocol.ParseCall("1x0301|0171234|0301_Sommer|id|");
            Assert.AreEqual("0301", r.For);
            Assert.AreEqual("", r.Kampagne);
        }

        [TestMethod]
        public void LeererText()
        {
            CallRecord r = CtiProtocol.ParseCall("");
            Assert.AreEqual("", r.Cmd);
            Assert.AreEqual("", r.Id);
            r = CtiProtocol.ParseCall(null);
            Assert.AreEqual("", r.Ziel);
        }
    }

    [TestClass]
    public class ParseBlinkTests
    {
        [TestMethod]
        public void BlinkenAnAus()
        {
            Assert.AreEqual(true, CtiProtocol.ParseBlink("5x|a|b|1"));
            Assert.AreEqual(false, CtiProtocol.ParseBlink("5x|a|b|0|rest"));
        }

        [TestMethod]
        public void OhneViertesFeldOderUnbekannt()
        {
            Assert.IsNull(CtiProtocol.ParseBlink("5x|a|b"));
            Assert.IsNull(CtiProtocol.ParseBlink("5x|a|b|"));
            Assert.IsNull(CtiProtocol.ParseBlink("5x|a|b|7"));
        }
    }

    [TestClass]
    public class LostCallTests
    {
        [TestMethod]
        public void KeineVerpasstenAnrufe()
        {
            Assert.AreEqual("", CtiProtocol.BuildLostCallUrl(""));
            Assert.AreEqual("", CtiProtocol.BuildLostCallUrl(null));
        }

        [TestMethod]
        public void FilterUndFruehestesDatumMinus3Minuten()
        {
            DateTime d1 = new DateTime(2026, 10, 29, 10, 0, 0);
            DateTime d2 = new DateTime(2026, 10, 29, 9, 30, 0);
            string lost = "201," + d1.ToString() + "|202," + d2.ToString() + "|";
            string url = CtiProtocol.BuildLostCallUrl(lost);
            Assert.AreEqual("Monitor.PL?datum=" + d2.AddMinutes(-3).ToBinary().ToString() + "&filter=201,202,", url);
        }

        [TestMethod]
        public void DeutschesDatumUnabhaengigVonKultur()
        {
            CultureInfo alt = CultureInfo.CurrentCulture;
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                string url = CtiProtocol.BuildLostCallUrl("201,29.10.2026 10:00:00|");
                DateTime erwartet = new DateTime(2026, 10, 29, 9, 57, 0);
                Assert.AreEqual("Monitor.PL?datum=" + erwartet.ToBinary().ToString() + "&filter=201,", url);
            }
            finally
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = alt;
            }
        }

        [TestMethod]
        public void NichtAbgeschlossenerOderKaputterEintragWirdIgnoriert()
        {
            Assert.AreEqual("", CtiProtocol.BuildLostCallUrl("201,29.10.2026 10:00:00"));
            Assert.AreEqual("", CtiProtocol.BuildLostCallUrl("201,kaputt|"));
            Assert.AreEqual("", CtiProtocol.BuildLostCallUrl("ohneKomma|"));
        }
    }

    [TestClass]
    public class DialTests
    {
        [TestMethod]
        public void NummerBereinigen()
        {
            Assert.AreEqual("+49(30)123-4567", CtiProtocol.CleanDialNumber("+49 (30) 123-45 67"));
            Assert.AreEqual("0301234", CtiProtocol.CleanDialNumber("Tel: 030/1234"));
        }

        [TestMethod]
        public void SonderwahlUnveraendert()
        {
            Assert.AreEqual("*21#", CtiProtocol.CleanDialNumber("*21#"));
            Assert.AreEqual("# 1", CtiProtocol.CleanDialNumber("# 1"));
            Assert.AreEqual("I201", CtiProtocol.CleanDialNumber("I201"));
        }

        [TestMethod]
        public void Waehlbar()
        {
            Assert.IsTrue(CtiProtocol.IsDialable("0301234"));
            Assert.IsTrue(CtiProtocol.IsDialable("*"));
            Assert.IsFalse(CtiProtocol.IsDialable("()-"));
            Assert.IsFalse(CtiProtocol.IsDialable(""));
            Assert.IsFalse(CtiProtocol.IsDialable(null));
        }

        [TestMethod]
        public void UrlEncodeDial()
        {
            Assert.AreEqual("%2B49+30", CtiProtocol.UrlEncodeDial("+49 30"));
            Assert.AreEqual("a/b:c-1.2", CtiProtocol.UrlEncodeDial("a/b:c-1.2"));
            Assert.AreEqual("%2321%23", CtiProtocol.UrlEncodeDial("#21#"));
            Assert.AreEqual("%28030%29", CtiProtocol.UrlEncodeDial("(030)"));
            byte ae = TOOL.Ansi.GetBytes("ä")[0];
            Assert.AreEqual("%" + ae.ToString("X2"), CtiProtocol.UrlEncodeDial("ä"));
            Assert.AreEqual("", CtiProtocol.UrlEncodeDial(""));
        }
    }

    [TestClass]
    public class HelperTests
    {
        [TestMethod]
        public void MidAmStringende()
        {
            Assert.AreEqual("bc", VB.Mid("abc", 1, 5));
            Assert.AreEqual("b", VB.Mid("abc", 1, 1));
            Assert.AreEqual("", VB.Mid("abc", 5, 1));
            Assert.AreEqual("c", VB.Mid("abc", 2));
        }

        [TestMethod]
        public void Val()
        {
            Assert.AreEqual(42, VB.Val("42"));
            Assert.AreEqual(0, VB.Val("abc"));
            Assert.AreEqual(0, VB.Val(""));
        }

        [TestMethod]
        public void SplitServer()
        {
            string host;
            int port;
            TOOL.SplitServer("cti.firma.local:8080", 80, out host, out port);
            Assert.AreEqual("cti.firma.local", host);
            Assert.AreEqual(8080, port);

            TOOL.SplitServer("192.168.1.5", 443, out host, out port);
            Assert.AreEqual("192.168.1.5", host);
            Assert.AreEqual(443, port);

            // IPv6 (mehrere ':') bleibt unverändert
            TOOL.SplitServer("fe80::1", 80, out host, out port);
            Assert.AreEqual("fe80::1", host);
            Assert.AreEqual(80, port);
        }

        [TestMethod]
        public void UrlEnc()
        {
            Assert.AreEqual("a+b%26c%3d", TOOL.UrlEnc("a b&c="));
            Assert.AreEqual("", TOOL.UrlEnc(null));
        }

        [TestMethod]
        public void AppVersionAusAssemblyInfo()
        {
            Version v = typeof(TOOL).Assembly.GetName().Version;
            Assert.AreEqual(v.Major + "." + v.Minor + "." + v.Build, TOOL.AppVersion);
            StringAssert.Matches(TOOL.AppVersion, new System.Text.RegularExpressions.Regex(@"^\d+\.\d+\.\d+$"));
        }
    }
}
