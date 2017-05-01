using Windows.System.Profile;

namespace Mirror.Core
{
    public class OSVersion
    {
        public static OSVersion Current { get; } = new OSVersion();

        public ulong Major { get; }

        public ulong Minor { get; }

        public ulong Build { get; }

        public ulong Revision { get; }

        private OSVersion()
        {
            string deviceFamilyVersion = AnalyticsInfo.VersionInfo.DeviceFamilyVersion;
            ulong version = ulong.Parse(deviceFamilyVersion);
            Major = (version & 0xFFFF000000000000L) >> 48;
            Minor = (version & 0x0000FFFF00000000L) >> 32;
            Build = (version & 0x00000000FFFF0000L) >> 16;
            Revision = (version & 0x000000000000FFFFL);
        }

        public override string ToString() => $"{Major}.{Minor}.{Build}.{Revision}";
    }
}