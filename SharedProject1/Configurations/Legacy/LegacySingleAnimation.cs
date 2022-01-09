using System.Collections.Generic;

namespace Recoding.ClippyVSPackage.Configurations.Legacy
{
    /* ----- Genius and Legacy -------- */
    public class GeniusSingleAnimation
    {
        public string Name { get; set; }
        public List<Frame> Frames { get; set; }
    }
    public class Branch
    {
        public int frameIndex { get; set; }
        public int weight { get; set; }
    }

    public class Branching
    {
        public List<Branch> branches { get; set; }
    }

    public class Frame
    {
        public int Duration { get; set; }
        public List<List<int>> ImagesOffsets { get; set; }
        public int? exitBranch { get; set; }
        public Branching branching { get; set; }
        public string sound { get; set; }
    }
}
