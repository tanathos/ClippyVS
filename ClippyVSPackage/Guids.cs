// Guids.cs
// MUST match guids.h
using System;

namespace Recoding.ClippyVSPackage
{
    static class GuidList
    {
        public const string guidClippyVSPkgString = "6aae52ac-8840-421f-bb35-60b52ab431e2";
        public const string guidClippyVSCmdSetString = "fbed79a9-1faa-4dc3-9f96-9fb39d31bfdb";

        public static readonly Guid guidClippyVSCmdSet = new Guid(guidClippyVSCmdSetString);
    };
}