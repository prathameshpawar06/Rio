using BAL.Response;
using BAL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    //[Authorize(Roles = "Admin")]
    [Authorize]
    public class DashBoardController : Controller
    {
        private readonly ShoppingCartService _shoppingCartService;
        private readonly LoggerService _loggerService;

        public DashBoardController(ShoppingCartService shoppingCartService,
            LoggerService loggerService)
        {
            _shoppingCartService = shoppingCartService;
            _loggerService = loggerService;
        }

        [HttpGet]
        [Route("dashBoard/salesReport/")]
        public async Task<DashBoardResponse> SalesReportAsync(DateTime? startDate = null, DateTime? endDate = null, int companyId = 0, int pageNo = 1, int pageSize = 10)
        {
            try
            {
                DashBoardResponse resp = new();

                var salesReportList = await _shoppingCartService.GetOrderDetailsAsync(startDate, endDate, companyId, pageIndex: pageNo - 1, pageSize: pageSize);
                if (salesReportList.TotalCount == 0)
                {
                    resp.Error = 1;
                    resp.Message = "No record found Please enter valid data.";
                    return resp;
                }

                var saleQty = 0;
                decimal totalSaleAmount = 0;
                foreach (var item in salesReportList)
                {
                    saleQty += item.ItemsQuantity;
                    totalSaleAmount += item.TotalAmount;
                }

                resp.Data.SaleQty = saleQty;
                resp.Data.TotalSaleAmount = totalSaleAmount;
                resp.Data.SalesReportModels = salesReportList;
                resp.Data.PageIndex = pageNo;
                resp.Data.TotalCount = salesReportList.TotalCount;
                resp.Data.HasNextPage = salesReportList.HasNextPage;
                resp.Message = "All records retrieved successfully.";
                resp.Error = 0;
                return resp;
            }
            catch (Exception ex)
            {
                await _loggerService.InsertLogAsync(ex.Message);
                return new DashBoardResponse
                {
                    Error = 1,
                    Message = ex.Message
                };
            }
        }
    }
}
