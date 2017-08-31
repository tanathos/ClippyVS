using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Recoding.ClippyVSPackage
{
    [Guid(Constants.guidOptionsPage)]
    public class OptionsPage : Microsoft.VisualStudio.Shell.DialogPage
    {
        IClippyVSSettings settings
        {
            get
            {
                var componentModel = (IComponentModel)(Site.GetService(typeof(SComponentModel)));
                IClippyVSSettings s = componentModel.DefaultExportProvider.GetExportedValue<IClippyVSSettings>();

                return s;
            }
        }

        public OptionsPage()
        {
            
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
            IClippyVSSettings storedValues = settings;

            IClippyVSSettings currentValues = new ClippyVSSettings()
            {
                ShowAtStartup = ShowAtStartup
            };

            settings.ShowAtStartup = currentValues.ShowAtStartup;

            settings.Store();

            base.OnApply(e);
        }

        private void BindSettings()
        {
            ShowAtStartup = settings.ShowAtStartup;
        }
    }
}
