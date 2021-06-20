using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Recoding.ClippyVSPackage
{
    public static class BalloonCommands
    {
        public static readonly RoutedCommand DoClose = new RoutedCommand("Close", typeof(Balloon));
    }
}
