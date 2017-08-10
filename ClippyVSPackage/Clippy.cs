using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Recoding.ClippyVSPackage.Configurations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Threading;

namespace Recoding.ClippyVSPackage
{
    /// <summary>
    /// The core object that represents Clippy and its animations
    /// </summary>
    public class Clippy
    {
        /// <summary>
        /// The URI for the sprite with all the animation stages for Clippy
        /// </summary>
        private static string spriteResourceUri = "pack://application:,,,/ClippyVSPackage;component/resources/clippy.png";

        /// <summary>
        /// The URI for the animations json definition
        /// </summary>
        private static string animationsResourceUri = "pack://application:,,,/ClippyVSPackage;component/resources/animations.json";

        /// <summary>
        /// The sprite with all the animation stages for Clippy
        /// </summary>
        public BitmapSource Sprite;

        /// <summary>
        /// The actual Clippy container that works as a clipping mask
        /// </summary>
        public Canvas ClippyCanvas { get; private set; }

        /// <summary>
        /// The image that holds the sprite
        /// </summary>
        public Image clippedImage;

        /// <summary>
        /// The with of the frame
        /// </summary>
        public static int ClipWidth = 124;

        /// <summary>
        /// The height of the frame
        /// </summary>
        public static int ClipHeight = 93;

        private static int IdleAnimationTimeout = 45;

        public bool IsAnimating { get; set; }

        /// <summary>
        /// The list of couples of Columns/Rows double animations
        /// </summary>
        private static Dictionary<string, Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames>> WPFAnimations;

        /// <summary>
        /// All the animations that represents an Idle state
        /// </summary>
        public static List<ClippyAnimationTypes> IdleAnimations = new List<ClippyAnimationTypes>() { 
            ClippyAnimationTypes.Idle1_1, 
            ClippyAnimationTypes.IdleRopePile, 
            ClippyAnimationTypes.IdleAtom, 
            ClippyAnimationTypes.IdleEyeBrowRaise, 
            ClippyAnimationTypes.IdleFingerTap, 
            ClippyAnimationTypes.IdleHeadScratch, 
            ClippyAnimationTypes.IdleSideToSide, 
            ClippyAnimationTypes.IdleSnooze };

        /// <summary>
        /// The time dispatcher to perform the animations in a random way
        /// </summary>
        private DispatcherTimer WPFAnimationsDispatcher;


        private EnvDTE80.DTE2 dte;

        EnvDTE.Events events;

        EnvDTE.DocumentEventsClass docEvents;

        EnvDTE.BuildEventsClass buildEvents;

        /// <summary>
        /// Default ctor
        /// </summary>
        public Clippy(Canvas canvas)
        {
            this.Sprite = new BitmapImage(new Uri(spriteResourceUri, UriKind.RelativeOrAbsolute));

            clippedImage = new System.Windows.Controls.Image();
            clippedImage.Source = Sprite;
            clippedImage.Stretch = Stretch.None;

            canvas.Children.Clear();
            canvas.Children.Add(clippedImage);

            if (WPFAnimations == null)
                RegisterAnimations();

            dte = (EnvDTE80.DTE2)Package.GetGlobalService(typeof(SDTE));
            events = dte.Events;
            docEvents = (EnvDTE.DocumentEventsClass)dte.Events.DocumentEvents;
            buildEvents = (EnvDTE.BuildEventsClass)dte.Events.BuildEvents;

            RegisterToDTEEvents();

            RegisterIdleRandomAnimations();
        }

        /// <summary>
        /// Registers all the animation definitions into a static property
        /// </summary>
        private void RegisterAnimations()
        {
            Uri uri = new Uri(animationsResourceUri, UriKind.RelativeOrAbsolute);
            StreamResourceInfo info = Application.GetResourceStream(uri);

            List<Animation> storedAnimations = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Animation>>(StreamToString(info.Stream));

            WPFAnimations = new Dictionary<string, Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames>>();

            foreach (Animation animation in storedAnimations)
            {
                DoubleAnimationUsingKeyFrames xDoubleAnimation = new DoubleAnimationUsingKeyFrames();
                xDoubleAnimation.FillBehavior = FillBehavior.HoldEnd;

                DoubleAnimationUsingKeyFrames yDoubleAnimation = new DoubleAnimationUsingKeyFrames();
                yDoubleAnimation.FillBehavior = FillBehavior.HoldEnd;

                int lastCol = 0;
                int lastRow = 0;
                double timeOffset = 0;

                foreach (Recoding.ClippyVSPackage.Configurations.Frame frame in animation.Frames)
                {
                    if (frame.ImagesOffsets != null)
                    {
                        lastCol = frame.ImagesOffsets.Column;
                        lastRow = frame.ImagesOffsets.Row;
                    }

                    // X
                    DiscreteDoubleKeyFrame xKeyFrame = new DiscreteDoubleKeyFrame(ClipWidth * -lastCol, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(timeOffset)));

                    // Y
                    DiscreteDoubleKeyFrame yKeyFrame = new DiscreteDoubleKeyFrame(ClipHeight * -lastRow, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(timeOffset)));

                    timeOffset += ((double)frame.Duration / 1000);

                    xDoubleAnimation.KeyFrames.Add(xKeyFrame);
                    yDoubleAnimation.KeyFrames.Add(yKeyFrame);
                }

                WPFAnimations.Add(animation.Name, new Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames>(xDoubleAnimation, yDoubleAnimation));

                xDoubleAnimation.Completed += xDoubleAnimation_Completed;
            }
        }

        void xDoubleAnimation_Completed(object sender, EventArgs e)
        {
            IsAnimating = false;
        }

        /// <summary>
        /// Registers a function to perform a subset of animations randomly
        /// </summary>
        private void RegisterIdleRandomAnimations() 
        {
            WPFAnimationsDispatcher = new DispatcherTimer();
            WPFAnimationsDispatcher.Interval = TimeSpan.FromSeconds(IdleAnimationTimeout);
            WPFAnimationsDispatcher.Tick += WPFAnimationsDispatcher_Tick;

            WPFAnimationsDispatcher.Start();
        }

        void WPFAnimationsDispatcher_Tick(object sender, EventArgs e)
        {
            Random rmd = new Random();
            int random_int = rmd.Next(0, IdleAnimations.Count);

            StartAnimation(IdleAnimations[random_int]);
        }

        /// <summary>
        /// Start a specific animation
        /// </summary>
        /// <param name="animationType"></param>
        public void StartAnimation(ClippyAnimationTypes animationType, bool byPassCurrentAnimation = false)
        {
            if (!IsAnimating || byPassCurrentAnimation) 
            {
                IsAnimating = true;

                clippedImage.BeginAnimation(Canvas.LeftProperty, WPFAnimations[animationType.ToString()].Item1);
                clippedImage.BeginAnimation(Canvas.TopProperty, WPFAnimations[animationType.ToString()].Item2);
            }
        }

        private void RegisterToDTEEvents()
        {
            docEvents.DocumentOpening += DocumentEvents_DocumentOpening;
            docEvents.DocumentSaved += DocumentEvents_DocumentSaved;
            buildEvents.OnBuildBegin += buildEvents_OnBuildBegin;
        }

        void buildEvents_OnBuildBegin(EnvDTE.vsBuildScope Scope, EnvDTE.vsBuildAction Action)
        {
            StartAnimation(ClippyAnimationTypes.Processing, true);
        }

        void DocumentEvents_DocumentSaved(EnvDTE.Document Document)
        {
            StartAnimation(ClippyAnimationTypes.Save, true);
        }

        void DocumentEvents_DocumentOpening(string DocumentPath, bool ReadOnly)
        {
            StartAnimation(ClippyAnimationTypes.LookUp);
        }

        /// <summary>
        /// Reads the content of a stream into a string
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        private static string StreamToString(Stream stream)
        {
            stream.Position = 0;
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
