using System;

namespace Recoding.ClippyVSPackage
{
    static class Constants
    {
        public const string GuidClippyVsPkgString = "6aae52ac-8840-421f-bb35-60b52ab431e2";
        public const string GuidClippyVsCmdSetString = "fbed79a9-1faa-4dc3-9f96-9fb39d31bfdb";
        public const string GuidOptionsPage = "663638f6-c16c-4401-84db-8bc093182cf8";

        public static readonly Guid GuidClippyVsCmdSet = new Guid(GuidClippyVsCmdSetString);

        /// <summary>
        /// The name of the collection of settings in the WritableSettingsStore
        /// </summary>
        public const string SettingsCollectionPath = "ClippyVS";
    };
}