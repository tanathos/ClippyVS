using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Controls;
using SharedProject1.AssistImpl;

namespace SharedProjectTests2
{
    [TestClass]
    public class GeniusTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            Canvas panel = new Canvas();
            Canvas panel2 = new Canvas();
            var subject = new Genius(panel, panel2);

            Assert.IsTrue(subject.AllAnimations.Count > 0);
        }
    }
}
