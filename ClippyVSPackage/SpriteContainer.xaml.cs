﻿using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.Settings;
using Recoding.ClippyVSPackage.Configurations;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Recoding.ClippyVSPackage
{
    /// <summary>
    /// Interaction logic for SpriteContainer.xaml
    /// </summary>
    public partial class SpriteContainer : System.Windows.Window
    {
        /// <summary>
        /// The instance of Clippy, our hero
        /// </summary>
        private Clippy _clippy { get; set; }

        private Balloon _balloon = null;

        /// <summary>
        /// This VSIX package
        /// </summary>
        private AsyncPackage _package;

        /// <summary>
        /// The settings store of this package, to save preferences about the extension
        /// </summary>
        private WritableSettingsStore _userSettingsStore;

        private double RelativeLeft { get; set; }

        private double RelativeTop { get; set; }

        private Events events;
        private DocumentEvents docEvents;
        private BuildEvents buildEvents;
        private ProjectItemsEvents projectItemsEvents;
        private ProjectItemsEvents csharpProjectItemsEvents;

        /// <summary>
        /// Default ctor
        /// </summary>
        /// <param name="package"></param>
        public SpriteContainer(AsyncPackage package)
        {
            _package = package;

            ThreadHelper.ThrowIfNotOnUIThread();
            var settingsManager = new ShellSettingsManager(ServiceProvider.GlobalProvider);
            _userSettingsStore = settingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);

            InitializeComponent();

            this.Owner = Application.Current.MainWindow;
            this.Topmost = false;

            #region Register event handlers
            if (package == null)
                throw new ArgumentException("Package was null");

            IVsActivityLog activityLog = package.GetServiceAsync(typeof(SVsActivityLog))
                .ConfigureAwait(true).GetAwaiter().GetResult() as IVsActivityLog;
            //if (activityLog == null) return;
            //System.Windows.Forms.MessageBox.Show("Found the activity log service.");
            ThreadHelper.ThrowIfNotOnUIThread();
            DTE dte = (DTE)package.GetServiceAsync(typeof(DTE)).ConfigureAwait(true).GetAwaiter().GetResult();
            events = dte.Events;
            docEvents = dte.Events.DocumentEvents;
            buildEvents = dte.Events.BuildEvents;

            RegisterToDTEEvents();

            Owner.LocationChanged += Owner_LocationChanged;
            Owner.StateChanged += Owner_StateOrSizeChanged;
            Owner.SizeChanged += Owner_StateOrSizeChanged;

            LocationChanged += SpriteContainer_LocationChanged;

            #endregion

            #region -- Restore Sprite postion --

            double? storedRelativeTop = null;
            double? storedRelativeLeft = null;

            double relativeTop = 0;
            double relativeLeft = 0;

            try
            {
                if (_userSettingsStore.PropertyExists(Constants.SettingsCollectionPath, nameof(RelativeTop)))
                    storedRelativeTop = double.Parse(_userSettingsStore.GetString(Constants.SettingsCollectionPath, nameof(RelativeTop)), CultureInfo.InvariantCulture);

                if (_userSettingsStore.PropertyExists(Constants.SettingsCollectionPath, nameof(RelativeLeft)))
                    storedRelativeLeft = Double.Parse(_userSettingsStore.GetString(Constants.SettingsCollectionPath, nameof(RelativeLeft)), CultureInfo.InvariantCulture);
            }
            catch (Exception e)
            {
                Debug.Fail(e.Message);
            }

            if (!storedRelativeTop.HasValue || !storedRelativeLeft.HasValue)
            {
                recalculateSpritePosition(out relativeTop, out relativeLeft, true);
                storedRelativeTop = relativeTop;
                storedRelativeLeft = relativeLeft;

                this.RelativeLeft = relativeLeft;
                this.RelativeTop = relativeTop;

                storeRelativeSpritePosition(storedRelativeTop.Value, storedRelativeLeft.Value);
            }
            else
            {
                recalculateSpritePosition(out relativeTop, out relativeLeft);

                this.RelativeLeft = relativeLeft;
                this.RelativeTop = relativeTop;
            }

            double ownerTop = this.Owner.Top;
            double ownerLeft = this.Owner.Left;

            if (this.Owner.WindowState == WindowState.Maximized)
            {
                ownerTop = 0;
                ownerLeft = 0;
            }

            this.Top = ownerTop + storedRelativeTop.Value;
            this.Left = ownerLeft + storedRelativeLeft.Value;

            #endregion

            var values = Enum.GetValues(typeof(ClippyAnimation));

            //// TEMP: create a voice for each animation in the context menu
            //var pMenu = (ContextMenu)this.Resources["cmButton"];

            //foreach (ClippySingleAnimation val in values)
            //{
            //    var menuItem = new MenuItem()
            //    {
            //        Header = val.ToString(),
            //        Name = "cmd" + val.ToString()
            //    };
            //    menuItem.Click += cmdTestAnimation_Click;
            //    pMenu.Items.Add(menuItem);
            //}
            //// /TEMP

            _clippy = new Clippy((Canvas)FindName("ClippyCanvas"));
            _clippy.StartAnimation(ClippyAnimation.Idle1_1);

            // Initialize balloon window
            if (_balloon == null)
            {
                _balloon = new Balloon();
            }
        }

        private void RegisterToDTEEvents()
        {
            docEvents.DocumentOpening += DocumentEvents_DocumentOpening;
            docEvents.DocumentSaved += DocumentEvents_DocumentSaved;
            docEvents.DocumentClosing += DocEvents_DocumentClosing;

            buildEvents.OnBuildBegin += BuildEvents_OnBuildBegin;
            buildEvents.OnBuildDone += BuildEvents_OnBuildDone;

            ThreadHelper.ThrowIfNotOnUIThread();

            DTE dte = _package.GetServiceAsync(typeof(DTE)).ConfigureAwait(true).GetAwaiter().GetResult() as EnvDTE.DTE;


            Events2 events2 = dte.Events as Events2;
            if (events2 != null)
            {
                this.projectItemsEvents = events2.ProjectItemsEvents;
                this.projectItemsEvents.ItemAdded += ProjectItemsEvents_ItemAdded;
                this.projectItemsEvents.ItemRemoved += ProjectItemsEvents_ItemRemoved;
                this.projectItemsEvents.ItemRenamed += ProjectItemsEvents_ItemRenamed;
            }

            this.csharpProjectItemsEvents = dte.Events.GetObject("CSharpProjectItemsEvents") as EnvDTE.ProjectItemsEvents;
            if (this.csharpProjectItemsEvents != null)
            {
                this.csharpProjectItemsEvents.ItemAdded += ProjectItemsEvents_ItemAdded;
                this.csharpProjectItemsEvents.ItemRemoved += ProjectItemsEvents_ItemRemoved;
                this.csharpProjectItemsEvents.ItemRenamed += ProjectItemsEvents_ItemRenamed;
            }
        }

        #region -- IDE Event Handlers --

        private void ProjectItemsEvents_ItemRenamed(EnvDTE.ProjectItem ProjectItem, string OldName)
        {
            _clippy.StartAnimation(ClippyAnimation.Writing, true);
        }

        private void ProjectItemsEvents_ItemRemoved(EnvDTE.ProjectItem ProjectItem)
        {
            _clippy.StartAnimation(ClippyAnimation.EmptyTrash, true);
        }

        private void ProjectItemsEvents_ItemAdded(EnvDTE.ProjectItem ProjectItem)
        {
            _clippy.StartAnimation(ClippyAnimation.Congratulate, true);
        }

        private void FindEventsClass_FindDone(EnvDTE.vsFindResult Result, bool Cancelled)
        {
            _clippy.StartAnimation(ClippyAnimation.Searching, true);
        }

        private void BuildEvents_OnBuildDone(EnvDTE.vsBuildScope Scope, EnvDTE.vsBuildAction Action)
        {
            _clippy.StartAnimation(ClippyAnimation.Congratulate, true);
        }

        private void DocEvents_DocumentClosing(EnvDTE.Document Document)
        {
            _clippy.StartAnimation(ClippyAnimation.GestureDown, true);
        }

        private void BuildEvents_OnBuildBegin(EnvDTE.vsBuildScope Scope, EnvDTE.vsBuildAction Action)
        {
            _clippy.StartAnimation(ClippyAnimation.Processing, true); // GetTechy
        }

        private void DocumentEvents_DocumentSaved(EnvDTE.Document Document)
        {
            _clippy.StartAnimation(ClippyAnimation.Save, true);
        }

        private void DocumentEvents_DocumentOpening(string DocumentPath, bool ReadOnly)
        {
            _clippy.StartAnimation(ClippyAnimation.LookUp);
        }

        #endregion

        #region -- Owner Event Handlers --

        private void Owner_StateOrSizeChanged(object sender, EventArgs e)
        {
            double relativeTop = 0;
            double relativeLeft = 0;

            recalculateSpritePosition(out relativeTop, out relativeLeft);
        }

        private void Owner_LocationChanged(object sender, EventArgs e)
        {
            // Detach the LocationChanged event of the Sprite Window to avoid recursing relative positions calculation... 
            // TODO: find a better approach
            this.LocationChanged -= SpriteContainer_LocationChanged;

            double ownerTop = this.Owner.Top;
            double ownerLeft = this.Owner.Left;

            if (this.Owner.WindowState == WindowState.Maximized)
            {
                ownerTop = 0;
                ownerLeft = 0;
            }

            this.Top = ownerTop + this.RelativeTop;
            this.Left = ownerLeft + this.RelativeLeft;

            // Reattach the location changed event for the Clippy Sprite Window
            this.LocationChanged += SpriteContainer_LocationChanged;
        }

        #endregion

        #region -- ClippyWindow Event Handlers --

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void Grid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ContextMenu cm = this.FindResource("cmButton") as ContextMenu;
            cm.PlacementTarget = sender as Button;
            cm.IsOpen = true;
        }

        private async void cmdClose_Click(object sender, RoutedEventArgs e)
        {
            var window = this;

            _clippy.StartAnimation(ClippyAnimation.GoodBye, true);
            // refactor this be handled by end event.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            BackgroundWorker bgWorker = new BackgroundWorker();
            bgWorker.DoWork += (a, r) =>
            {
                while (_clippy.IsAnimating) { }

                this.Dispatcher.Invoke(new Action(() =>
                {
                    window.Owner.Focus();
                    window.Close();

                }), DispatcherPriority.Render);

            };
            bgWorker.RunWorkerAsync();
        }

        private void cmdRandom_Click(object sender, RoutedEventArgs e)
        {
            Random rmd = new Random();
            int random_int = rmd.Next(0, _clippy.AllAnimations.Count);

            _clippy.StartAnimation(_clippy.AllAnimations[random_int]);
        }

        private void cmdTestAnimation_Click(object sender, RoutedEventArgs e)
        {
            ClippyAnimation animation = (ClippyAnimation)Enum.Parse(typeof(ClippyAnimation), (sender as MenuItem).Header.ToString());

            _clippy.StartAnimation(animation, true);
        }

        private void ClippySpriteContainer_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                e.Cancel = true;
                this.Hide();
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.Message);
            }
        }

        private void ClippySpriteContainer_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.IsVisible)
            {
                _clippy.StartAnimation(ClippyAnimation.Idle1_1, true);

                _balloon.Owner = this;
                _balloon.Topmost = false;
            }
        }

        private void SpriteContainer_LocationChanged(object sender, EventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine(String.Format("Parent {0} {1}", this.Owner.Top, this.Owner.Left));
            //System.Diagnostics.Debug.WriteLine(String.Format("Child {0} {1}", this.Top, this.Left));

            if (_balloon.IsVisible)
                _balloon.HideBalloon();

            double relativeTop = this.Top;
            double relativeLeft = this.Left;

            recalculateSpritePosition(out relativeTop, out relativeLeft);

            this.RelativeLeft = relativeLeft;
            this.RelativeTop = relativeTop;

            // System.Diagnostics.Debug.WriteLine($"relativeTop {relativeTop} relativeLeft {relativeLeft}");

            try
            {
                storeRelativeSpritePosition(relativeTop, relativeLeft);
            }
            catch
            {

            }
        }

        /// <summary>
        /// TEMP: to test the balloons
        /// TODO: remove
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void SpriteContainer_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //if (balloon == null)
            //    balloon = new Balloon();

            //balloon.Owner = this;
            //balloon.Topmost = false;

            if (!_balloon.IsVisible)
                _balloon.ShowBalloon("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor", this, new BalloonButton("Close", BalloonCommands.DoClose));
            else
                _balloon.HideBalloon();
        }

        #endregion

        private void recalculateSpritePosition(out double relativeTop, out double relativeLeft, bool getDefaultPositioning = false)
        {
            // Managing multi-screen scenarios
            var ownerScreen = System.Windows.Forms.Screen.FromHandle(new System.Windows.Interop.WindowInteropHelper(this.Owner).Handle);
            double leftBoundScreenCorrection = ownerScreen.Bounds.X;
            double topBoundScreenCorrection = ownerScreen.Bounds.Y;

            double ownerTop = this.Owner.Top;
            double ownerLeft = this.Owner.Left;

            if (this.Owner.WindowState == WindowState.Maximized)
            {
                ownerTop = 0 + topBoundScreenCorrection;
                ownerLeft = 0 + leftBoundScreenCorrection;
            }

            double ownerRight = this.Owner.ActualWidth + ownerLeft;
            double ownerBottom = this.Owner.ActualHeight + ownerTop;

            if (ownerTop > this.Top)
                this.Top = ownerTop;

            if (ownerLeft > this.Left)
                this.Left = ownerLeft;

            if (this.Left + this.ActualWidth > ownerRight)
                this.Left = ownerRight - this.ActualWidth;

            if (this.Top + this.ActualHeight > ownerBottom)
                this.Top = ownerBottom - this.ActualHeight;

            if (!getDefaultPositioning)
            {
                relativeTop = this.Top - ownerTop;
                relativeLeft = this.Left - ownerLeft;
            }
            else
            {
                relativeTop = ownerBottom - (Clippy.ClipHeight + 100);
                relativeLeft = ownerRight - (Clippy.ClipWidth + 100);
            }
        }

        private void storeRelativeSpritePosition(double relativeTop, double relativeLeft)
        {
            try
            {
                if (!_userSettingsStore.CollectionExists(Constants.SettingsCollectionPath))
                {
                    _userSettingsStore.CreateCollection(Constants.SettingsCollectionPath);
                }

                _userSettingsStore.SetString(Constants.SettingsCollectionPath, nameof(RelativeTop), relativeTop.ToString());
                _userSettingsStore.SetString(Constants.SettingsCollectionPath, nameof(RelativeLeft), relativeLeft.ToString());
            }
            catch
            {

            }
        }
    }
}
