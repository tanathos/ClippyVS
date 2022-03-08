using Microsoft.VisualStudio.Shell;
using Recoding.ClippyVSPackage;
using Recoding.ClippyVSPackage.Configurations;
using SharedProject1.AssistImpl;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

        protected void InitAssistant(Panel canvas, string SpriteResourceUri)
        {
            var spResUri = SpriteResourceUri;
#if Dev19
            spResUri = SpriteResourceUri.Replace("ClippyVs2022", "ClippyVSPackage");
#endif
#if Dev22
#endif
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

        protected void RegisterAnimationsImpl(string AnimationsResourceUri, ref Dictionary<string, Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames>> animations, EventHandler XDoubleAnimation_Completed, int ClipWidth, int ClipHeight)
        {
            var spResUri = AnimationsResourceUri;

#if Dev19
            spResUri = spResUri.Replace("ClippyVs2022", "ClippyVSPackage");
#endif
            var uri = new Uri(spResUri, UriKind.RelativeOrAbsolute);

            var info = Application.GetResourceStream(uri);

            if (info == null)
                return;

            // Can go to Constructor/Init
            var storedAnimations =
                Newtonsoft.Json.JsonConvert.DeserializeObject<List<ClippySingleAnimation>>(StreamToString(info.Stream));

            animations = new Dictionary<string, Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames>>();

            if (storedAnimations == null) return;

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
                        var xKeyFrame = new DiscreteDoubleKeyFrame(ClipWidth * -lastCol, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(timeOffset)));

                        // Y
                        var yKeyFrame = new DiscreteDoubleKeyFrame(ClipHeight * -lastRow, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(timeOffset)));

                        timeOffset += ((double)frame.Duration / 1000);
                        xDoubleAnimation.KeyFrames.Add(xKeyFrame);
                        yDoubleAnimation.KeyFrames.Add(yKeyFrame);
                    }
                }

                animations.Add(animation.Name, new Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames>(xDoubleAnimation, yDoubleAnimation));
                xDoubleAnimation.Completed += XDoubleAnimation_Completed;
            }
        }
    }
}