using AutoMapper;
using BAL.Response;
using BAL.Services;
using BAL.ViewModels;
using DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class SalesmansController : Controller
    {
        #region Field

        private readonly IMapper _mapper;
        private readonly SalesmanService _salesmanService;
        private readonly BranchMasterService _branchMasterService;
        private readonly LoggerService _loggerService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        #endregion

        #region Ctor

        public SalesmansController(IMapper mapper,
            SalesmanService salesmanService,
            BranchMasterService branchMasterService,
            LoggerService loggerService,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _mapper = mapper;
            _salesmanService = salesmanService;
            _branchMasterService = branchMasterService;
            _loggerService = loggerService;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        #endregion

        #region Methods

        [HttpPost]
        [Route("salesman/create/{branchId}")]
        public async Task<SalesmansResponse> CreateAsync([FromBody] SalesmanModel salesmanModel, int branchId)
        {
            try
            {
                SalesmansResponse resp = new();
                resp.Error = 1;
                if (salesmanModel == null)
                {
                    resp.Message = "Please enter data.";
                    return resp;
                }

                var userExite = await _userManager.FindByNameAsync(salesmanModel.MobileNo);
                if (userExite != null)
                {
                    resp.Message = "User alreday exists!";
                    return resp;
                }

                var branchMaster = await _branchMasterService.GetBranchMasterBySrNoAsync(branchId);
                if (branchMaster == null)
                {
                    resp.Message = "Branch not found";
                    return resp;
                }

                //Add the user in the database 
                IdentityUser user = new()
                {
                    SecurityStamp = Guid.NewGuid().ToString(),
                    PhoneNumber = salesmanModel.MobileNo,
                    UserName = salesmanModel.MobileNo
                };

                var result = await _userManager.CreateAsync(user);

                if (!result.Succeeded)
                {
                    resp.Message = "User creation failed.";
                    return resp;
                }

                if (!await _roleManager.RoleExistsAsync("Salesman"))
                    await _roleManager.CreateAsync(new IdentityRole("Salesman"));

                var newSalesman = _mapper.Map<Salesman>(salesmanModel);
                newSalesman.BranchCode = branchMaster.BranchCode ?? string.Empty;
                newSalesman.BranchName = branchMaster.BranchName ?? string.Empty;
                newSalesman.MobileNoOTP = "";
                newSalesman.COCODE = Guid.NewGuid().ToString();
                newSalesman.SerialNO = Guid.NewGuid().ToString();

                await _salesmanService.CreateSalesManAsync(newSalesman);

                // Insert the new product category mapping
                await _branchMasterService.CreateBrachSalesmanMappingAsync(new BranchMaster_SalesmanMapping
                {
                    BranchMasterId = branchId,
                    SalesManId = newSalesman.EmpCode
                });

                if (await _roleManager.RoleExistsAsync("Salesman"))
                    await _userManager.AddToRoleAsync(user, "Salesman");

                resp.Error = 0;
                resp.Data.SalesmanModel.Add(_mapper.Map<SalesmanResponseModel>(newSalesman));
                resp.Message = "Salesman created successfully.";
                return resp;
            }
            catch (Exception ex)
            {
                await _loggerService.InsertLogAsync(ex.Message);
                return new SalesmansResponse
                {
                    Error = 1,
                    Message = ex.Message
                };
            }
        }

        [Authorize(Roles = "Salesman,Admin")]
        [HttpPost]
        [Route("salesman/edit/{empCode}")]
        public async Task<SalesmansResponse> EditAsync([FromBody] SalesmanModel salesmanModel, int empCode)
        {
            try
            {
                SalesmansResponse resp = new();
                var salesMan = await _salesmanService.GetSalesManByEmpCodeAsync(empCode);
                if (salesMan != null)
                {
                    salesMan.SalesMan = salesmanModel.SalesMan;
                    salesMan.SalesManAR = salesmanModel.SalesManAR;
                    salesMan.GPS = salesmanModel.GPS;
                    await _salesmanService.UpdateSalesManAsync(salesMan);

                    resp.Error = 0;
                    resp.Data.SalesmanModel.Add(_mapper.Map<SalesmanResponseModel>(salesMan));
                    resp.Message = "Salesman updated successfully.";
                    return resp;
                }

                resp.Message = "Salesman not found Please enter a valid ID.";
                resp.Error = 1;
                return resp;
            }
            catch (Exception ex)
            {
                await _loggerService.InsertLogAsync(ex.Message);
                return new SalesmansResponse
                {
                    Error = 1,
                    Message = ex.Message
                };
            }
        }

        //[Authorize(Roles = "Admin")]
        [Authorize]
        [HttpPost]
        [Route("salesman/delete/{empCode}")]
        public async Task<ResponseModel> DeleteAsync(int empCode)
        {
            try
            {
                ResponseModel resp = new();
                var salesmansBySrno = await _salesmanService.GetSalesManByEmpCodeAsync(empCode);
                if (salesmansBySrno != null)
                {
                    await _salesmanService.DeleteSalesManAsync(salesmansBySrno);

                    resp.Error = 0;
                    resp.Message = "Record has been successfully deleted.";
                    return resp;
                }

                resp.Message = "Salesman not found";
                resp.Error = 1;
                return resp;
            }
            catch (Exception ex)
            {
                await _loggerService.InsertLogAsync(ex.Message);
                return new ResponseModel
                {
                    Error = 1,
                    Message = ex.Message
                };
            }
        }

        //[Authorize(Roles = "Admin")]
        [Authorize]
        [HttpGet]
        [Route("salesMan/searchsalesman/")]
        public async Task<SalesmansResponse> SearchSalesManAsync(int salesManId, string salesManName, int companyId = 0, int branchId = 0, int pageNo = 1, int pageSize = 10)
        {
            try
            {
                SalesmansResponse resp = new();
                var salesManList = await _salesmanService.GetAllSalesmenAsync(companyId, branchId, salesManId, salesManName, pageIndex: pageNo - 1, pageSize: pageSize);

                foreach (var item in salesManList)
                {
                    var salesman = _mapper.Map<SalesmanResponseModel>(item);
                    resp.Data.SalesmanModel.Add(salesman);
                }

                resp.Data.PageIndex = pageNo;
                resp.Data.TotalCount = salesManList.TotalCount;
                resp.Data.HasNextPage = salesManList.HasNextPage;
                resp.Message = resp.Data.SalesmanModel.Count == 0 ? "Salesman record not found." : "Successfully retrieved all records by CompanyID and BranchId.";
                resp.Error = resp.Data.SalesmanModel.Count == 0 ? 1 : 0;
                return resp;
            }
            catch (Exception ex)
            {
                await _loggerService.InsertLogAsync(ex.Message);
                return new SalesmansResponse
                {
                    Error = 1,
                    Message = ex.Message
                };
            }
        }

        #endregion
    }
}
