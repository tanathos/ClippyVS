using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Controls;
using Recoding.ClippyVSPackage;

namespace SharedProjectTests2
{
    [TestClass]
    public class ClippyTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            Canvas panel = new Canvas();
            var subject = new Clippy(panel);

            Assert.IsTrue(subject.AllAnimations.Count > 0);
        }
    }
}
