using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recoding.ClippyVSPackage.Configurations
{
    public class Animation
    {
        public Animation()
        {
            Frames = new List<Frame>();
        }

        public string Name { get; set; }

        public List<Frame> Frames { get; set; }
    }
}
