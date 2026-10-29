using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using myCTI2;

namespace myCTI.Tests
{
    // Absicherung der Umstellung .NET Framework -> .NET: dort war Encoding.Default ANSI,
    // jetzt ist es UTF-8. Das Protokoll braucht weiterhin ANSI (ein Byte = ein Zeichen).
    [TestClass]
    public class EncodingTests
    {
        [TestMethod]
        public void AnsiIstEinByteCodepage()
        {
            Assert.IsTrue(TOOL.Ansi.IsSingleByte, "Codepage " + TOOL.Ansi.CodePage + " ist keine Ein-Byte-Codepage");
            Assert.AreNotEqual(65001, TOOL.Ansi.CodePage, "ANSI darf nicht UTF-8 sein");
        }

        [TestMethod]
        public void UmlauteWieBisher()
        {
            // Werte wie in convertUTF2HTML (Latin-1/1252)
            Assert.AreEqual(0xE4, TOOL.Ansi.GetBytes("ä")[0]);
            Assert.AreEqual(0xDF, TOOL.Ansi.GetBytes("ß")[0]);
            Assert.AreEqual("Müller", TOOL.Ansi.GetString(new byte[] { 0x4D, 0xFC, 0x6C, 0x6C, 0x65, 0x72 }));
        }

        [TestMethod]
        public void AlleBytesUeberlebenHinUndZurueck()
        {
            // WAV-Dateien und Anruferbilder werden als String weitergereicht
            byte[] alle = Enumerable.Range(0, 256).Select(i => (byte)i).ToArray();
            byte[] zurueck = TOOL.Ansi.GetBytes(TOOL.Ansi.GetString(alle));
            CollectionAssert.AreEqual(alle, zurueck);
        }

        [TestMethod]
        public void AnrufMitUmlautUndBinaeremBild()
        {
            // UDP-Paket wie vom Server: Text in ANSI, danach Bilddaten mit beliebigen Bytes (auch '|')
            byte[] bild = Enumerable.Range(0, 256).Select(i => (byte)i).Concat(new byte[] { 0x7C, 0x00, 0xFF }).ToArray();
            byte[] kopf = TOOL.Ansi.GetBytes("1x0301|Jörg Müller|0301|id42|");
            byte[] paket = kopf.Concat(bild).ToArray();

            CallRecord r = CtiProtocol.ParseCall(TOOL.Ansi.GetString(paket));
            Assert.AreEqual("Jörg Müller", r.Anrufer);
            Assert.AreEqual("id42", r.Id);
            CollectionAssert.AreEqual(bild, TOOL.Ansi.GetBytes(r.Bild));
        }

        [TestMethod]
        public void UrlKodierungMitUmlautAnsi()
        {
            Assert.AreEqual("M%fcller", TOOL.UrlEnc("Müller"));
            Assert.AreEqual("M%FCller", CtiProtocol.UrlEncodeDial("Müller"));
        }
    }
}
