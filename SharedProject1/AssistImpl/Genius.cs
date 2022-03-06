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
using Frame = Recoding.ClippyVSPackage.Configurations.Legacy.Frame;

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
        private static LayeredAnimations _animations;

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

            _animations = new LayeredAnimations(storedAnimations.Count);

            foreach (var animation in storedAnimations)
            {
                RegisterAnimation(animation);
            }
        }

        private void RegisterAnimation(GeniusSingleAnimation animation)
        {
            var xDoubleAnimation = new DoubleAnimationUsingKeyFrames
            {
                FillBehavior = FillBehavior.HoldEnd
            };
            var yDoubleAnimation = new DoubleAnimationUsingKeyFrames
            {
                FillBehavior = FillBehavior.HoldEnd
            };
            var visibility0 = new ObjectAnimationUsingKeyFrames();

            var xDoubleAnimation1 = new DoubleAnimationUsingKeyFrames
            {
                FillBehavior = FillBehavior.HoldEnd
            };
            var yDoubleAnimation1 = new DoubleAnimationUsingKeyFrames
            {
                FillBehavior = FillBehavior.HoldEnd
            };
            var visibility1 = new ObjectAnimationUsingKeyFrames();

            var xDoubleAnimation2 = new DoubleAnimationUsingKeyFrames
            {
                FillBehavior = FillBehavior.HoldEnd
            };
            var yDoubleAnimation2 = new DoubleAnimationUsingKeyFrames
            {
                FillBehavior = FillBehavior.HoldEnd
            };
            var visibility2 = new ObjectAnimationUsingKeyFrames();

            double timeOffset = 0;
            var frameIndex = 0;
            var animationMaxLayers = 0;

            foreach (var frame in animation.Frames)
            {
                animationMaxLayers = RegisterFrame(frame, animationMaxLayers, xDoubleAnimation, yDoubleAnimation, xDoubleAnimation1, yDoubleAnimation1, xDoubleAnimation2, yDoubleAnimation2, visibility0, visibility1, visibility2, ref timeOffset, ref frameIndex);
            }

            _animations.Add(animation.Name,
                new Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames>(xDoubleAnimation, yDoubleAnimation),
                visibility0,
                new Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames>(xDoubleAnimation1, yDoubleAnimation1),
                visibility1,
                animationMaxLayers);

            Debug.WriteLine("Added Genius Anim {0}" + animation.Name);
            Debug.WriteLine("...  Frame Count: " + xDoubleAnimation.KeyFrames.Count + " - " +
                            yDoubleAnimation.KeyFrames.Count);
            Debug.WriteLine($"Animation {animation.Name} has {animationMaxLayers} layers");

            xDoubleAnimation.Completed += XDoubleAnimation_Completed;
        }

        private static int RegisterFrame(Frame frame, int animationMaxLayers, DoubleAnimationUsingKeyFrames xDoubleAnimation,
            DoubleAnimationUsingKeyFrames yDoubleAnimation, DoubleAnimationUsingKeyFrames xDoubleAnimation1,
            DoubleAnimationUsingKeyFrames yDoubleAnimation1, DoubleAnimationUsingKeyFrames xDoubleAnimation2,
            DoubleAnimationUsingKeyFrames yDoubleAnimation2, ObjectAnimationUsingKeyFrames visibility0, ObjectAnimationUsingKeyFrames visibility1, ObjectAnimationUsingKeyFrames visibility2, ref double timeOffset, ref int frameIndex)
        {
            if (frame.ImagesOffsets != null)
            {
                if (frame.ImagesOffsets.Count > animationMaxLayers)
                {
                    animationMaxLayers = frame.ImagesOffsets.Count;
                }

                for (var layerNum = 0; layerNum < frame.ImagesOffsets.Count; layerNum++)
                {
                    Debug.WriteLine("Processing Overlay " + layerNum);

                    // Prepare Key frame for all potential layers (max 3)
                    xDoubleAnimation.KeyFrames.Add(new DiscreteDoubleKeyFrame());
                    yDoubleAnimation.KeyFrames.Add(new DiscreteDoubleKeyFrame());
                    visibility0.KeyFrames.Add(new DiscreteObjectKeyFrame(0.0));
                    xDoubleAnimation1.KeyFrames.Add(new DiscreteDoubleKeyFrame());
                    yDoubleAnimation1.KeyFrames.Add(new DiscreteDoubleKeyFrame());
                    visibility1.KeyFrames.Add(new DiscreteObjectKeyFrame(0.0));
                    xDoubleAnimation2.KeyFrames.Add(new DiscreteDoubleKeyFrame());
                    yDoubleAnimation2.KeyFrames.Add(new DiscreteDoubleKeyFrame());
                    visibility2.KeyFrames.Add(new DiscreteObjectKeyFrame(0.0));

                    //Overlay is actually - layers - displayed at the same time...
                    var lastCol = frame.ImagesOffsets[layerNum][0];
                    var lastRow = frame.ImagesOffsets[layerNum][1];

                    // X and Y
                    var frameKeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(timeOffset));
                    var xKeyFrame = new DiscreteDoubleKeyFrame(lastCol * -1,
                        frameKeyTime);
                    var yKeyFrame = new DiscreteDoubleKeyFrame(lastRow * -1,
                        frameKeyTime);

                    switch (layerNum)
                    {
                        case 0:
                            xDoubleAnimation.KeyFrames.Insert(frameIndex, xKeyFrame);
                            yDoubleAnimation.KeyFrames.Insert(frameIndex, yKeyFrame);
                            visibility0.KeyFrames.Insert(frameIndex, new DiscreteObjectKeyFrame(1.0, frameKeyTime));
                            break;
                        case 1:
                            xDoubleAnimation1.KeyFrames.Insert(frameIndex, xKeyFrame);
                            yDoubleAnimation1.KeyFrames.Insert(frameIndex, yKeyFrame);
                            visibility1.KeyFrames.Insert(frameIndex, new DiscreteObjectKeyFrame(1.0, frameKeyTime));
                            break;
                        case 2:
                            xDoubleAnimation2.KeyFrames.Insert(frameIndex, xKeyFrame);
                            yDoubleAnimation2.KeyFrames.Insert(frameIndex, yKeyFrame);
                            visibility2.KeyFrames.Insert(frameIndex, new DiscreteObjectKeyFrame(1.0, frameKeyTime));
                            break;
                    }
                }

                //timeOffset += ((double)frame.Duration / 1000 * 4);
                timeOffset += ((double) frame.Duration / 1000);
                frameIndex++;
            }
            else
            {
                Debug.WriteLine("ImageOffsets was null");
            }

            return animationMaxLayers;
        }

        private void YKeyFrame_Changed(object sender, EventArgs e)
        {
            Debug.WriteLine("Keyframe changed!");
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
                    Debug.WriteLine(animation.Layer0.Item1.ToString() + animation.Layer0.Item2);

                    var animLayers = animation.MaxLayers;
                    IsAnimating = true;

                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    // well have to skip this (leave collapsed) if only one layer
                    if (animLayers > 1) {
                        _clippedImage1.Visibility = Visibility.Visible;
                        ((Canvas) _clippedImage1.Parent).Visibility = Visibility.Visible;
                    }
                    ClippedImage.BeginAnimation(Canvas.LeftProperty, animation.Layer0.Item1);
                    ClippedImage.BeginAnimation(Canvas.TopProperty, animation.Layer0.Item2);
                    

                    _clippedImage1.BeginAnimation(Canvas.LeftProperty, animation.Layer1.Item1);
                    _clippedImage1.BeginAnimation(Canvas.TopProperty, animation.Layer1.Item2);
                    _clippedImage1.BeginAnimation(Image.OpacityProperty, animation.Visibility1);
                }
            }
            catch (Exception)
            {
                Debug.WriteLine("StartAnimAsyncException Genius " + animationType);
            }
        }
    }

    public class LayeredAnimation
    {
        public readonly string Name;
        public readonly Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames> Layer0;
        public readonly ObjectAnimationUsingKeyFrames Visibility0;
        public readonly Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames> Layer1;
        public readonly ObjectAnimationUsingKeyFrames Visibility1;
        public readonly int MaxLayers;

        public LayeredAnimation(string name, Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames> layer0,
            ObjectAnimationUsingKeyFrames visibility0, 
            Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames> layer1,
            ObjectAnimationUsingKeyFrames visibility1,
            int animMaxLayers)
        {
            Name = name;
            Layer0 = layer0;
            Visibility0 = visibility0;
            Layer1 = layer1;
            Visibility1 = visibility1;
            MaxLayers = animMaxLayers;
            
        }
    }

    public class LayeredAnimations
    {
        private readonly List<LayeredAnimation> _animations;

        public LayeredAnimations(int capacity)
        {
            _animations = new List<LayeredAnimation>(capacity);
        }

        public void Add(string animName, Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames> layer0, ObjectAnimationUsingKeyFrames visibility0,
            Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames> layer1, ObjectAnimationUsingKeyFrames visibility1,
            int animMaxLayers)
        {
            _animations.Add(new LayeredAnimation(animName, layer0, visibility0, layer1, visibility1, animMaxLayers));
            Debug.WriteLine("Added animation");
        }

        public LayeredAnimation this[string animName]
        {
            get
            {
                return _animations.First(animation => animation.Name.Equals(animName));
            }
        }
    }

    //public class OverlayAnimationFrames
    //{
    //    public readonly Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames> KeyFrames;

    //    public OverlayAnimationFrames(Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames> xyKeyFrames)
    //    {
    //        KeyFrames = xyKeyFrames;
    //    }
    //}
}
