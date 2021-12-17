using Microsoft.VisualStudio.Settings;
using System;
using System.ComponentModel.Composition;
using System.Diagnostics;

namespace Recoding.ClippyVSPackage
{
    /// <summary>
    /// Instance class to represent the ClippyVS user's settings
    /// </summary>
    [Export(typeof(IClippyVsSettings))]
    public class ClippyVsSettings : IClippyVsSettings
    {
        /// <summary>
        /// If true shows clippy at the VS startup
        /// </summary>
        public bool ShowAtStartup { get; set; } = true;

        /// <summary>
        /// String identifier for startup boolean in settings store
        /// </summary>
        private const string ShowAtStartupStoreName = "ShowAtStartup";

        /// <summary>
        /// If true shows clippy at the VS startup
        /// </summary>
        public string SelectedAssistantName { get; set; } = "";

        /// <summary>
        /// String identifier for startup boolean in settings store
        /// </summary>
        private const string SelectedAssistantNameName = "SelectedAssistantName";

        /// <summary>
        /// The real store in which the settings will be saved
        /// </summary>
        private readonly WritableSettingsStore _writableSettingsStore;

        #region -- Constructors --

        /// <summary>
        /// Constructor for service injection
        /// </summary>
        /// <param name="store">Settings Store</param>
        [ImportingConstructor]
        public ClippyVsSettings(WritableSettingsStore store)
        {
            _writableSettingsStore = store;
            LoadSettings();
        }

        #endregion

        /// <summary>
        /// Performs the store of the instance of this interface to the user's settings
        /// </summary>
        public void SaveSettings()
        {
            try
            {
                if (!_writableSettingsStore.CollectionExists(Constants.SettingsCollectionPath))
                {
                    _writableSettingsStore.CreateCollection(Constants.SettingsCollectionPath);
                }

                _writableSettingsStore.SetBoolean(Constants.SettingsCollectionPath, ShowAtStartupStoreName, ShowAtStartup);
                _writableSettingsStore.SetString(Constants.SettingsCollectionPath, SelectedAssistantNameName, SelectedAssistantName);
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
                if (_writableSettingsStore.PropertyExists(Constants.SettingsCollectionPath, ShowAtStartupStoreName))
                {
                    ShowAtStartup = _writableSettingsStore.GetBoolean(Constants.SettingsCollectionPath, ShowAtStartupStoreName, false);
                }

                if (_writableSettingsStore.PropertyExists(Constants.SettingsCollectionPath, SelectedAssistantNameName))
                {
                    SelectedAssistantName = _writableSettingsStore.GetString(Constants.SettingsCollectionPath, SelectedAssistantNameName, "");
                }
                Debug.WriteLine("Setting loaded which is {0} {1}", ShowAtStartup, SelectedAssistantName);
            }
            catch (ArgumentException ex)
            {
                Debug.WriteLine("Loading Settings failed with error " + ex.Message);
            }
        }
    }
}
