using System;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Recoding.ClippyVSPackage
{
    public class AssistantBase : IDisposable
    {
        /// <summary>
        /// The time dispatcher to perform the animations in a random way
        /// </summary>
        protected DispatcherTimer WpfAnimationsDispatcher;

        /// <summary>
        /// The sprite with all the animation stages for Clippy
        /// </summary>
        protected BitmapSource Sprite;
        
        /// <summary>
        /// The image that holds the sprite
        /// </summary>
        protected Image ClippedImage;

        /// <summary>
        /// Seconds between a random idle animation and another
        /// </summary>
        protected const int IdleAnimationTimeout = 45;

        /// <summary>
        /// When is true it means an animation is actually running
        /// </summary>
        protected bool IsAnimating { get; set; }

        /// <summary>
        /// Reads the content of a stream into a string
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        protected static string StreamToString(Stream stream)
        {
            string streamString;
            stream.Position = 0;
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                streamString = reader.ReadToEnd();
            }
            return streamString;
        }

        public void Dispose()
        {
            WpfAnimationsDispatcher?.Stop();
        }
    }
}