using System.ComponentModel.DataAnnotations;

namespace BAL.ViewModels
{
    public class LoginModel
    {
        public required string MobileNo { get; set; } 
        
        public string MobileNoOTP { get; set; }    
    }
}
