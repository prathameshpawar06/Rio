using AutoMapper;
using BAL.Response;
using BAL.Services;
using BAL.ViewModels;
using DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    //[Authorize(Roles = "Admin")]
    [Authorize]
    public class BranchMastersController : Controller
    {
        #region Filed 

        private readonly IMapper _mapper;
        private readonly BranchMasterService _branchMasterService;
        private readonly CompanyMastersService _companyMasterService;
        private readonly LoggerService _loggerService;

        #endregion

        #region Ctor

        public BranchMastersController(IMapper mapper,
            BranchMasterService branchMasterService,
            CompanyMastersService companyMasterService,
            LoggerService loggerService)
        {
            _mapper = mapper;
            _branchMasterService = branchMasterService;
            _companyMasterService = companyMasterService;
            _loggerService = loggerService;
        }

        #endregion

        #region Methods

        [HttpGet]
        [Route("branch/searchBranch/")]
        public async Task<BranchMastersResponse> SearchBranchAsync(string branchname, int pageNo = 1, int pageSize = 10, int companyId = 0)
        {
            try
            {
                BranchMastersResponse resp = new();
                var branch = await _branchMasterService.GetAllBranchMasterAsync(branchname: branchname, compnayId: companyId, pageIndex: pageNo - 1, pageSize: pageSize);

                if (branch.Count == 0)
                {
                    resp.Message = "Record not found.";
                    resp.Error = 1;
                    return resp;
                }
                foreach (var item in branch)
                {
                    var branchMaster = _mapper.Map<BranchMasterResponseModel>(item);
                    resp.Data.BranchModels.Add(branchMaster);
                }

                resp.Data.PageIndex = pageNo;
                resp.Data.TotalCount = branch.TotalCount;
                resp.Data.HasNextPage = branch.HasNextPage;
                resp.Message = "Successfully retrieved all records by companyMasterId.";
                resp.Error = 0;
                return resp;
            }
            catch (Exception ex)
            {
                await _loggerService.InsertLogAsync(ex.Message);
                return new BranchMastersResponse
                {
                    Error = 1,
                    Message = ex.Message
                };
            }

        }

        [HttpPost]
        [Route("branch/create/")]
        public async Task<BranchMastersResponse> CreateAsync([FromBody] BranchMasterModel branchMasterModel, int comapnySrNo)
        {
            try
            {
                BranchMastersResponse resp = new();
                var companyMasterById = await _companyMasterService.GetCompanyMasterBySrNoAsync(comapnySrNo);
                var brachCodeUnique = await _branchMasterService.BrachCodeIsUnique(branchMasterModel.BranchCode);
                if (companyMasterById == null || brachCodeUnique)
                {
                    resp.Error = 1;
                    resp.Message = brachCodeUnique ? "Please enter a unique Branch Code." : "Company Master not found. Please enter a valid CompanyID.";
                    return resp;
                }

                BranchMaster branchMaster = _mapper.Map<BranchMaster>(branchMasterModel);
                branchMaster.MobileNo = "";
                branchMaster.MobileNoOTP = "";
                branchMaster.CRNO = companyMasterById.CRNO;
                branchMaster.VATNO = companyMasterById.VATNO;
                branchMaster.COCODE = Guid.NewGuid().ToString();
                branchMaster.SerialNO = Guid.NewGuid().ToString();
                branchMaster.BranchCode = branchMasterModel.BranchCode;
                await _branchMasterService.CreateBranchMasterAsync(branchMaster);
                await _companyMasterService.CreateCompanyBrachMappingAsync(branchMaster, comapnySrNo);

                resp.Data.BranchModels.Add(_mapper.Map<BranchMasterResponseModel>(branchMaster));
                resp.Error = 0;
                resp.Message = "Branch has been registered successfully.";
                return resp;
            }
            catch (Exception ex)
            {
                await _loggerService.InsertLogAsync(ex.Message);
                return new BranchMastersResponse
                {
                    Error = 1,
                    Message = ex.Message
                };
            }
        }

        [HttpPost]
        [Route("branch/edit/{branchId}")]
        public async Task<BranchMastersResponse> EditAsync([FromBody] BranchMasterModel branchMasterModel, int branchId)
        {
            try
            {
                BranchMastersResponse resp = new();
                var branchBySrNo = await _branchMasterService.GetBranchMasterBySrNoAsync(branchId);
                if (branchBySrNo == null)
                {
                    resp.Error = 1;
                    resp.Message = "BranchMaster not found. Please provide the correct branch ApplicationId.";
                    return resp;
                }

                _mapper.Map(branchMasterModel, branchBySrNo);
                await _branchMasterService.UpdateBranchMasterAsync(branchBySrNo);

                resp.Error = 0;
                resp.Data.BranchModels.Add(_mapper.Map<BranchMasterResponseModel>(branchBySrNo));
                resp.Message = "Branch details have been updated successfully.";
                return resp;


            }
            catch (Exception ex)
            {
                await _loggerService.InsertLogAsync(ex.Message);
                return new BranchMastersResponse
                {
                    Error = 1,
                    Message = ex.Message
                };
            }
        }

        [HttpPost]
        [Route("branch/delete/{branchId}")]
        public async Task<ResponseModel> DeleteAsync(int branchId)
        {
            try
            {
                ResponseModel resp = new();
                var branchBySrno = await _branchMasterService.GetBranchMasterBySrNoAsync(branchId);
                if (branchBySrno == null)
                {
                    resp.Error = 1;
                    resp.Message = "Branch not found.";
                    return resp;
                }

                await _branchMasterService.DeleteBranchMasterAsync(branchBySrno);
                resp.Error = 0;
                resp.Message = "BranchMaster has been deleted successfully.";
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

        #endregion
    }
}
