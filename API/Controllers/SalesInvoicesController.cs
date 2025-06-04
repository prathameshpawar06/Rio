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
    public class SalesinvoicesController : Controller
    {
        #region Fields

        private readonly IMapper _mapper;
        private readonly SalesinvoiceService _salesinvoiceService;
        private readonly LoggerService _loggerService;

        #endregion

        #region Ctor

        public SalesinvoicesController(IMapper mapper,
            SalesinvoiceService salesinvoiceService,
            LoggerService loggerService)
        {
            _mapper = mapper;
            _salesinvoiceService = salesinvoiceService;
            _loggerService = loggerService;
        }

        #endregion

        #region Method

        [HttpGet]
        [Route("salesinvoice/getsalesinvoicebyapplicationsrno/{invoiceId}")]
        public async Task<SalesInvoiceResponse> GetSalesInvoiceByApplicationSrNoAsync(int invoiceId)
        {
            try
            {
                SalesInvoiceResponse resp = new();
                var salesInvoice = await _salesinvoiceService.GetSalesinvoiceByApplicationSrNoAsync(invoiceId);
                if (salesInvoice == null)
                {
                    resp.Message = "SalesInvoice record not found Please enter the correct ApplicationSrNo.";
                    resp.Error = 1;
                    return resp;
                }

                resp.Data.Add(_mapper.Map<SalesInvoiceResponseModel>(salesInvoice));
                resp.Message = "Successfully retrieved SalesInvoice record by ApplicationSrNo.";
                resp.Error = 0;
                return resp;
            }
            catch (Exception ex)
            {
                await _loggerService.InsertLogAsync(ex.Message);
                return new SalesInvoiceResponse
                {
                    Error = 1,
                    Message = ex.Message
                };
            }
        }

        [HttpPost]
        [Route("salesinvoice/create/")]
        public async Task<SalesInvoiceResponse> CreateAsync([FromBody] SalesInvoiceModel salesInvoiceModel)
        {
            try
            {
                var salesInvoice = _mapper.Map<SalesInvoice>(salesInvoiceModel);
                salesInvoice.ItemCode = salesInvoiceModel.ItemCode;
                salesInvoice.COCODE = Guid.NewGuid().ToString();
                salesInvoice.SerialNO = Guid.NewGuid().ToString();
                await _salesinvoiceService.CreateSalesInvoiceAsync(salesInvoice);

                SalesInvoiceResponse resp = new();
                resp.Error = 0;
                resp.Message = "SalesInvoice generated successfully.";
                return resp;
            }
            catch (Exception ex)
            {
                await _loggerService.InsertLogAsync(ex.Message);
                return new SalesInvoiceResponse
                {
                    Error = 1,
                    Message = ex.Message
                };
            }
        }

        [HttpPost]
        [Route("salesinvoice/edit/{invoiceId}")]
        public async Task<SalesInvoiceResponse> EditAsync([FromBody] SalesInvoiceModel salesInvoiceModel, int invoiceId)
        {
            try
            {
                SalesInvoiceResponse resp = new();
                var salesinvoiceBySrNo = await _salesinvoiceService.GetSalesinvoiceByApplicationSrNoAsync(invoiceId);
                if (salesinvoiceBySrNo != null)
                {
                    _mapper.Map(salesInvoiceModel, salesinvoiceBySrNo);

                    await _salesinvoiceService.UpdateSalesinvoiceAsync(salesinvoiceBySrNo);
                    resp.Error = 0;
                    resp.Data.Add(_mapper.Map<SalesInvoiceResponseModel>(salesinvoiceBySrNo));
                    resp.Message = "SalesInvoice updated successfully.";
                    return resp;
                }

                resp.Message = "SalesInvoice not found.";
                resp.Error = 1;
                return resp;
            }
            catch (Exception ex)
            {
                await _loggerService.InsertLogAsync(ex.Message);
                return new SalesInvoiceResponse
                {
                    Error = 1,
                    Message = ex.Message
                };
            }
        }

        [HttpPost]
        [Route("salesinvoice/delete/{invoiceId}")]
        public async Task<ResponseModel> DeleteAsync(int invoiceId)
        {
            try
            {
                ResponseModel resp = new();
                var salesInvoiceBySrno = await _salesinvoiceService.GetSalesinvoiceByApplicationSrNoAsync(invoiceId);
                if (salesInvoiceBySrno != null)
                {
                    await _salesinvoiceService.DeleteSalesinvoiceAsync(salesInvoiceBySrno);
                    resp.Error = 0;
                    resp.Message = "SalesInvoice deleted successfully.";
                    return resp;
                }

                resp.Message = "SalesInvoice not found";
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
