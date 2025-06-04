namespace BAL.Response
{
    public class AppVersion
    {
        public string AndroidVersion { get; set; }

        public string IosVersion { get; set; }

        public bool IsMaintenance { get; set; }

        public bool IsAppUpdate { get; set; }
    }
}
