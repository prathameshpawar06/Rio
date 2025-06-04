using AutoMapper;
using BAL.Response;
using BAL.Services;
using BAL.ViewModels;
using DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize(Roles = "Admin,Salesman")]
    public class ShoppingCartController : Controller
    {
        #region Fields

        private readonly ShoppingCartService _shoppingCartService;
        private readonly LoggerService _loggerService;
        private readonly CompanyMastersService _companyMastersService;
        private readonly BranchMasterService _branchMasterService;
        private readonly SalesmanService _salesmanService;
        private readonly CustomerService _customerService;
        private readonly ItemService _itemService;
        private readonly IMapper _mapper;

        #endregion

        #region Ctor

        public ShoppingCartController(ShoppingCartService shoppingCartService,
            LoggerService loggerService,
            CompanyMastersService companyMastersService,
            IMapper mapper,
            BranchMasterService branchMasterService,
            SalesmanService salesmanService,
            CustomerService customerService,
            ItemService itemService)
        {
            _shoppingCartService = shoppingCartService;
            _loggerService = loggerService;
            _companyMastersService = companyMastersService;
            _mapper = mapper;
            _branchMasterService = branchMasterService;
            _salesmanService = salesmanService;
            _customerService = customerService;
            _itemService = itemService;
        }

        #endregion

        #region Methods 

        [HttpPost]
        [Route("shoppingcart/orderconfirm/")]
        public async Task<OrderResponse> OrderConfirmAsync([FromBody] OrderViewModel orderViewModel)
        {
            try
            {
                // Check if the provided OrderViewModel is null
                if (orderViewModel is null)
                {
                    return new OrderResponse
                    {
                        Error = 1,
                        Message = "Please enter valid data."
                    };
                }

                // Check if the customer with the given CustomerId exists
                var customer = await _customerService.GetCustomerByApplicationSrNoAsync(orderViewModel.CustomerId);
                if (customer is null)
                {
                    return new OrderResponse
                    {
                        Error = 1,
                        Message = "Please enter a correct customer Id."
                    };
                }

                // Get details (branchId and companyId) based on the SalesManId
                var (branchId, companyId) = await _salesmanService.GetDetailsBySalesManAsync(orderViewModel.SalesManId);
                if (branchId == 0 && companyId == 0)
                {
                    return new OrderResponse
                    {
                        Error = 1,
                        Message = "Salesman is not registered with the correct branch and company."
                    };
                }

                // Confirm the order using the provided details
                var order = await _shoppingCartService.ConfirmOrder(orderViewModel, companyId, branchId);

                // Check if the order confirmation was successful
                if (order is null)
                {
                    return new OrderResponse
                    {
                        Error = 1,
                        Message = "Please check your order confirm details and try again."
                    };
                }

                // Build the response with the confirmed order details
                var response = new OrderResponse
                {
                    Error = 0,
                    Data = new OrderPaginationResponse
                    {
                        OrderResponseModels = await _shoppingCartService.GetOrderList(order.Id)
                    },
                    Message = "Order is confirmed."
                };

                return response;
            }
            catch (Exception ex)
            {
                // Log any exception that occurs during order confirmation
                await _loggerService.InsertLogAsync(ex.Message);

                // Return an error response with the exception message
                return new OrderResponse
                {
                    Error = 1,
                    Message = ex.Message
                };
            }
        }

        [HttpGet]
        [Route("shoppingcart/orderlist/")]
        public async Task<OrderResponse> OrderList(int orderId, int companyId, int branchId, int salesmanId, int pageNo = 1, int pageSize = 1)
        {
            try
            {
                OrderResponse resp = new();
                var orderList = await _shoppingCartService.GetOrderList(orderId, companyId, branchId, salesmanId, pageIndex: pageNo - 1, pageSize: pageSize);
                if (orderList.TotalCount >= 1)
                {
                    resp.Data.OrderResponseModels = orderList;
                    resp.Error = 0;
                    resp.Data.PageIndex = pageNo;
                    resp.Data.TotalCount = orderList.TotalCount;
                    resp.Data.HasNextPage = orderList.HasNextPage;
                    resp.Message = orderList.Count == 0 ? "There are currently no pending orders." : "Successfully retrieved the list of all orders.";
                    return resp;
                }

                resp.Message = "Please enter a valid CompanyId,BranchId,SalesmanId.";
                return resp;
            }
            catch (Exception ex)
            {
                await _loggerService.InsertLogAsync(ex.Message);
                return new OrderResponse
                {
                    Error = 1,
                    Message = ex.Message
                };
            }
        }

        [HttpPost("shoppingcart/updatepaymentstatus/{orderId}/{newPaymentStatus}")]
        public async Task<OrderResponse> UpdatePaymentStatus(int orderId, PaymentStatus newPaymentStatus)
        {
            var orderById = await _shoppingCartService.GetOrderByIdAsync(orderId);
            if (orderById == null)
            {
                return new OrderResponse
                {
                    Error = 1,
                    Message = "Order not found,Plase enter valied orderId"
                };
            }

            // Check if the provided newPaymentStatus is a valid enum value
            if (!Enum.IsDefined(typeof(PaymentStatus), newPaymentStatus))
            {
                return new OrderResponse
                {
                    Error = 1,
                    Message = "Enter valid payment status value"
                };
            }

            if (orderById.PaymentStatusId == 1)
            {
                return new OrderResponse
                {
                    Error = 1,
                    Message = "Payment is already completed; you cannot change the booking status."
                };
            }

            await _shoppingCartService.UpdatePaymentStatus(orderById, newPaymentStatus);

            OrderPaginationResponse resp = new()
            {
                OrderResponseModels = await _shoppingCartService.GetOrderList(orderId)
            };

            return new OrderResponse
            {
                Error = 0,
                Data = resp,
                Message = "The payment status is updated successfully",
            };
        }

        #endregion
    }
}
