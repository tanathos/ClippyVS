using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Controls;
using SharedProject1.AssistImpl;

namespace SharedProjectTests2
{
    [TestClass]
    public class MerlinTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            Canvas panel = new Canvas();
            var subject = new Merlin(panel);

            Assert.IsTrue(subject.AllAnimations.Count > 0);
        }
    }
}
