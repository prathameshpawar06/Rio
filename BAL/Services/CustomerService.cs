using DAL.Context;
using DAL.Helper;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace BAL.Services
{
    public class CustomerService
    {
        #region Fields

        private readonly RioContext _context;

        #endregion

        #region Ctor

        public CustomerService(RioContext context)
        {
            _context = context;
        }

        #endregion

        #region Methods

        public async Task CreateCustomerAsync(Customer customer)
        {
            await _context.Customers.AddAsync(customer);
            await _context.SaveChangesAsync();
        }

        public async Task<Customer> GetCustomerByApplicationSrNoAsync(int applicationSrNo)
        {
            var customerapplicationSrNo = await _context.Customers.FirstOrDefaultAsync(x => x.ApplicationSrNo == applicationSrNo && !x.Deleted);

            return customerapplicationSrNo;
        }

        public async Task UpdateCustomerAsync(Customer customer)
        {
            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCustomerAsync(Customer customer)
        {
            customer.Deleted = true;
            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();
        }

        public async Task<IPagedList<Customer>> GetAllCustomerAsync(string customerName = null, int customerId = 0, int companyId = 0, int branchId = 0, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var customerList = _context.Customers.Where(x => !x.Deleted);

            var customer = from customers in customerList
                           join branchCustomerMapping in _context.BranchMaster_CustomerMappings
                           on customers.ApplicationSrNo equals branchCustomerMapping.CustomerId
                           join branchMapping in _context.CompanyMaster_BranchMasterMappings
                           on branchCustomerMapping.branchMasterId equals branchMapping.BranchMasterId
                           select new
                           {
                               Customer = customers,
                               BranchMapping = branchMapping
                           };

            if (!string.IsNullOrWhiteSpace(customerName))
                customer = customer.Where(c => c.Customer.EstCoName.ToLower().Contains(customerName.Trim().ToLower()));

            if (customerId > 0)
                customer = customer.Where(c => c.Customer.ApplicationSrNo == customerId);

            if (companyId > 0)
                customer = customer.Where(c => c.BranchMapping.ComanyMasterId == companyId);

            if (branchId > 0)
                customer = customer.Where(c => c.BranchMapping.BranchMasterId == branchId);

            var result = await customer.Select(c => c.Customer).ToListAsync();

            return new PagedList<Customer>(result, pageIndex, pageSize);
        }

        #endregion
    }
}
