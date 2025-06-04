using DAL.Context;
using DAL.Helper;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace BAL.Services
{
    public class SalesmanService
    {
        #region Fields

        private readonly RioContext _context;

        #endregion

        #region Ctor

        public SalesmanService(RioContext context)
        {
            _context = context;
        }

        #endregion

        #region Methods

        public async Task CreateSalesManAsync(Salesman salesman)
        {
            await _context.Salesmans.AddAsync(salesman);
            await _context.SaveChangesAsync();
        }

        public async Task<Salesman> GetSalesManByEmpCodeAsync(int empCode)
        {
            var salesmanempCode = await _context.Salesmans.FirstOrDefaultAsync(x => x.EmpCode == empCode && !x.Deleted);

            return salesmanempCode;
        }

        public async Task UpdateSalesManAsync(Salesman salesman)
        {
            _context.Salesmans.Update(salesman);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteSalesManAsync(Salesman salesman)
        {
            salesman.Deleted = true;
            _context.Salesmans.Update(salesman);
            await _context.SaveChangesAsync();
        }

        public async Task<IPagedList<Salesman>> GetAllSalesmenAsync(int companyId = 0, int branchId = 0, int salesManId = 0, string salesManName = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var salesManList = _context.Salesmans.Where(x => !x.Deleted);

            var salesMan = from newSalesMan in salesManList
                           join bs in _context.BranchMaster_SalesmanMappings
                           on newSalesMan.EmpCode equals bs.SalesManId
                           join cm in _context.CompanyMaster_BranchMasterMappings
                           on bs.BranchMasterId equals cm.BranchMasterId
                           select new
                           {
                               SalesMan = newSalesMan,
                               compnayBrachMapping = cm
                           };

            //By Category Name
            if (!string.IsNullOrWhiteSpace(salesManName))
                salesMan = salesMan.Where(x => x.SalesMan.SalesMan.ToLower().Contains(salesManName.Trim().ToLower()));

            if (companyId > 0)
                salesMan = salesMan.Where(x => x.compnayBrachMapping.ComanyMasterId == companyId);

            if (branchId > 0)
                salesMan = salesMan.Where(x => x.compnayBrachMapping.BranchMasterId == branchId);

            if (salesManId > 0)
                salesMan = salesMan.Where(x => x.SalesMan.EmpCode == salesManId);

            var result = await salesMan.Select(x => x.SalesMan).ToListAsync();

            return new PagedList<Salesman>(result, pageIndex, pageSize);
        }

        public async Task<Salesman> GetSalesmenByMobileNoAsync(string mobileNo)
        {
            var salesman = await _context.Salesmans.Where(x => x.MobileNo == mobileNo && !x.Deleted).AsNoTracking().FirstOrDefaultAsync();

            return salesman;
        }

        //Get brachId and CompanyId by salesmanId
        public async Task<(int brachId, int companyId)> GetDetailsBySalesManAsync(int salesManId)
        {
            var salesMan = await _context.Salesmans.FirstOrDefaultAsync(x => x.EmpCode == salesManId);

            if (salesMan is null)
            {
                return ( 0, 0); // Return a tuple with all values set to 0 if salesMan is null
            }

            var brachId = (await _context.BranchMaster_SalesmanMappings.FirstOrDefaultAsync(x => x.SalesManId == salesManId))?.BranchMasterId;

            var companyId = (await _context.CompanyMaster_BranchMasterMappings.FirstOrDefaultAsync(x => x.BranchMasterId == brachId))?.ComanyMasterId;

            return (brachId ?? 0, companyId ?? 0);
        }

        #endregion
    }
}
