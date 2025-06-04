using BAL.Response;
using BAL.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class CheckAppVersionController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly LoggerService _loggerService;

        public CheckAppVersionController(IConfiguration configuration,
            LoggerService logger)
        {
            _configuration = configuration;
            _loggerService = logger;
        }

        [HttpGet]
        [Route("checkappversion/checkversion/")]
        public async Task<CheckAppVersionResponse> CheckVersionAsync()
        {
            CheckAppVersionResponse resp = new();
            try
            {
                AppVersion appVersion = new()
                {
                    AndroidVersion = _configuration.GetValue<string>("AndroidVersion"),
                    IosVersion = _configuration.GetValue<string>("IosVersion"),
                    IsMaintenance = _configuration.GetValue<bool>("IsMaintenance"),
                    IsAppUpdate = _configuration.GetValue<bool>("IsAppUpdate")
                };

                resp.Data = appVersion;
                resp.Message = "Please check your version.";
                return resp;
            }
            catch (Exception ex)
            {
                await _loggerService.InsertLogAsync(ex.Message);
                return new CheckAppVersionResponse
                {
                    Error = 1,
                    Message = ex.Message
                };
            }
        }
    }
}
