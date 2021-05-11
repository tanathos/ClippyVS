using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell.Settings;
using System;
using System.ComponentModel.Composition;
using System.Diagnostics;

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
        /// Constructor for service injection
        /// </summary>
        /// <param name="vsServiceProvider"></param>
        [ImportingConstructor]
        public ClippyVSSettings(IServiceProvider vsServiceProvider)
        {
            var shellSettingsManager = new ShellSettingsManager(vsServiceProvider);
            writableSettingsStore = shellSettingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);

            LoadSettings();
        }

        #endregion

        /// <summary>
        /// If true shows clippy at the VS startup
        /// </summary>
        public bool ShowAtStartup { get; set; } = true;

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

                writableSettingsStore.SetBoolean(Constants.SettingsCollectionPath, "ShowAtStartup", ShowAtStartup);
                Debug.WriteLine("Setting stored which is {0}", ShowAtStartup);
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
            try
            {
                // Tries to retrieve the configurations if previously saved
                if (writableSettingsStore.PropertyExists(Constants.SettingsCollectionPath, "ShowAtStartup"))
                {
                    ShowAtStartup = writableSettingsStore.GetBoolean(Constants.SettingsCollectionPath, "ShowAtStartup");
                    Debug.WriteLine("Setting loaded which is {0}", ShowAtStartup);
                }
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.Message);
            }
        }
    }
}
