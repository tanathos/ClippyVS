using System;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Windows;
using Microsoft.VisualStudio;
using System.Runtime.InteropServices;
using System.Linq;
using SharedProject1;
using Task = System.Threading.Tasks.Task;

namespace Recoding.ClippyVSPackage
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "0.4", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideService(typeof(OleMenuCommandService))]
    [ProvideAutoLoad(VSConstants.UICONTEXT.NoSolution_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionHasMultipleProjects_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionHasSingleProject_string, PackageAutoLoadFlags.BackgroundLoad)]

    [Guid(Constants.GuidClippyVsPkgString)]
    [ProvideOptionPage(typeof(OptionsPage), "Clippy VS", "General", 0, 0, supportsAutomation: true)]

    public sealed class ClippyVisualStudioPackage : AsyncPackage
    {
        private SpriteContainer SpriteContainer { get; set; }
        private IClippyVsSettings Settings { get; set; }

        /// <summary>
        /// Default ctor
        /// https://docs.microsoft.com/en-us/visualstudio/extensibility/internals/designing-xml-command-table-dot-vsct-files?view=vs-2022
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
            try
            {
                Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
                Initialize();

                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(this.DisposalToken);
                if (Application.Current.MainWindow != null)
                    Application.Current.MainWindow.ContentRendered += MainWindow_ContentRendered;

                var shellSettingsManager = new ShellSettingsManager(ServiceProvider.GlobalProvider);
                var writableSettingsStore = shellSettingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);

                Settings = new ClippyVsSettings(writableSettingsStore);

                // Add our command handlers for menu (commands must exist in the .vsct file)
                if (await GetServiceAsync(typeof(IMenuCommandService)) is OleMenuCommandService mcs)
                {
                    // Create the commands for the menu item.
                    var menuCommandId = new CommandID(Constants.GuidClippyVsCmdSet, (int)PkgCmdIdList.CmdShowClippy);
                    var menuItem = new MenuCommand(MenuItemCallback, menuCommandId);
                    mcs.AddCommand(menuItem);

                    var menuCommand2Id = new CommandID(Constants.GuidClippyVsCmdSet, (int)PkgCmdIdList.CmdShowMerlin);
                    var menuItem2 = new OleMenuCommand(MenuItemCallback, menuCommand2Id);
                    mcs.AddCommand(menuItem2);

                    var menuCommandGeniusId = new CommandID(Constants.GuidClippyVsCmdSet, (int)PkgCmdIdList.CmdidCommandGenius);
                    var menuItemGenius = new MenuCommand(MenuItemCallback, menuCommandGeniusId);
                    mcs.AddCommand(menuItemGenius);
                }

                await Command1.InitializeAsync(this).ConfigureAwait(true);
                await Command2.InitializeAsync(this).ConfigureAwait(true);
                await CommandGenius.InitializeAsync(this).ConfigureAwait(true);
            }
            catch (Exception e)
            {
                MessageBox.Show("Exception !" + e.Message);
            }
        }

        internal void ReviveMerlinCommand()
        {
            var visibleAssistants = Application.Current.Windows.OfType<SpriteContainer>();
            if (!visibleAssistants.Any())
            {
                SpriteContainer = new SpriteContainer(this, true);
            }

            Settings.SelectedAssistantName = "Merlin";
            Settings.SaveSettings();
            
            Application.Current.Windows.OfType<SpriteContainer>().First().Show();
            SpriteContainer.ReviveMerlin();
        }


        private void ReviveClippyCommand()
        {
            var visibleAssistants = Application.Current.Windows.OfType<SpriteContainer>();
            if (!visibleAssistants.Any())
            {
                SpriteContainer = new SpriteContainer(this, true);
            }

            Settings.SelectedAssistantName = "Clippy";
            Settings.SaveSettings();

            Application.Current.Windows.OfType<SpriteContainer>().First().Show();
            SpriteContainer.ReviveClippy();
        }


        internal void ReviveGeniusCommand()
        {
            var visibleAssistants = Application.Current.Windows.OfType<SpriteContainer>();
            if (!visibleAssistants.Any())
            {
                SpriteContainer = new SpriteContainer(this, true);
            }

            Settings.SelectedAssistantName = "Genius";
            Settings.SaveSettings();

            Application.Current.Windows.OfType<SpriteContainer>().First().Show();
            SpriteContainer.ReviveGenius();
        }

        private async void MainWindow_ContentRendered(object sender, EventArgs e)
        {
            await MainWindow_ContentRenderedAsync();
        }

        private async Task MainWindow_ContentRenderedAsync()
        {
            try
            {
                var token = new CancellationToken();
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(token);
                switch (Settings.SelectedAssistantName)
                {
                    case "Genius":
                        SpriteContainer = new SpriteContainer(this, false, true);
                        break;
                    case "Merlin":
                        SpriteContainer = new SpriteContainer(this, true);
                        break;
                    default:
                        SpriteContainer = new SpriteContainer(this);
                        break;
                }

                if (Settings.ShowAtStartup)
                    SpriteContainer.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Application Exception occured. Closing." + Environment.NewLine + ex.Message);
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

            ThreadHelper.ThrowIfNotOnUIThread();
            this.ReviveClippyCommand();
        }
    }
}
