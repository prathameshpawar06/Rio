namespace BAL.Response
{
    public class CheckAppVersionResponse : ResponseModel
    {
        public CheckAppVersionResponse()
        {
            Data = new AppVersion();
        }
        public AppVersion Data { get; set; }
    }
}
