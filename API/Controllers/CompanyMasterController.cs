using AutoMapper;
using BAL.Response;
using BAL.Services;
using BAL.ViewModels;
using DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MLP.BAL.ViewModels;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace API.Controllers
{
    public class CompanyMasterController : Controller
    {
        #region Fields

        public readonly IMapper _mapper;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly CompanyMastersService _companyMastersService;
        private readonly IConfiguration _configuration;
        private readonly LoggerService _loggerService;
        private readonly ItemService _itemService;
        private readonly SalesmanService _salesmanService;

        #endregion

        #region Ctor

        public CompanyMasterController(IMapper mapper,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            CompanyMastersService companyMastersService,
            IConfiguration configuration,
            LoggerService loggerService,
            ItemService itemService,
            SalesmanService salesmanService)
        {
            _mapper = mapper;
            _userManager = userManager;
            _roleManager = roleManager;
            _companyMastersService = companyMastersService;
            _configuration = configuration;
            _loggerService = loggerService;
            _itemService = itemService;
            _salesmanService = salesmanService;
        }

        #endregion

        #region Utility

        private string GenerateJwtToken(IdentityUser identityUser, string roleName,int id)
        {
            var claims = new[]
            {
                   new Claim(ClaimTypes.Email, identityUser.Email ?? string.Empty),
                   new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                   new Claim(ClaimTypes.NameIdentifier, identityUser.Id.ToString()),
                   new Claim(ClaimTypes.MobilePhone, identityUser.PhoneNumber ?? string.Empty),
                   new Claim(ClaimTypes.Role, roleName),
                   new Claim("Id", id.ToString())
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetValue<string>("Key")));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
               issuer: _configuration.GetValue<string>("Issuer"),
               audience: _configuration.GetValue<string>("Audience"),
               claims: claims,
               expires: DateTime.UtcNow.AddDays(1), // Token expiration time
               signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        //Numeric OTP
        private static string GenerateEmailOTP()
        {
            // Generate a six-digit random OTP
            int otpLength = 6;
            string allowedChars = "1234567890";
            char[] chars = new char[otpLength];

            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                byte[] data = new byte[otpLength];
                rng.GetBytes(data);

                for (int i = 0; i < otpLength; i++)
                {
                    int index = data[i] % allowedChars.Length;
                    chars[i] = allowedChars[index];
                }
            }
            return new string(chars);
        }

        #endregion

        #region Methods 

        [HttpPost]
        [Route("/companymaster/registration/")]
        public async Task<CompanyMasterResponse> RegisterAsync([FromBody] CompanyMasterModel companyMasterModel)
        {
            try
            {
                CompanyMasterResponse resp = new();
                resp.Error = 1;
                var userExite = await _userManager.FindByEmailAsync(companyMasterModel.EmailID);
                var userExiteByMobile = await _userManager.FindByNameAsync(companyMasterModel.MobileNo);
                var userExiteBYMobileNo = await _companyMastersService.GetCompanyMasterByMobileNoAsync(companyMasterModel.MobileNo);
               
                if (userExite != null)
                {
                    resp.Message = "User already exists by email!";
                    return resp;
                }
                else if (userExiteBYMobileNo != null || userExiteByMobile != null)
                {
                    resp.Message = "User already exists by mobile number!";
                    return resp;
                }

                //Add the user in the database 
                IdentityUser user = new()
                {
                    SecurityStamp = Guid.NewGuid().ToString(),
                    Email = companyMasterModel.EmailID,
                    PhoneNumber = companyMasterModel.MobileNo,
                    UserName = companyMasterModel.MobileNo
                };

                var result = await _userManager.CreateAsync(user);

                if (result.Succeeded)
                {
                    if (!await _roleManager.RoleExistsAsync("Admin"))
                        await _roleManager.CreateAsync(new IdentityRole("Admin"));

                    if (!String.IsNullOrEmpty(companyMasterModel?.Logo))
                    {
                        string savedFilename = _itemService.SaveImage(companyMasterModel.Logo);
                        companyMasterModel.Logo = savedFilename ?? string.Empty;
                    }

                    CompanyMaster companymaster = _mapper.Map<CompanyMaster>(companyMasterModel);
                    companymaster.MobileNoOTP = "";
                    companymaster.EmailOTP = "";
                    companymaster.COCODE = Guid.NewGuid().ToString();
                    companymaster.SerialNO = Guid.NewGuid().ToString();
                    await _companyMastersService.CreateCompanyMastersAsync(companymaster);

                    if (await _roleManager.RoleExistsAsync("Admin"))
                        await _userManager.AddToRoleAsync(user, "Admin");

                    //for response
                    var companyMasters = _mapper.Map<CompanyMasterResponseModel>(companymaster);
                    resp.Data.CompanyModels.Add(companyMasters);
                    resp.Error = 0;
                    resp.Message = "Company registration successful.";
                    return resp;
                }

                resp.Message = "User already exists.";
                return resp;
            }
            catch (Exception ex)
            {
                await _loggerService.InsertLogAsync(ex.Message);
                return new CompanyMasterResponse
                {
                    Error = 1,
                    Message = ex.Message ?? "Check internet connection"
                };
            }
        }

        [HttpPost]
        [Route("/companyMaster/login/")]
        public async Task<LoginResponse> LoginAsync([FromBody] LoginModel loginModel)
        {
            try
            {
                LoginResponse resp = new()
                {
                    Error = 1,
                    Message = "Please enter a valid mobile number,User not registered."
                };

                var customer = await _userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == loginModel.MobileNo);

                if (customer == null)
                {
                    return resp;
                }

                var userRole = (await _userManager.GetRolesAsync(customer)).FirstOrDefault() ?? string.Empty;

                if (string.IsNullOrEmpty(userRole))
                {
                    return resp;
                }

                if (userRole == "Admin")
                {
                    var CMcustomer = await _companyMastersService.GetCompanyMasterByMobileNoAsync(loginModel.MobileNo);
                    if (CMcustomer != null)
                    {
                        CMcustomer.MobileNoOTP = GenerateEmailOTP();
                        await _companyMastersService.UpdateCompanyMastersAsync(CMcustomer);

                        resp.Error = 0;
                        resp.Message = "OTP sent to your registered mobile number,Please check your OTP";
                        resp.Data.MobileNoOTP = CMcustomer.MobileNoOTP;
                        resp.Data.LoginDate = DateTime.Now;
                    }
                }
                else if (userRole == "Salesman")
                {
                    var SMCustomer = await _salesmanService.GetSalesmenByMobileNoAsync(loginModel.MobileNo);
                    if (SMCustomer != null)
                    {
                        SMCustomer.MobileNoOTP = GenerateEmailOTP();
                        await _salesmanService.UpdateSalesManAsync(SMCustomer);

                        resp.Error = 0;
                        resp.Message = "OTP sent to your registered mobile number,Please check your OTP";
                        resp.Data.MobileNoOTP = SMCustomer.MobileNoOTP;
                        resp.Data.LoginDate = DateTime.Now;
                    }
                }

                return resp;
            }
            catch (Exception ex)
            {
                await _loggerService.InsertLogAsync(ex.Message);
                return new LoginResponse
                {
                    Error = 1,
                    Message = ex.Message ?? "Check internet connection"
                };
            }
        }

        [HttpPost]
        [Route("/companymaster/verifymobileotp/")]
        public async Task<LoginResponse> VerifyMobileOTPAsync([FromBody] LoginModel loginModel)
        {
            try
            {
                LoginResponse resp = new();
                resp.Error = 1;

                var customer = await _userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == loginModel.MobileNo);

                if (customer == null)
                {
                    resp.Message = "Customer not found.";
                    return resp;
                }

                var userRole = (await _userManager.GetRolesAsync(customer)).FirstOrDefault() ?? string.Empty;

                if (string.IsNullOrEmpty(userRole))
                {
                    resp.Message = "Please contact the admin to assign a role.";
                    return resp;
                }

                if (userRole == "Admin")
                {
                    var cmCustomer = await _companyMastersService.GetCompanyMasterByMobileNoAsync(loginModel.MobileNo);

                    if (string.IsNullOrEmpty(cmCustomer?.MobileNoOTP))
                    {
                        resp.Message = "Please log in first before proceeding.";
                        return resp;
                    }

                    var success = cmCustomer.MobileNoOTP == loginModel.MobileNoOTP;
                    if (success)
                    {
                        var roleName = "Admin";
                        resp.Data.LoginRole = userRole;
                        resp.Data.companyMaster = cmCustomer;
                        resp.Data.Token = GenerateJwtToken(customer, roleName, cmCustomer.ApplicationSrNo);
                        resp.Message = "Your verification is successful.";
                        resp.Error = 0;
                        return resp;
                    }
                    resp.Message = "Please insert the correct OTP.";
                    return resp;
                }
                else if (userRole == "Salesman")
                {
                    var smCustomer = await _salesmanService.GetSalesmenByMobileNoAsync(loginModel.MobileNo);
                    if (smCustomer == null && string.IsNullOrEmpty(smCustomer?.MobileNoOTP))
                    {
                        resp.Message = "Please log in first before proceeding.";
                        return resp;
                    }

                    var success = smCustomer.MobileNoOTP == loginModel.MobileNoOTP;
                    if (success)
                    {
                        var companyMaster = await _companyMastersService.GetCompanyMasterBySalesManAsync(smCustomer);
                        if(companyMaster != null)
                        {
                            companyMaster.MobileNoOTP = "";
                            companyMaster.EmailOTP = "";
                            resp.Data.companyMaster = companyMaster;
                        }

                        var roleName = "Salesman";
                        resp.Data.LoginRole = userRole;
                        resp.Data.salesman = smCustomer;
                        resp.Data.Token = GenerateJwtToken(customer, roleName, smCustomer.EmpCode);
                        resp.Message = "Your verification is successful.";
                        resp.Error = 0;
                        return resp;
                    }
                    resp.Message = "Please insert the correct OTP";
                    return resp;
                }

                resp.Message = "Please contact the admin.";
                return resp;
            }
            catch (Exception ex)
            {
                await _loggerService.InsertLogAsync(ex.Message);
                return new LoginResponse
                {
                    Error = 1,
                    Message = ex.Message ?? "Check internet connection"
                };
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("/companymaster/refresh/")]
        public async Task<LoginResponse> Refresh(string token)
        {
            try
            {
                LoginResponse resp = new();
                resp.Error = 1;
                resp.Message = "Please enter a valid token";
                var tokenHandler = new JwtSecurityTokenHandler();

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = false, // Skip lifetime validation
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = _configuration.GetValue<string>("Issuer"),
                    ValidAudience = _configuration.GetValue<string>("Audience"),
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetValue<string>("Key")))
                };

                var tokenDetails = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                if (tokenDetails == null)
                {
                    return resp;
                }

                var userIdClaim = tokenDetails.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                var userEmail = tokenDetails.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                var userPhoneNumber = tokenDetails.Claims.FirstOrDefault(c => c.Type == ClaimTypes.MobilePhone)?.Value;
                var userRole = tokenDetails.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                var tableId = tokenDetails.Claims.FirstOrDefault(c => c.Type == "Id")?.Value;

                if (string.IsNullOrEmpty(userIdClaim) && string.IsNullOrEmpty(userPhoneNumber) && string.IsNullOrEmpty(userRole) && string.IsNullOrEmpty(tableId))
                {
                    return resp;
                }

                if (userRole == "Admin")
                {
                    if (string.IsNullOrEmpty(userEmail))
                    {
                        return resp;
                    }

                    var isValidCustomer = await _companyMastersService.GetCompanyMasterByMobileNoAsync(userPhoneNumber!);
                    if (isValidCustomer != null && isValidCustomer.EmailID == userEmail && isValidCustomer.MobileNo == userPhoneNumber && isValidCustomer.ApplicationSrNo == int.Parse(tableId!))
                    {
                        var roleName = "Admin";
                        var user = new IdentityUser
                        {
                            Email = userEmail,
                            Id = userIdClaim!,
                            PhoneNumber = userPhoneNumber,
                        };

                        resp.Data.Token = GenerateJwtToken(user, roleName, isValidCustomer.ApplicationSrNo);
                        resp.Error = 0;
                        resp.Message = "Token refresh successfully";
                        return resp;
                    }

                }

                if (userRole == "Salesman")
                {
                    var isValidSalesman = await _salesmanService.GetSalesmenByMobileNoAsync(userPhoneNumber!);
                    if (isValidSalesman != null && isValidSalesman.MobileNo == userPhoneNumber && isValidSalesman.EmpCode == int.Parse(tableId!))
                    {
                        var roleName = "Salesman";
                        var user = new IdentityUser
                        {
                            UserName = userPhoneNumber,
                            Id = userIdClaim!,
                            PhoneNumber = userPhoneNumber,
                        };

                        resp.Data.Token = GenerateJwtToken(user, roleName, isValidSalesman.EmpCode);
                        resp.Error = 0;
                        resp.Message = "Token refresh successfully";
                        return resp;
                    }
                }

                return resp;
            }
            catch (Exception ex)
            {
                await _loggerService.InsertLogAsync(ex.Message);
                return new LoginResponse
                {
                    Error = 1,
                    Message = ex.Message
                };
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Route("companyMaster/edit/{companyId}")]
        public async Task<CompanyMasterResponse> EditAsync([FromBody] CompanyMasterModel companyMasterModel, int companyId)
        {
            try
            {
                CompanyMasterResponse resp = new();
                var companyMaster = await _companyMastersService.GetCompanyMasterBySrNoAsync(companyId);
                if (companyMaster != null)
                {
                    if (!String.IsNullOrEmpty(companyMasterModel?.Logo))
                    {
                        string savedFilename = _itemService.SaveImage(companyMasterModel.Logo);
                        companyMasterModel.Logo = savedFilename ?? string.Empty;
                    }

                    _mapper.Map(companyMasterModel, companyMaster);
                    await _companyMastersService.UpdateCompanyMastersAsync(companyMaster);

                    resp.Error = 0;
                    var companyMasters = _mapper.Map<CompanyMasterResponseModel>(companyMaster);
                    resp.Data.CompanyModels.Add(companyMasters);
                    resp.Message = "Company successfully edited.";
                    return resp;
                }

                resp.Message = "No company found Please enter a valid CompanyID.";
                resp.Error = 1;
                return resp;
            }
            catch (Exception ex)
            {
                await _loggerService.InsertLogAsync(ex.Message);
                return new CompanyMasterResponse
                {
                    Error = 1,
                    Message = ex.Message
                };
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        [Route("companyMaster/searchcompanymaster/")]
        public async Task<CompanyMasterResponse> SearchCompanyMasterAsync(int companyId, string companyName, int pageNo = 1, int pageSize = 10)
        {
            try
            {
                CompanyMasterResponse resp = new();
                var companyList = await _companyMastersService.GetAllCompanyMasterAsync(companyId, companyName, pageIndex: pageNo - 1, pageSize: pageSize);
                if (companyList.TotalCount == 0)
                {
                    resp.Message = "Record not found.";
                    resp.Error = 1;
                    return resp;
                }
                foreach (var item in companyList)
                {
                    var companyMaster = _mapper.Map<CompanyMasterResponseModel>(item);
                    resp.Data.CompanyModels.Add(companyMaster);
                }

                resp.Data.PageIndex = pageNo;
                resp.Data.TotalCount = companyList.TotalCount;
                resp.Data.HasNextPage = companyList.HasNextPage;
                resp.Message = "Successfully get all record by companyMasterId.";
                resp.Error = 0;
                return resp;
            }
            catch (Exception ex)
            {
                await _loggerService.InsertLogAsync(ex.Message);
                return new CompanyMasterResponse
                {
                    Error = 1,
                    Message = ex.Message
                };
            }
        }

        #endregion
    }
}
