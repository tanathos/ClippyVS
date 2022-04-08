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

        [TestMethod]
        public void TestRegisterAnimations()
        {
            Canvas panel = new Canvas();
            Canvas panel2 = new Canvas();
            var subject = new TestableGenius(panel, panel2);

            subject.RegisterAnimations();
            Assert.IsTrue(subject.AllAnimations.Count == 47);
        }
    }

    public class TestableGenius : Genius
    {
        public TestableGenius(Panel canvas, Panel canvas1) : base(canvas, canvas)
        {
            
        }

        public new void RegisterAnimations()
        {
            base.RegisterAnimations();
        }
    }
}
