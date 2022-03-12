using System.Collections.Generic;

namespace Recoding.ClippyVSPackage.Configurations
{
    /// <summary>
    /// Represents a single animation, as it is directly mapped from the JSON, NOT valid for legacy Genius json, only Clippy and Merlin (Converted)
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
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
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string Name { get; set; }

        /// <summary>
        /// Frames in the animation
        /// </summary>
        // ReSharper disable once CollectionNeverUpdated.Global
        public List<Frame> Frames { get; }
    }


}
