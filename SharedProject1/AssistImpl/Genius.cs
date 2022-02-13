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
using Microsoft.VisualStudio.PlatformUI;
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
        public static int ClipHeight { get; } = 93;

        /// <summary>
        /// The with of the frame
        /// </summary>
        public static int ClipWidth { get; } = 124;

        /// <summary>
        /// The image that holds the sprite
        /// </summary>
        private readonly Image _clippedImage1;

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
        private static readonly List<GeniusAnimations> IdleAnimations = new List<GeniusAnimations>() {
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
            // ReSharper disable once RedundantAssignment
            var spResUri = SpriteResourceUri;
#if Dev19
            spResUri = SpriteResourceUri.Replace("ClippyVs2022", "ClippyVSPackage");
#endif
#if Dev22
#endif
            Sprite = new BitmapImage(new Uri(spResUri, UriKind.RelativeOrAbsolute));

            ClippedImage = new Image
            {
                Source = Sprite,
                Stretch = Stretch.None
            };

            _clippedImage1 = new Image
            {
                Source = Sprite,
                Stretch = Stretch.None
            };

            ClippedImage.Visibility = Visibility.Visible;
            _clippedImage1.Visibility = Visibility.Collapsed;

            if (canvas == null) return;

            canvas.Children.Clear();
            canvas.Children.Add(ClippedImage);
            //canvas.Effect = new BlurEffect();

            canvas1.Children.Clear();
            canvas1.Children.Add(_clippedImage1);
            //canvas1.Effect = new DropShadowEffect();

            canvas.AddPropertyChangeHandler(Canvas.LeftProperty, LeftPropChangedHandler);
            canvas1.AddPropertyChangeHandler(Canvas.LeftProperty, LeftPropChangedHandler);


            if (_animations == null)
                RegisterAnimations();

            //XX Requires testing..
            AllAnimations = new List<GeniusAnimations>();
            var values = Enum.GetValues(typeof(GeniusAnimations));
            AllAnimations.AddRange(values.Cast<GeniusAnimations>());
            RegisterIdleRandomAnimations();
        }

        private void LeftPropChangedHandler(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Registers all the animation definitions into a static property
        /// </summary>
        private void RegisterAnimations()
        {
            var storedAnimations = ParseAnimDescriptions();
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

                var frameIndex = 0;
                var animationMaxLayers = 0;
                foreach (var frame in animation.Frames)
                {
                    if (frame.ImagesOffsets != null)
                    {
                        var overlays = frame.ImagesOffsets.Count;
                        if (overlays > animationMaxLayers)
                        {
                            animationMaxLayers = overlays;
                        }

                        for (var layerNum = 0; layerNum < overlays; layerNum++)
                        {
                            Debug.WriteLine("Processing Overlay " + layerNum);

                            // Prepare Key frame for all potential layers (max 3)
                            xDoubleAnimation.KeyFrames.Add(new DiscreteDoubleKeyFrame());
                            yDoubleAnimation.KeyFrames.Add(new DiscreteDoubleKeyFrame());
                            xDoubleAnimation1.KeyFrames.Add(new DiscreteDoubleKeyFrame());
                            yDoubleAnimation1.KeyFrames.Add(new DiscreteDoubleKeyFrame());
                            xDoubleAnimation2.KeyFrames.Add(new DiscreteDoubleKeyFrame());
                            yDoubleAnimation2.KeyFrames.Add(new DiscreteDoubleKeyFrame());

                            //Overlay is actually - layers - displayed at the same time...
                            var lastCol = frame.ImagesOffsets[layerNum][0];
                            var lastRow = frame.ImagesOffsets[layerNum][1];

                            // X and Y
                            var xKeyFrame = new DiscreteDoubleKeyFrame(lastCol * -1, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(timeOffset)));
                            var yKeyFrame = new DiscreteDoubleKeyFrame(lastRow * -1, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(timeOffset)));

                            switch (layerNum)
                            {
                                case 0:
                                    xDoubleAnimation.KeyFrames.Insert(frameIndex, xKeyFrame);
                                    yDoubleAnimation.KeyFrames.Insert(frameIndex, yKeyFrame);
                                    break;
                                case 1:
                                    xDoubleAnimation1.KeyFrames.Insert(frameIndex, xKeyFrame);
                                    yDoubleAnimation1.KeyFrames.Insert(frameIndex, yKeyFrame);
                                    break;
                                case 2:
                                    xDoubleAnimation2.KeyFrames.Insert(frameIndex, xKeyFrame);
                                    yDoubleAnimation2.KeyFrames.Insert(frameIndex, yKeyFrame);
                                    break;
                            }
                        }

                        //timeOffset += ((double)frame.Duration / 1000 * 4);
                        timeOffset += ((double)frame.Duration / 1000);
                        frameIndex++;
                    }
                    else
                    {
                        Debug.WriteLine("ImageOffsets was null");
                    }
                }

                _animations.Add(animation.Name, new Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames>(xDoubleAnimation, yDoubleAnimation));
                _animations.Add1(animation.Name, new Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames>(xDoubleAnimation1, yDoubleAnimation1));
                _animations.Add2(animation.Name, new Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames>(xDoubleAnimation2, yDoubleAnimation2));
                _animations.AddLayers(animation.Name, animationMaxLayers);

                Debug.WriteLine("Added Genius Anim {0}" + animation.Name);
                Debug.WriteLine("...  Frame Count: " + xDoubleAnimation.KeyFrames.Count + " - " +
                                yDoubleAnimation.KeyFrames.Count);
                Debug.WriteLine($"Animation {animation.Name} has {animationMaxLayers} layers");

                xDoubleAnimation.Completed += XDoubleAnimation_Completed;
            }
        }

        private static List<GeniusSingleAnimation> ParseAnimDescriptions()
        {
            var spResUri = AnimationsResourceUri;

#if Dev19
            spResUri = spResUri.Replace("ClippyVs2022", "ClippyVSPackage");
#endif
            var uri = new Uri(spResUri, UriKind.RelativeOrAbsolute);

            var animJStream = Application.GetResourceStream(uri);

            if (animJStream == null)
                return null;

            // Can go to Constructor/Init
            var errors = new List<string>();

            var storedAnimations = DeserializeAnimations(animJStream, errors);
            return storedAnimations;
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

        /// <summary>
        /// Callback to execute at the end of an animation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void XDoubleAnimation_Completed(object sender, EventArgs e)
        {
            IsAnimating = false;
            ClippedImage.Visibility = Visibility.Visible;
            if (ClippedImage.Parent is Canvas canvas)
                canvas.Visibility = Visibility.Visible;

            _clippedImage1.Visibility = Visibility.Hidden;
            if (_clippedImage1.Parent is Canvas canvas1)
                canvas1.Visibility=Visibility.Hidden;
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

                    Debug.WriteLine("Triggering Genius " + animationType);
                    Debug.WriteLine(animation.KeyFrames.Item1.ToString() + animation.KeyFrames.Item2);

                    var animation1 = _animations.GetAnimation1(animationType.ToString());
                    var animLayers = _animations.GetAnimationLayerCnt(animationType.ToString());
                    IsAnimating = true;

                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    // well have to skip this (leave collapsed) if only one layer
                    if (animLayers > 1) {
                        _clippedImage1.Visibility = Visibility.Visible;
                        ((Canvas) _clippedImage1.Parent).Visibility = Visibility.Visible;
                    }
                    ClippedImage.BeginAnimation(Canvas.LeftProperty, animation.KeyFrames.Item1);
                    ClippedImage.BeginAnimation(Canvas.TopProperty, animation.KeyFrames.Item2);

                    _clippedImage1.BeginAnimation(Canvas.LeftProperty, animation1.KeyFrames.Item1);
                    _clippedImage1.BeginAnimation(Canvas.TopProperty, animation1.KeyFrames.Item2);
                }
            }
            catch (Exception)
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
        readonly Dictionary<string, int> _animationLayers;

        public OverlayAnimations(int capacity)
        {
            _animations0 =
                new Dictionary<string, OverlayAnimation>(capacity);
            _animations1 =
                new Dictionary<string, OverlayAnimation>(capacity);
            _animations2 =
                new Dictionary<string, OverlayAnimation>(capacity);
            _animationLayers = new Dictionary<string, int>(capacity);

        }

        public void Add(string animName, Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames> frames0)
        {
            _animations0.Add(animName, new OverlayAnimation(frames0));
        }

        public void Add1(string animName, Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames> frames0)
        {
            _animations1.Add(animName, new OverlayAnimation(frames0));
        }

        public void Add2(string animName, Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames> frames0)
        {
            _animations2.Add(animName, new OverlayAnimation(frames0));
        }


        public OverlayAnimation this[string animName] => _animations0[animName];
        public OverlayAnimation GetAnimation2(string animName) => _animations2[animName];
        public OverlayAnimation GetAnimation1(string animName) => _animations1[animName];
        public int GetAnimationLayerCnt(string animName) => _animationLayers[animName];

        internal void AddLayers(string name, int animationMaxLayers)
        {
            _animationLayers.Add(name, animationMaxLayers);
        }
    }

    public class OverlayAnimation
    {
        public readonly Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames> KeyFrames;

        public OverlayAnimation(Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames> xyKeyFrames)
        {
            KeyFrames = xyKeyFrames;
        }
    }
}
