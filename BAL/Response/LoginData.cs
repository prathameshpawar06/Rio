using DAL.Models;

namespace BAL.Response
{
    public class LoginData
    {
        public string MobileNoOTP { get; set; }
        public DateTime LoginDate { get; set; }
        public string Token { get; set; }
        public string LoginRole { get; set; }
        public CompanyMaster companyMaster { get; set; }
        public Salesman salesman { get; set; }
    }
}
