namespace Recoding.ClippyVSPackage.Configurations
{
    /// <summary>
    /// A single frame in an animation
    /// </summary>
    public class Frame
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public Frame()
        {

        }

        /// <summary>
        /// How long this frame as to be seen, expressed in milliseconds
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// The image to show in this frame, expressed as offsetts of the global sprite
        /// </summary>
        public ColumnRow ImagesOffsets { get; set; }
    }
}
