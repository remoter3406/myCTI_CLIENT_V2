using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using myCTI2;

namespace myCTI.Tests
{
    // .NET 10 hat keinen BinaryFormatter mehr: prüfen, dass Bilder/Icons aus den .resx weiterhin laden
    [TestClass]
    public class ResourceTests
    {
        private static void inSta(Action action)
        {
            Exception error = null;
            Thread t = new Thread(() =>
            {
                try { action(); }
                catch (Exception ex) { error = ex; }
            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();
            if (error != null)
                throw new AssertFailedException(error.ToString());
        }

        [TestMethod]
        public void Resource1BilderUndIcons()
        {
            Assert.IsNotNull(Resource1.myCTI);
            Assert.IsNotNull(Resource1.myCTI2);
            Assert.IsNotNull(Resource1.smilie1);
            Assert.IsNotNull(Resource1.clockon);
        }

        [TestMethod]
        public void TokenPlatzhalterVorhanden()
        {
            StringAssert.StartsWith(myCTI2.Properties.Resources.token, "FKtokenFKstartFK");
        }

        [TestMethod]
        public void CtiMainBildAusResx()
        {
            ComponentResourceManager rm = new ComponentResourceManager(typeof(CTIMain));
            Assert.IsInstanceOfType(rm.GetObject("buttonZeitplan.Image"), typeof(Image));
            Assert.IsInstanceOfType(rm.GetObject("$this.Icon"), typeof(Icon));
        }

        [TestMethod]
        public void FormsLassenSichErzeugen()
        {
            inSta(() =>
            {
                using (CTIausgabe au = new CTIausgabe(1, "201", "", "")) { Assert.AreEqual("Anruf 1", au.Text); }
                using (ShutDownDummy sd = new ShutDownDummy()) { Assert.IsNotNull(sd.Icon); }
                using (InputBox ib = new InputBox("Titel", "Frage", "x")) { Assert.IsNotNull(ib); }
            });
        }
    }
}
