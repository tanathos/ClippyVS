using System.Collections.Generic;

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
