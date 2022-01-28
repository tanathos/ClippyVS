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
using System.Windows.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Recoding.ClippyVSPackage.Configurations.Legacy;

namespace SharedProject1.AssistImpl
{
    /// <summary>
    /// The core object that represents Clippy and its animations
    /// </summary>
    public class Genius : AssistantBase
    {
        /// <summary>
        /// The URI for the sprite with all the animation stages for Clippy
        /// </summary>
        //private static string spriteResourceUri = "pack://application:,,,/ClippyVSPackage;component/clippy.png";
        private static readonly string SpriteResourceUri = "pack://application:,,,/ClippyVs2022;component/genius_map.png";

        /// <summary>
        /// The URI for the animations json definition
        /// </summary>
        //private static string animationsResourceUri = "pack://application:,,,/ClippyVSPackage;component/animations.json";
        private static readonly string AnimationsResourceUri = "pack://application:,,,/ClippyVs2022;component/Genius.json";

        /// <summary>
        /// The height of the frame
        /// </summary>
        public static int ClipHeight { get; set; } = 93;

        /// <summary>
        /// The with of the frame
        /// </summary>
        public static int ClipWidth { get; set; } = 124;

        /// <summary>
        /// The image that holds the sprite
        /// </summary>
        protected Image ClippedImage1;

        /// <summary>
        /// The list of all the available animations
        /// </summary>
        public List<GeniusAnimations> AllAnimations { get; } = new List<GeniusAnimations>();

        /// <summary>
        /// The list of couples of Columns/Rows double animations , supports no overlays
        /// </summary>
        private static OverlayAnimations _animations;

        /// <summary>
        /// All the animations that represents an Idle state
        /// </summary>
        public static List<GeniusAnimations> IdleAnimations = new List<GeniusAnimations>() {
GeniusAnimations.Idle0,
GeniusAnimations.Idle1,
GeniusAnimations.Idle2,
GeniusAnimations.Idle3,
GeniusAnimations.Idle4,
GeniusAnimations.Idle5,
GeniusAnimations.Idle6,
GeniusAnimations.Idle7,
GeniusAnimations.Idle8,
GeniusAnimations.Idle9};


        /// <summary>
        /// Default ctor
        /// </summary>
        public Genius(Panel canvas, Panel canvas1)
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

            ClippedImage1 = new Image
            {
                Source = Sprite,
                Stretch = Stretch.None
            };

            ClippedImage.Visibility = Visibility.Visible;
            ClippedImage1.Visibility = Visibility.Visible;

            if (canvas == null) return;

            canvas.Children.Clear();
            canvas.Children.Add(ClippedImage);

            canvas1.Children.Clear();
            canvas1.Children.Add(ClippedImage1);


            if (_animations == null)
                RegisterAnimations();


            //XX Requires testing..
            AllAnimations = new List<GeniusAnimations>();
            var values = Enum.GetValues(typeof(GeniusAnimations));
            AllAnimations.AddRange(values.Cast<GeniusAnimations>());
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

            var animJStream = Application.GetResourceStream(uri);

            if (animJStream == null)
                return;

            // Can go to Constructor/Init
            List<string> errors = new List<string>();

            var storedAnimations = DeserializeAnimations(animJStream, errors);
            if (storedAnimations == null) return;

            _animations = new OverlayAnimations(storedAnimations.Count);

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
                var xDoubleAnimation1 = new DoubleAnimationUsingKeyFrames
                {
                    FillBehavior = FillBehavior.HoldEnd
                };

                var yDoubleAnimation1 = new DoubleAnimationUsingKeyFrames
                {
                    FillBehavior = FillBehavior.HoldEnd
                };
                var xDoubleAnimation2 = new DoubleAnimationUsingKeyFrames
                {
                    FillBehavior = FillBehavior.HoldEnd
                };

                var yDoubleAnimation2 = new DoubleAnimationUsingKeyFrames
                {
                    FillBehavior = FillBehavior.HoldEnd
                };

                double timeOffset = 0;

                var idx = 0;
                foreach (Recoding.ClippyVSPackage.Configurations.Legacy.Frame frame in animation.Frames)
                {
                    if (frame.ImagesOffsets != null)
                    {
                        //var i = 0; // get rid of this once overlay processing is implemented
                        var overlays = frame.ImagesOffsets.Count;
                        for (int i = 0; i < overlays; i++)
                        {
                            Debug.WriteLine("Processing Overlay " + i);
                            Debug.WriteLine("Overlay is actually - layers - displayed at the same time...");

                            var lastCol = frame.ImagesOffsets[i][0];
                            var lastRow = frame.ImagesOffsets[i][1];

                            // X
                            var xKeyFrame = new DiscreteDoubleKeyFrame(lastCol * -1, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(timeOffset)));

                            // Y
                            var yKeyFrame = new DiscreteDoubleKeyFrame(lastRow * -1, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(timeOffset)));

                            // Sendmail is f...cked.... fix, reverse engineer or something.
                            // XXXX Remove slowdown
                            //timeOffset += ((double)frame.Duration / 1000 * 4);
                            
                            xDoubleAnimation.KeyFrames.Add(new DiscreteDoubleKeyFrame());
                            yDoubleAnimation.KeyFrames.Add(new DiscreteDoubleKeyFrame());
                            xDoubleAnimation1.KeyFrames.Add(new DiscreteDoubleKeyFrame());
                            yDoubleAnimation1.KeyFrames.Add(new DiscreteDoubleKeyFrame());
                            xDoubleAnimation2.KeyFrames.Add(new DiscreteDoubleKeyFrame());
                            yDoubleAnimation2.KeyFrames.Add(new DiscreteDoubleKeyFrame());

                            switch (i)
                            {
                                case 0:
                                    xDoubleAnimation.KeyFrames.Insert(idx, xKeyFrame);
                                    yDoubleAnimation.KeyFrames.Insert(idx, yKeyFrame);
                                    break;
                                case 1:
                                    xDoubleAnimation1.KeyFrames.Insert(idx,xKeyFrame);
                                    yDoubleAnimation1.KeyFrames.Insert(idx,yKeyFrame);
                                    break;
                                case 2:
                                    xDoubleAnimation2.KeyFrames.Insert(idx,xKeyFrame);
                                    yDoubleAnimation2.KeyFrames.Insert(idx, yKeyFrame);
                                    break;

                            }
                        }
                        timeOffset += ((double)frame.Duration / 1000);

                        idx++;
                    }
                    else
                    {
                        Debug.WriteLine("ImageOffsets was null");
                    }
                }

                _animations.Add(animation.Name, new Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames>(xDoubleAnimation, yDoubleAnimation));
                _animations.Add1(animation.Name, new Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames>(xDoubleAnimation1, yDoubleAnimation1));
                _animations.Add2(animation.Name, new Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames>(xDoubleAnimation2, yDoubleAnimation2));
                Debug.WriteLine("Added Genius Anim {0}" + animation.Name);
                Debug.WriteLine("...  Frame Count: " + xDoubleAnimation.KeyFrames.Count + " - " +
                                yDoubleAnimation.KeyFrames.Count);

                xDoubleAnimation.Changed += XDoubleAnimation_Changed;
                xDoubleAnimation.Completed += XDoubleAnimation_Completed;
            }
        }

        private static List<GeniusSingleAnimation> DeserializeAnimations(StreamResourceInfo animJStream, List<string> errors)
        {
            var storedAnimations =
                JsonConvert.DeserializeObject<List<GeniusSingleAnimation>>(StreamToString(animJStream.Stream),
                    new JsonSerializerSettings
                    {
                        Error = delegate (object sender, ErrorEventArgs args)
                        {
                            errors.Add(args.ErrorContext.Error.Message);
                            args.ErrorContext.Handled = true;
                        },
                        MissingMemberHandling = MissingMemberHandling.Error,
                        NullValueHandling = NullValueHandling.Include
                    });
            return storedAnimations;
        }

        private void XDoubleAnimation_Changed(object sender, EventArgs e)
        {
            Debug.WriteLine("Genius: Animation changing");
        }

        /// <summary>
        /// Callback to execute at the end of an animation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void XDoubleAnimation_Completed(object sender, EventArgs e)
        {
            IsAnimating = false;
            ClippedImage.Visibility = Visibility.Visible;
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

        public void StartAnimation(GeniusAnimations animations, bool byPassCurrentAnimation = false)
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
        public async System.Threading.Tasks.Task StartAnimationAsync(GeniusAnimations animationType, bool byPassCurrentAnimation = false)
        {
            try
            {
                if (!IsAnimating || byPassCurrentAnimation)
                {
                    var animation = _animations[animationType.ToString()];
                    if (animation == null) return;

                    var animation1 = _animations.GetAnimation1(animationType.ToString());
                    Debug.WriteLine("Triggering Genius " + animationType);
                    Debug.WriteLine(animation._keyFrames.Item1.ToString() + animation._keyFrames.Item2);
                    IsAnimating = true;
                    ClippedImage1.Visibility = Visibility.Visible;
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    ClippedImage.BeginAnimation(Canvas.LeftProperty, animation._keyFrames.Item1);
                    ClippedImage.BeginAnimation(Canvas.TopProperty, animation._keyFrames.Item2);

                    
                    ClippedImage1.BeginAnimation(Canvas.LeftProperty, animation1._keyFrames.Item1);
                    ClippedImage1.BeginAnimation(Canvas.TopProperty, animation1._keyFrames.Item2);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("StartAnimAsyncException Genius " + animationType);
            }

        }
    }

    public class OverlayAnimations
    {
        readonly Dictionary<string, OverlayAnimation> _animations0;
        readonly Dictionary<string, OverlayAnimation> _animations1;
        readonly Dictionary<string, OverlayAnimation> _animations2;

        public OverlayAnimations(int capacity)
        {
            _animations0 =
                new Dictionary<string, OverlayAnimation>(capacity);
            _animations1 =
                new Dictionary<string, OverlayAnimation>(capacity);
            _animations2 =
                new Dictionary<string, OverlayAnimation>(capacity);

        }

        public void Add(string animName, Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames> frames0)
        {
            _animations0.Add(animName, new OverlayAnimation(animName, frames0));
        }

        public void Add1(string animName, Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames> frames0)
        {
            _animations1.Add(animName, new OverlayAnimation(animName, frames0));
        }

        public void Add2(string animName, Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames> frames0)
        {
            _animations2.Add(animName, new OverlayAnimation(animName, frames0));
        }


        public OverlayAnimation this[string animName] => _animations0[animName];
        public OverlayAnimation GetAnimation2(string animName) => _animations2[animName];
        public OverlayAnimation GetAnimation1(string animName) => _animations1[animName];
    }

    public class OverlayAnimation
    {
        private string _animationName;
        public Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames> _keyFrames;
        
        public OverlayAnimation(string animName, Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames> xyKeyFrames)
        {
            _animationName = animName;
            _keyFrames = xyKeyFrames;
        }
    }
}
