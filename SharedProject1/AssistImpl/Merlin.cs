using Microsoft.VisualStudio.Shell;
using Recoding.ClippyVSPackage.Configurations;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Linq;
using Recoding.ClippyVSPackage;
using System.Diagnostics;

namespace SharedProject1.AssistImpl
{
    /// <summary>
    /// The core object that represents Clippy and its animations
    /// </summary>
    public class Merlin : AssistantBase
    {
        /// <summary>
        /// The URI for the sprite with all the animation stages for Clippy
        /// </summary>
        //private static string spriteResourceUri = "pack://application:,,,/ClippyVSPackage;component/clippy.png";
        private static readonly string SpriteResourceUri = "pack://application:,,,/ClippyVs2022;component/merlin_map.png";

        /// <summary>
        /// The URI for the animations json definition
        /// </summary>
        //private static string animationsResourceUri = "pack://application:,,,/ClippyVSPackage;component/animations.json";
        private static readonly string AnimationsResourceUri = "pack://application:,,,/ClippyVs2022;component/merlin_agent.js";

        /// <summary>
        /// The height of the frame
        /// </summary>
        public static int ClipHeight { get; set; } = 128;

        /// <summary>
        /// The with of the frame
        /// </summary>
        public static int ClipWidth { get; set; } = 128;

        /// <summary>
        /// The list of all the available animations
        /// </summary>
        public List<MerlinAnimations> AllAnimations { get; } = new List<MerlinAnimations>();

        /// <summary>
        /// The list of couples of Columns/Rows double animations
        /// </summary>
        private static Dictionary<string, Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames>> _animations;

        /// <summary>
        /// All the animations that represents an Idle state
        /// </summary>
        public static List<MerlinAnimations> IdleAnimations = new List<MerlinAnimations>() {
            MerlinAnimations.MoveLeft,
MerlinAnimations.Idle3_2,
MerlinAnimations.Idle3_1,
MerlinAnimations.Idle2_2,
MerlinAnimations.Idle2_1,
MerlinAnimations.Idle1_4,
MerlinAnimations.Idle1_1,
MerlinAnimations.Idle1_3,
MerlinAnimations.Idle1_2};


        /// <summary>
        /// Default ctor
        /// </summary>
        public Merlin(Panel canvas)
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

            if (_animations == null)
                RegisterAnimations();


            //XX Requires testing..
            AllAnimations = new List<MerlinAnimations>();
            var values = Enum.GetValues(typeof(MerlinAnimations));
            AllAnimations.AddRange(values.Cast<MerlinAnimations>());
            RegisterIdleRandomAnimations();
        }

        /// <summary>
        /// Registers all the animation definitions into a static property
        /// </summary>
        private void RegisterAnimations()
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

            _animations = new Dictionary<string, Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames>>();

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

                foreach (Recoding.ClippyVSPackage.Configurations.Frame frame in animation.Frames)
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
                    else
                    {
                        Debug.WriteLine("ImageOffsets was null");
                    }
                }

                _animations.Add(animation.Name, new Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames>(xDoubleAnimation, yDoubleAnimation));
                Debug.WriteLine("Added Merlin Anim {0}", animation.Name);

                xDoubleAnimation.Changed += XDoubleAnimation_Changed;
                xDoubleAnimation.Completed += XDoubleAnimation_Completed;
            }
        }

        private void XDoubleAnimation_Changed(object sender, EventArgs e)
        {
            //Debug.WriteLine("Merlin: Animation changing");
        }

        /// <summary>
        /// Callback to execute at the end of an animation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void XDoubleAnimation_Completed(object sender, EventArgs e)
        {
            IsAnimating = false;
        }

        /// <summary>
        /// Registers a function to perform a subset of animations randomly (the idle ones)
        /// </summary>
        private void RegisterIdleRandomAnimations()
        {
            WpfAnimationsDispatcher = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(IdleAnimationTimeout)
            };
            WpfAnimationsDispatcher.Tick += WPFAnimationsDispatcher_Tick;

            WpfAnimationsDispatcher.Start();
        }

        private void WPFAnimationsDispatcher_Tick(object sender, EventArgs e)
        {
            var rmd = new Random();
            var randomInt = rmd.Next(0, IdleAnimations.Count);

            StartAnimation(IdleAnimations[randomInt]);
        }

        public void StartAnimation(MerlinAnimations animations, bool byPassCurrentAnimation = false)
        {
            ThreadHelper.JoinableTaskFactory.Run(
                async delegate
                {
                    await StartAnimationAsync(animations, byPassCurrentAnimation);
                });
        }

        /// <summary>
        /// Start a specific animation
        /// </summary>
        /// <param name="animationType"></param>
        /// <param name="byPassCurrentAnimation">   </param>
        public async System.Threading.Tasks.Task StartAnimationAsync(MerlinAnimations animationType, bool byPassCurrentAnimation = false)
        {
            try
            {
                if (!IsAnimating || byPassCurrentAnimation)
                {
                    var animation = _animations[animationType.ToString()];
                    if (animation == null) return;

                    Debug.WriteLine("Triggering Merlin " + animationType);
                    Debug.WriteLine(animation.Item1.ToString() + animation.Item2);
                    IsAnimating = true;
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    ClippedImage.BeginAnimation(Canvas.LeftProperty, animation.Item1);
                    ClippedImage.BeginAnimation(Canvas.TopProperty, animation.Item2);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("StartAnimAsyncException Merlin " + animationType);
            }

        }
    }
}
