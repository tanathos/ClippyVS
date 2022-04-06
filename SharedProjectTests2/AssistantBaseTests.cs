using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Recoding.ClippyVSPackage;

namespace SharedProjectTests2
{
    [TestClass]
    public class AssistantBaseTests
    {
        [TestMethod]
        public void TestInit()
        {
            Canvas panel = new Canvas();
            var subj = new TestableAssistantBase();
            subj.InitAssistant(panel, "");
            
        }

    }

    public class TestableAssistantBase : AssistantBase
    {
        public new void InitAssistant(Panel canvas, string spriteResourceUri)
        {
            base.InitAssistant(canvas, spriteResourceUri);
        }
    }
}
