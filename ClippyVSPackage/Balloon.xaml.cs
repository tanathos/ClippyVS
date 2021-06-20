using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Recoding.ClippyVSPackage
{
    /// <summary>
    /// Interaction logic for Balloon.xaml
    /// </summary>
    public partial class Balloon : Window
    {
        public ObservableCollection<BalloonButton> Buttons { get; set; }

        public Balloon()
        {
            Buttons = new ObservableCollection<BalloonButton>();

            InitializeComponent();
        }

        public void ShowBalloon(string message, Control relativeTo, params BalloonButton[] buttons)
        {
            this.txtMessage.Text = message;

            this.Buttons.Clear();

            foreach (BalloonButton button in buttons)
            {
                this.Buttons.Add(button);
            }

            this.Show();

            // Issue: I've to fix this.
            // If I'm trying to calculate the position before the Show() method the Actual sizes are not updated and, no, the UpdateLayout on both this and relativeTo is not working

            // Calculate balloon position relative to Clippy
            Point location = GetControlPosition(relativeTo);
            this.Left = location.X - (this.ActualWidth / 2) + (relativeTo.ActualWidth / 2);
            this.Top = location.Y - (this.ActualHeight);
        }

        public void HideBalloon()
        {
            this.Hide();
        }

        private Point GetControlPosition(Control myControl)
        {
            Point locationToScreen = myControl.PointToScreen(new Point(0, 0));
            PresentationSource source = PresentationSource.FromVisual(myControl);

            return source.CompositionTarget.TransformFromDevice.Transform(locationToScreen);
        }

        private void BalloonButton_Click(object sender, RoutedEventArgs e)
        {
            var command = ((Recoding.ClippyVSPackage.BalloonButton)(sender as Button).DataContext).Command;
            if (command != null)
            {
                if (command.CanExecute(null, this))
                {
                    command.Execute(null, this);
                }
            }
        }

        private void DoClose_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.HideBalloon();
        }
    }
}
