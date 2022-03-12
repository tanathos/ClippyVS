using Recoding.ClippyVSPackage.Configurations;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Linq;
using System.Diagnostics;

namespace Recoding.ClippyVSPackage
{
    /// <summary>
    /// The core object that represents Clippy and its animations
    /// </summary>
    public class Clippy : AssistantBase
    {
        /// <summary>
        /// The URI for the sprite with all the animation stages for Clippy
        /// </summary>
        //private static string spriteResourceUri = "pack://application:,,,/ClippyVSPackage;component/clippy.png";
        private static readonly string SpriteResourceUri = "pack://application:,,,/ClippyVs2022;component/clippy.png";

        /// <summary>
        /// The URI for the animations json definition
        /// </summary>
        //private static string animationsResourceUri = "pack://application:,,,/ClippyVSPackage;component/animations.json";
        private static readonly string AnimationsResourceUri = "pack://application:,,,/ClippyVs2022;component/animations.json";

        /// <summary>
        /// The height of the frame
        /// </summary>
        public static int ClipHeight => 93;

        /// <summary>
        /// The with of the frame
        /// </summary>
        public static int ClipWidth => 124;

        /// <summary>
        /// The list of all the available animations
        /// </summary>
        public List<ClippyAnimation> AllAnimations { get; } = new List<ClippyAnimation>();

        /// <summary>
        /// The list of couples of Columns/Rows double animations
        /// </summary>
        private static Dictionary<string, Tuple<DoubleAnimationUsingKeyFrames, DoubleAnimationUsingKeyFrames>> _animations;

        /// <summary>
        /// All the animations that represents an Idle state
        /// </summary>
        private static readonly List<ClippyAnimation> IdleAnimations = new List<ClippyAnimation>() {
            ClippyAnimation.Idle11,
            ClippyAnimation.IdleRopePile,
            ClippyAnimation.IdleAtom,
            ClippyAnimation.IdleEyeBrowRaise,
            ClippyAnimation.IdleFingerTap,
            ClippyAnimation.IdleHeadScratch,
            ClippyAnimation.IdleSideToSide,
            ClippyAnimation.IdleSnooze };

        /// <summary>
        /// Default ctor
        /// </summary>
        public Clippy(Canvas canvas)
        {
            InitAssistant(canvas, SpriteResourceUri);

            if (_animations == null)
                RegisterAnimations();

            AllAnimations = new List<ClippyAnimation>();
            var values = Enum.GetValues(typeof(ClippyAnimation));
            AllAnimations.AddRange(values.Cast<ClippyAnimation>());
            RegisterIdleRandomAnimations();
        }

        /// <summary>
        /// Registers all the animation definitions into a static property
        /// </summary>
        private void RegisterAnimations()
        {
            _animations = RegisterAnimationsImpl(AnimationsResourceUri, XDoubleAnimation_Completed, ClipWidth, ClipHeight);
        }

        /// <summary>
        /// Callback to execute at the end of an animation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void XDoubleAnimation_Completed(object sender, EventArgs e)
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

        public void StartAnimation(ClippyAnimation animations, bool byPassCurrentAnimation = false)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.Run(
                async delegate
                {
                    await StartAnimationAsync(animations, byPassCurrentAnimation);
                });
        }

        /// <summary>
        /// Start a specific animation
        /// </summary>
        /// <param name="animationType"></param>
        /// <param name="byPassCurrentAnimation"></param>
        public async Task StartAnimationAsync(ClippyAnimation animationType, bool byPassCurrentAnimation = false)
        {
            if (!IsAnimating || byPassCurrentAnimation)
            {
                IsAnimating = true;
                await Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                if (_animations.ContainsKey(animationType.ToString()))
                {
                    ClippedImage.BeginAnimation(Canvas.LeftProperty, _animations[animationType.ToString()].Item1);
                    ClippedImage.BeginAnimation(Canvas.TopProperty, _animations[animationType.ToString()].Item2);
                } else
                {
                    Debug.WriteLine("Animation {0} not found!", animationType.ToString());
                }
            }

        }
    }
}
