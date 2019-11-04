using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;
using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Globalization;

namespace Recoding.ClippyVSPackage
{
    /// <summary>
    /// Instance class to represent the ClippyVS user's settings
    /// </summary>
    [Export(typeof(IClippyVSSettings))]
    public class ClippyVSSettings : IClippyVSSettings
    {
        /// <summary>
        /// The real store in which the settings will be saved
        /// </summary>
        readonly WritableSettingsStore writableSettingsStore;

        #region -- Constructors --

        /// <summary>
        /// Default ctor
        /// </summary>
        public ClippyVSSettings()
        {

        }

        /// <summary>
        /// Constructor for service injection
        /// </summary>
        /// <param name="vsServiceProvider"></param>
        [ImportingConstructor]
        public ClippyVSSettings(SVsServiceProvider vsServiceProvider) : this()
        {
            var shellSettingsManager = new ShellSettingsManager(vsServiceProvider);
            writableSettingsStore = shellSettingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);

            LoadSettings();
        }

        #endregion

        /// <summary>
        /// If true shows clippy at the VS startup
        /// </summary>
        public bool ShowAtStartup { get; set; }

        /// <summary>
        /// Performs the store of the instance of this interface to the user's settings
        /// </summary>
        public void Store()
        {
            try
            {
                if (!writableSettingsStore.CollectionExists(Constants.SettingsCollectionPath))
                {
                    writableSettingsStore.CreateCollection(Constants.SettingsCollectionPath);
                }

                writableSettingsStore.SetString(Constants.SettingsCollectionPath, nameof(ShowAtStartup), ShowAtStartup.ToString(CultureInfo.InvariantCulture));
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.Message);
            }
        }

        /// <summary>
        /// Read the stored settings
        /// </summary>
        private void LoadSettings()
        {
            // Default values
            this.ShowAtStartup = true;

            try
            {
                // Tries to retrieve the configurations if previously saved
                if (writableSettingsStore.PropertyExists(Constants.SettingsCollectionPath, nameof(ShowAtStartup)))
                {
                    bool b = this.ShowAtStartup;
                    if (Boolean.TryParse(writableSettingsStore.GetString(Constants.SettingsCollectionPath, nameof(ShowAtStartup)), out b))
                        this.ShowAtStartup = b;
                }
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.Message);
            }
        }
    }
}
