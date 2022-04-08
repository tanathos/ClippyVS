using Recoding.ClippyVSPackage.Configurations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
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


        /// <summary>
        /// Watch out, only supports single layer/image for now 
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="spriteResourceUri"></param>
        protected void InitAssistant(Panel canvas, string spriteResourceUri)
        {
            // ReSharper disable once RedundantAssignment
            var spResUri = spriteResourceUri;
#if Dev19
            spResUri = spriteResourceUri.Replace("ClippyVs2022", "ClippyVSPackage");
#endif
#if Dev22
#endif
            // pass BitmapImage
            this.Sprite = new BitmapImage(new Uri(spResUri, UriKind.RelativeOrAbsolute));

            ClippedImage = new Image
            {
                Source = Sprite,
                Stretch = Stretch.None
            };

            if (canvas == null) return;

            canvas.Children.Clear();
            canvas.Children.Add(ClippedImage);
        }

        protected Dictionary<string, Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames>> RegisterAnimationsImpl(string animationsResourceUri,
           
            EventHandler xDoubleAnimationCompleted, int clipWidth, int clipHeight)
        {
            var spResUri = animationsResourceUri;
            var animations = new Dictionary<string, Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames>>();

#if Dev19
            spResUri = spResUri.Replace("ClippyVs2022", "ClippyVSPackage");
#endif
            var uri = new Uri(spResUri, UriKind.RelativeOrAbsolute);

            var info = Application.GetResourceStream(uri);

            if (info == null)
                return animations;

            // Can go to Constructor/Init
            var storedAnimations =
                Newtonsoft.Json.JsonConvert.DeserializeObject<List<ClippySingleAnimation>>(StreamToString(info.Stream));
            
            if (storedAnimations == null) return animations;

            foreach (var animation in storedAnimations)
            {
                var xDoubleAnimation = new DoubleAnimationUsingKeyFrames
                {
                    FillBehavior = FillBehavior.HoldEnd
                };

                var yDoubleAnimation = new DoubleAnimationUsingKeyFrames
                {
                    FillBehavior = FillBehavior.HoldEnd
                };

                double timeOffset = 0;

                foreach (var frame in animation.Frames)
                {
                    if (frame.ImagesOffsets != null)
                    {
                        var lastCol = frame.ImagesOffsets.Column;
                        var lastRow = frame.ImagesOffsets.Row;

                        // X
                        var xKeyFrame = new DiscreteDoubleKeyFrame(clipWidth * -lastCol, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(timeOffset)));

                        // Y
                        var yKeyFrame = new DiscreteDoubleKeyFrame(clipHeight * -lastRow, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(timeOffset)));

                        timeOffset += ((double)frame.Duration / 1000);
                        xDoubleAnimation.KeyFrames.Add(xKeyFrame);
                        yDoubleAnimation.KeyFrames.Add(yKeyFrame);
                    }
                }

                animations.Add(animation.Name, new Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames>(xDoubleAnimation, yDoubleAnimation));
                xDoubleAnimation.Completed += xDoubleAnimationCompleted;
               
            }
            return animations;
        }
    }
}