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
    public class SalesReturnsController : Controller
    {
        #region Fields

        private readonly SalesReturnService _salesReturnService;
        private readonly IMapper _mapper;
        private readonly LoggerService _loggerService;

        #endregion

        #region Ctor

        public SalesReturnsController(SalesReturnService salesReturnService,
            IMapper mapper,
            LoggerService loggerService)
        {
            _salesReturnService = salesReturnService;
            _mapper = mapper;
            _loggerService = loggerService;
        }

        #endregion

        #region Method

        [HttpGet]
        [Route("salesreturn/getsalesreturnbyapplicationsrno/{salesReturnId}")]
        public async Task<SalesReturnResponse> GetSalesReturnByApplicationSrNoAsync(int salesReturnId)
        {
            try
            {
                SalesReturnResponse resp = new();
                var salesReturn = await _salesReturnService.GetSalesReturnByApplicationSrNoAsync(salesReturnId);
                if (salesReturn == null)
                {
                    resp.Message = "SalesReturn record not found Please enter the correct ApplicationSrNo.";
                    resp.Error = 1;
                    return resp;
                }

                resp.Data.Add(_mapper.Map<SalesReturnResponseModel>(salesReturn));
                resp.Message = "Successfully retrieved SalesReturn record by ApplicationSrNo.";
                resp.Error = 0;
                return resp;
            }
            catch (Exception ex)
            {
                await _loggerService.InsertLogAsync(ex.Message);
                return new SalesReturnResponse
                {
                    Error = 1,
                    Message = ex.Message
                };
            }
        }

        [HttpPost]
        [Route("salesreturn/create/")]
        public async Task<SalesReturnResponse> CreateAsync([FromBody] SalesReturnModel salesReturnModel)
        {
            try
            {
                var salesReturn = _mapper.Map<SalesReturn>(salesReturnModel);
                salesReturn.ItemCode = salesReturnModel.ItemCode;
                salesReturn.COCODE = Guid.NewGuid().ToString();
                salesReturn.SerialNO = Guid.NewGuid().ToString();
                await _salesReturnService.CreateSalesReturnAsync(salesReturn);

                SalesReturnResponse resp = new();
                resp.Error = 0;
                resp.Data.Add(_mapper.Map<SalesReturnResponseModel>(salesReturn));
                resp.Message = "Sales Return created successfully.";
                return resp;
            }
            catch (Exception ex)
            {
                await _loggerService.InsertLogAsync(ex.Message);
                return new SalesReturnResponse
                {
                    Error = 1,
                    Message = ex.Message
                };
            }
        }

        [HttpPost]
        [Route("salesreturn/edit/{salesReturnId}")]
        public async Task<SalesReturnResponse> EditAsync([FromBody] SalesReturnModel salesReturnModel, int salesReturnId)
        {
            try
            {
                SalesReturnResponse resp = new();
                var SalesReturnBySrNO = await _salesReturnService.GetSalesReturnByApplicationSrNoAsync(salesReturnId);
                if (SalesReturnBySrNO != null)
                {
                    _mapper.Map(salesReturnModel, SalesReturnBySrNO);
                    await _salesReturnService.UpdateSalesReturnAsync(SalesReturnBySrNO);

                    resp.Error = 0;
                    resp.Data.Add(_mapper.Map<SalesReturnResponseModel>(SalesReturnBySrNO));
                    resp.Message = "SalesReturn updated successfully.";
                    return resp;
                }

                resp.Message = "SalesReturn not found.";
                resp.Error = 1;
                return resp;
            }
            catch (Exception ex)
            {
                await _loggerService.InsertLogAsync(ex.Message);
                return new SalesReturnResponse
                {
                    Error = 1,
                    Message = ex.Message
                };
            }
        }

        [HttpPost]
        [Route("salesreturn/delete/{salesReturnId}")]
        public async Task<ResponseModel> DeleteAsync(int salesReturnId)
        {
            try
            {
                ResponseModel resp = new();
                var SalesReturnBySrno = await _salesReturnService.GetSalesReturnByApplicationSrNoAsync(salesReturnId);
                if (SalesReturnBySrno != null)
                {
                    await _salesReturnService.DeleteSalesReturnAsync(SalesReturnBySrno);

                    resp.Error = 0;
                    resp.Message = "SalesReturn data deleted successfully.";
                    return resp;
                }

                resp.Message = "SalesReturn not found";
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

        #endregion
    }
}
