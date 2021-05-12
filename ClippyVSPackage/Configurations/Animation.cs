using System.Collections.Generic;

namespace Recoding.ClippyVSPackage.Configurations
{
    /// <summary>
    /// Represents a single animation, as it is directly mapped from the JSON
    /// </summary>
    public class ClippyAnimation
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public ClippyAnimation()
        {
            Frames = new List<Frame>();
        }

        /// <summary>
        /// Name of the animation
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Frames in the animation
        /// </summary>
        public List<Frame> Frames { get; }
    }
}
