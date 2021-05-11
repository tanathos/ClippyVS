using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Recoding.ClippyVSPackage
{
    [Guid(Constants.guidOptionsPage)]
    public class OptionsPage : Microsoft.VisualStudio.Shell.DialogPage
    {
        private WritableSettingsStore _store;
        IClippyVSSettings settings
        {
            get
            {
                var s = GetClippySettings();
                return s;
            }
        }

        private IClippyVSSettings GetClippySettings()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            bool res = false;
            if (_store.PropertyExists(Constants.SettingsCollectionPath, "ShowAtStartup"))
            {
                res = _store.GetBoolean(Constants.SettingsCollectionPath, "ShowAtStartup");
                return new ClippyVSSettings(ServiceProvider.GlobalProvider)
                {
                    ShowAtStartup = res
                };
            }
            return null;

        }

        public OptionsPage()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var settingsManager = new ShellSettingsManager(ServiceProvider.GlobalProvider);
            _store = settingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
        }

        [Category("General")]
        [Description("If true shows Clippy at the VS startup")]
        [DisplayName("Show at startup")]
        public bool ShowAtStartup { get; set; }

        /// <summary>
        /// Handles "activate" messages from the Visual Studio environment.
        /// </summary>
        /// <devdoc>
        /// This method is called when Visual Studio wants to activate this page.  
        /// </devdoc>
        /// <remarks>If this handler sets e.Cancel to true, the activation will not occur.</remarks>
        protected override void OnActivate(CancelEventArgs e)
        {
            base.OnActivate(e);

            BindSettings();
        }

        /// <summary>
        /// Handles "apply" messages from the Visual Studio environment.
        /// </summary>
        /// <devdoc>
        /// This method is called when VS wants to save the user's 
        /// changes (for example, when the user clicks OK in the dialog).
        /// </devdoc>
        protected override void OnApply(PageApplyEventArgs e)
        {

            IClippyVSSettings appliedValues = new ClippyVSSettings(ServiceProvider.GlobalProvider)
            {
                ShowAtStartup = ShowAtStartup
            };

            appliedValues.Store();
            base.OnApply(e);
        }

        private void BindSettings()
        {
            ShowAtStartup = settings.ShowAtStartup;
        }
    }
}
