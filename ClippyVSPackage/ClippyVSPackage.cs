using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using Task = System.Threading.Tasks.Task;

namespace Recoding.ClippyVSPackage
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "0.3", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(Constants.guidClippyVSPkgString)]
    [ProvideOptionPageAttribute(typeof(OptionsPage), "Clippy VS", "General", 0, 0, supportsAutomation: true)]
    public sealed class ClippyVisualStudioPackage : AsyncPackage
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public ClippyVisualStudioPackage()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            Application.Current.MainWindow.ContentRendered += MainWindow_ContentRendered;

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = await GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                // Create the command for the menu item.
                CommandID menuCommandID = new CommandID(Constants.guidClippyVSCmdSet, (int)PkgCmdIDList.cmdShowClippy);
                MenuCommand menuItem = new MenuCommand(MenuItemCallback, menuCommandID);
                mcs.AddCommand(menuItem);
            }
            await Command1.InitializeAsync(this);
        }

        void MainWindow_ContentRendered(object sender, EventArgs e)
        {
            var componentModel = (IComponentModel)(GetService(typeof(SComponentModel)));
            IClippyVSSettings s = componentModel.DefaultExportProvider.GetExportedValue<IClippyVSSettings>();

            SpriteContainer container = new SpriteContainer(this);

            if (s.ShowAtStartup)
            {
                container.Show();
            }
        }

        #endregion

        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            if (!Application.Current.Windows.OfType<SpriteContainer>().Any())
            {
                SpriteContainer container = new SpriteContainer(this);
            }

            Application.Current.Windows.OfType<SpriteContainer>().First().Show();

        }
    }
}
