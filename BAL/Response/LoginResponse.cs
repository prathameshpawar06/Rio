using BAL.Response;
using DAL.Models;

namespace MLP.BAL.ViewModels
{
    public class LoginResponse : ResponseModel
    {
        public LoginResponse()
        {
            Data = new LoginData();
        }
        public LoginData Data { get; set; }
    }

}