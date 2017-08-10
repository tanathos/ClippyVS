using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;
using Recoding.ClippyVSPackage.Configurations;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Recoding.ClippyVSPackage
{
    /// <summary>
    /// Interaction logic for SpriteContainer.xaml
    /// </summary>
    public partial class SpriteContainer : Window
    {
        private Clippy _clippy { get; set; }

        private Package _package;

        private WritableSettingsStore _userSettingsStore;

        private const string CollectionPath = "ClippyVS";

        public SpriteContainer(Package package)
        {
            _package = package;

            InitializeComponent();

            this.Owner = System.Windows.Application.Current.MainWindow;

            #region Register event handlers

            this.Owner.LocationChanged += new EventHandler(Owner_LocationChanged);
            this.LocationChanged += new EventHandler(SpriteContainer_LocationChanged);

            #endregion


            SettingsManager settingsManager = new ShellSettingsManager(_package);
            _userSettingsStore = settingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);

            double mainWindowTop = (this.Owner.WindowState == System.Windows.WindowState.Maximized) ? 0 : this.Owner.Top;
            double mainWindowLeft = (this.Owner.WindowState == System.Windows.WindowState.Maximized) ? 0 : this.Owner.Left;

            this.Top = mainWindowTop + (this.Owner.ActualHeight - 200);
            this.Left = mainWindowLeft + (this.Owner.ActualWidth - 600);

            try
            {
                if (_userSettingsStore.PropertyExists(CollectionPath, "Top"))
                    this.Top = Double.Parse(_userSettingsStore.GetString(CollectionPath, "Top"));

                if (_userSettingsStore.PropertyExists(CollectionPath, "Left"))
                    this.Left = Double.Parse(_userSettingsStore.GetString(CollectionPath, "Left"));
            }
            catch 
            {
            
            }

            // TEMP: create a voice for each animation in the context menu
            var pMenu = (ContextMenu)this.Resources["cmButton"];
            var values = Enum.GetValues(typeof(ClippyAnimations));

            foreach (var val in values)
            {
                var menuItem = new MenuItem()
                {
                    Header = val.ToString(),
                    Name = "cmd" + val.ToString()
                };
                menuItem.Click += cmdTestAnimation_Click;
                pMenu.Items.Add(menuItem);
            }
            // /TEMP

            _clippy = new Clippy((Canvas)this.FindName("ClippyCanvas"));

            // Start default Idle animation
            this.Dispatcher.Invoke(new Action(() =>
            {
                _clippy.StartAnimation(ClippyAnimations.Idle1_1);
            }), DispatcherPriority.Send);
        }

        #region -- Event Handlers --

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

        private void cmdClose_Click(object sender, RoutedEventArgs e)
        {
            var window = this;

            this.Dispatcher.Invoke(new Action(() =>
            {
                _clippy.StartAnimation(ClippyAnimations.GoodBye, true);
            }), DispatcherPriority.Send);

            BackgroundWorker bgWorker = new BackgroundWorker();
            bgWorker.DoWork += (a, r) =>
            {
                while (_clippy.IsAnimating) { }

                this.Dispatcher.Invoke(new Action(() =>
                {
                    window.Owner.Focus();

                    window.Close();
                    
                }), DispatcherPriority.Send);
                
            };
            bgWorker.RunWorkerAsync();
        }

        private void cmdIdle_Click(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                Random rmd = new Random();
                int random_int = rmd.Next(0, Clippy.IdleAnimations.Count);

                _clippy.StartAnimation(Clippy.IdleAnimations[random_int]);
            }));
        }

        private void cmdTestAnimation_Click(object sender, RoutedEventArgs e)
        {
            ClippyAnimations animation = (ClippyAnimations)Enum.Parse(typeof(ClippyAnimations), (sender as MenuItem).Header.ToString());

            this.Dispatcher.Invoke(new Action(() =>
            {
                _clippy.StartAnimation(animation, true);
            }));
        }

        private void ClippySpriteContainer_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (!_userSettingsStore.CollectionExists(CollectionPath))
                {
                    _userSettingsStore.CreateCollection(CollectionPath);
                }

                _userSettingsStore.SetString(CollectionPath, "Top", this.Top.ToString());
                _userSettingsStore.SetString(CollectionPath, "Left", this.Left.ToString());

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
                this.Dispatcher.Invoke(new Action(() =>
                {
                    _clippy.StartAnimation(ClippyAnimations.Idle1_1, true);
                }), DispatcherPriority.Send);
            }
        }

        private void Owner_LocationChanged(object sender, EventArgs e)
        {
            // TODO: implement child relative positioning on parent location changed

            //foreach (SpriteContainer win in this.Owner.OwnedWindows)
            //{
            //    win.Top = this.Owner.Top + 100;
            //    win.Left = this.Owner.Left + 100;
            //}
        }

        private void SpriteContainer_LocationChanged(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(String.Format("Parent {0} {1}", this.Owner.Top, this.Owner.Left));
            System.Diagnostics.Debug.WriteLine(String.Format("Child {0} {1}", this.Top, this.Left));

            var ownerTop = this.Owner.Top;
            var ownerLeft = this.Owner.Left;

            if (this.Owner.WindowState == WindowState.Maximized)
            {
                ownerTop = 0;
                ownerLeft = 0;
            }

            var ownerRight = this.Owner.ActualWidth + ownerLeft;
            var ownerBottom = this.Owner.ActualHeight + ownerTop;

            if (ownerTop > this.Top)
                this.Top = ownerTop;

            if (ownerLeft > this.Left)
                this.Left = ownerLeft;

            if (this.Left + this.ActualWidth > ownerRight)
                this.Left = ownerRight - this.ActualWidth;

            if (this.Top + this.ActualHeight > ownerBottom)
                this.Top = ownerBottom - this.ActualHeight;

            // TODO: calculate relative positioning for child
            // TODO: store values 
        }

        #endregion
    }
}
