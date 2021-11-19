namespace Recoding.ClippyVSPackage
{
    public interface IClippyVSSettings
    {
        /// <summary>
        /// If true shows clippy at the VS startup
        /// </summary>
        bool ShowAtStartup { get; set; }

        /// <summary>
        /// Performs the store of the instance of this interface to the user's settings
        /// </summary>
        void Store();
    }
}
