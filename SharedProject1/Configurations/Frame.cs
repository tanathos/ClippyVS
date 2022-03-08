namespace Recoding.ClippyVSPackage.Configurations
{
    /// <summary>
    /// A single frame in an animation
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Frame
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        // ReSharper disable once EmptyConstructor
        public Frame()
        {

        }

        /// <summary>
        /// How long this frame as to be seen, expressed in milliseconds
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public int Duration { get; set; }

        /// <summary>
        /// The image to show in this frame, expressed as offsetts of the global sprite
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public ColumnRow ImagesOffsets { get; set; }
    }
}
