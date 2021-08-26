using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Recoding.ClippyVSPackage
{
    public class BalloonButton
    {
        public BalloonButton(string text, Action action)
        {
            this.Text = text;
            this.Action = action;
        }

        public BalloonButton(string text, RoutedCommand e)
        {
            this.Text = text;
            this.Command = e;
        }

        public string Text { get; set; }

        public RoutedCommand Command { get; set; }

        public Action Action { get; set; }
    }
}
