using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Recoding.ClippyVSPackage
{
    [Guid(Constants.GuidOptionsPage)]
    public class OptionsPage : DialogPage
    {
        private readonly WritableSettingsStore _store;

        [Category("General")]
        [Description("If true shows Clippy at the VS startup")]
        [DisplayName("Show at startup")]
        private bool ShowAtStartup { get; set; }

        public OptionsPage()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var settingsManager = new ShellSettingsManager(ServiceProvider.GlobalProvider);
            _store = settingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
        }

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
            ThreadHelper.ThrowIfNotOnUIThread();
            var shellSettingsManager = new ShellSettingsManager(ServiceProvider.GlobalProvider);
            var writableSettingsStore = shellSettingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);

            IClippyVsSettings appliedValues = new ClippyVsSettings(writableSettingsStore)
            {
                ShowAtStartup = ShowAtStartup
            };

            appliedValues.SaveSettings();
            base.OnApply(e);
        }

        private IClippyVsSettings GetClippySettings()
        {
            bool res = false;
            if (_store.PropertyExists(Constants.SettingsCollectionPath, "ShowAtStartup"))
            {
                res = _store.GetBoolean(Constants.SettingsCollectionPath, "ShowAtStartup");
            }

            return new ClippyVsSettings(_store)
            {
                ShowAtStartup = res
            };
        }

        private void BindSettings()
        {
            ShowAtStartup = GetClippySettings().ShowAtStartup;
        }
    }
}
