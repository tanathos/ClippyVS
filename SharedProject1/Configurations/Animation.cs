using System.Collections.Generic;

namespace Recoding.ClippyVSPackage.Configurations
{
    /// <summary>
    /// Represents a single animation, as it is directly mapped from the JSON, NOT valid for legacy Genius json, only Clippy and Merlin (Converted)
    /// </summary>
    public class ClippySingleAnimation
    {
        /// <summary>
        /// Default Merlin
        /// </summary>
        public ClippySingleAnimation()
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
