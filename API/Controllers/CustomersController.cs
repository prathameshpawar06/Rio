using AutoMapper;
using BAL.Response;
using BAL.Services;
using BAL.ViewModels;
using DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class CustomersController : Controller
    {
        #region Fields

        private readonly IMapper _mapper;
        private readonly CustomerService _customersService;
        private readonly BranchMasterService _branchMasterService;
        private readonly LoggerService _loggerService;

        #endregion

        #region Ctor

        public CustomersController(IMapper mapper,
           CustomerService customersService,
           BranchMasterService branchMaster,
           LoggerService loggerService)
        {
            _mapper = mapper;
            _customersService = customersService;
            _branchMasterService = branchMaster;
            _loggerService = loggerService;
        }

        #endregion

        #region Methods
       //[Authorize(Roles = "Admin")]
        [HttpGet]
        [Route("customer/searchcustomer/")]
        public async Task<CustomerResponse> SearchCustomerAsync(string customerName, int customerId = 0, int pageNo = 1, int pageSize = 10, int companyId = 0, int branchId = 0)
        {
            try
            {
                CustomerResponse resp = new();
                var customerList = await _customersService.GetAllCustomerAsync(customerName, customerId, companyId, branchId, pageIndex: pageNo - 1, pageSize: pageSize);
                foreach (var item in customerList)
                {
                    var customers = _mapper.Map<CustomerResponseModel>(item);
                    resp.Data.CustomerModel.Add(customers);
                }
                if (resp.Data.CustomerModel.Count == 0)
                {
                    resp.Message = "Customer record not found.";
                    resp.Error = 1;
                    return resp;
                }

                resp.Data.PageIndex = pageNo;
                resp.Data.TotalCount = customerList.TotalCount;
                resp.Data.HasNextPage = customerList.HasNextPage;
                resp.Message = "All records successfully retrieved by CompanyID and BranchId.";
                resp.Error = 0;
                return resp;
            }
            catch (Exception ex)
            {
                await _loggerService.InsertLogAsync(ex.Message);
                return new CustomerResponse
                {
                    Error = 1,
                    Message = ex.Message
                };
            }
        }

        //[Authorize(Roles = "Admin,Salesman")]
        [HttpPost]
        [Route("customer/create/{branchSrNo}")]
        public async Task<CustomerResponse> CreateAsync([FromBody] CustomerModel customerModel, int branchSrNo)
        {
            try
            {
                CustomerResponse resp = new();
                var branch = await _branchMasterService.GetBranchMasterBySrNoAsync(branchSrNo);
                if (customerModel != null && branch != null)
                {
                    var customer = _mapper.Map<Customer>(customerModel);
                    customer.COCODE = Guid.NewGuid().ToString();
                    customer.SerialNO = Guid.NewGuid().ToString();
                    customer.VATNO = branch.VATNO;
                    customer.CRNO = branch.CRNO;
                    await _customersService.CreateCustomerAsync(customer);

                    //insert the new branch customer mapping
                    await _branchMasterService.CreateBrachCustomerMappingAsync(new BranchMaster_CustomerMapping
                    {
                        branchMasterId = branchSrNo,
                        CustomerId = customer.ApplicationSrNo
                    });

                    resp.Error = 0;
                    resp.Data.CustomerModel.Add(_mapper.Map<CustomerResponseModel>(customer));
                    resp.Message = "Customer created successfully.";
                    return resp;
                }

                resp.Message = branch == null ? "Please enter a valid BranchId" : "Please reenter the data.";
                resp.Error = 1;
                return resp;
            }
            catch (Exception ex)
            {
                await _loggerService.InsertLogAsync(ex.Message);
                return new CustomerResponse
                {
                    Error = 1,
                    Message = ex.Message
                };
            }
        }

        //[Authorize(Roles = "Admin,Salesman")]
        [HttpPost]
        [Route("customer/edit/{customerId}")]
        public async Task<CustomerResponse> EditAsync([FromBody] CustomerModel customerModel, int customerId)
        {
            try
            {
                CustomerResponse resp = new();
                var customerBySrNo = await _customersService.GetCustomerByApplicationSrNoAsync(customerId);
                if (customerBySrNo != null)
                {
                    _mapper.Map(customerModel, customerBySrNo);
                    await _customersService.UpdateCustomerAsync(customerBySrNo);

                    resp.Error = 0;
                    resp.Data.CustomerModel.Add(_mapper.Map<CustomerResponseModel>(customerBySrNo));
                    resp.Message = "Customer record updated successfully.";
                    return resp;
                }

                resp.Message = "Customer not found.";
                resp.Error = 1;
                return resp;
            }
            catch (Exception ex)
            {
                await _loggerService.InsertLogAsync(ex.Message);
                return new CustomerResponse
                {
                    Error = 1,
                    Message = ex.Message
                };
            }
        }

        //[Authorize(Roles = "Admin")]
        [HttpPost]
        [Route("customer/delete/{customerId}")]
        public async Task<CustomerResponse> DeleteAsync(int customerId)
        {
            try
            {
                CustomerResponse resp = new();
                var customersBySrno = await _customersService.GetCustomerByApplicationSrNoAsync(customerId);
                if (customersBySrno != null)
                {
                    await _customersService.DeleteCustomerAsync(customersBySrno);

                    resp.Error = 0;
                    resp.Message = "Customer record has been successfully deleted.";
                    return resp;
                }

                resp.Message = "Customer not found";
                resp.Error = 1;
                return resp;
            }
            catch (Exception ex)
            {
                await _loggerService.InsertLogAsync(ex.Message);
                return new CustomerResponse
                {
                    Error = 1,
                    Message = ex.Message
                };
            }
        }

        #endregion
    }
}
