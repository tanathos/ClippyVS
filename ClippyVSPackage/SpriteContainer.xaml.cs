using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.Settings;
using Recoding.ClippyVSPackage.Configurations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
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
            var values = Enum.GetValues(typeof(ClippyAnimationTypes));

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

            // this.Show();

            _clippy = new Clippy((Canvas)this.FindName("ClippyCanvas"));

            this.Dispatcher.Invoke(new Action(() =>
            {
                _clippy.StartAnimation(ClippyAnimationTypes.Idle1_1);
            }), DispatcherPriority.Send);
        }

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
                _clippy.StartAnimation(ClippyAnimationTypes.GoodBye, true);
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
            ClippyAnimationTypes animation = (ClippyAnimationTypes)Enum.Parse(typeof(ClippyAnimationTypes), (sender as MenuItem).Header.ToString());

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
                    _clippy.StartAnimation(ClippyAnimationTypes.Idle1_1, true);
                }), DispatcherPriority.Send);
            }
        }
    }
}
