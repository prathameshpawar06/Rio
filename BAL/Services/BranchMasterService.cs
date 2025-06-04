using DAL.Context;
using DAL.Helper;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace BAL.Services
{
    public class BranchMasterService
    {
        #region Fields

        private readonly RioContext _context;

        #endregion

        #region Ctor

        public BranchMasterService(RioContext context)
        {
            _context = context;
        }

        #endregion

        #region Methods

        public async Task<bool> BrachCodeIsUnique(string branchCode)
        {
            var isUnique = await _context.BranchMasters.AnyAsync(x => x.BranchCode.Contains(branchCode));
            return isUnique;
        }

        public async Task CreateBranchMasterAsync(BranchMaster branchMaster)
        {
            await _context.BranchMasters.AddAsync(branchMaster);
            await _context.SaveChangesAsync();
        }

        public async Task<BranchMaster> GetBranchMasterBySrNoAsync(int applicationSrNo)
        {
            if (applicationSrNo == 0)
                return null;

            var branchBySrNo = await _context.BranchMasters.FindAsync(applicationSrNo);

            return branchBySrNo;
        }

        public async Task UpdateBranchMasterAsync(BranchMaster branchMaster)
        {
            _context.BranchMasters.Update(branchMaster);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteBranchMasterAsync(BranchMaster branchMaster)
        {
            branchMaster.Deleted = true;
            _context.BranchMasters.Update(branchMaster);
            await _context.SaveChangesAsync();
        }

        public async Task<IPagedList<BranchMaster>> GetAllBranchMasterAsync(string branchname = null, int compnayId = 0, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var branchList = _context.BranchMasters.Where(x => !x.Deleted);

            if (compnayId > 0)
                branchList = (from b in branchList
                              join cb in _context.CompanyMaster_BranchMasterMappings
                              on b.ApplicationSrNo equals cb.BranchMasterId
                              where cb.ComanyMasterId == compnayId
                              select b);

            if (!string.IsNullOrWhiteSpace(branchname))
                branchList = branchList.Where(c => c.BranchName.ToLower().Contains(branchname.Trim().ToLower()));

            return new PagedList<BranchMaster>(await branchList.ToListAsync(), pageIndex, pageSize);

        }

        #region Mapping 

        //Branch Category Mapping Service
        public async Task CreateBrachCategoryMappingAsync(BranchMaster_CategoryMapping categoryMapping)
        {
            await _context.BranchMaster_CategoryMappings.AddAsync(categoryMapping);
            await _context.SaveChangesAsync();
        }

        //Branch Customer Mapping Service
        public async Task CreateBrachCustomerMappingAsync(BranchMaster_CustomerMapping branchMaster_customerMapping)
        {
            await _context.BranchMaster_CustomerMappings.AddAsync(branchMaster_customerMapping);
            await _context.SaveChangesAsync();
        }

        // Brach Salesman Mapping Service
        public async Task CreateBrachSalesmanMappingAsync(BranchMaster_SalesmanMapping branchMaster_SalesmanMapping)
        {
            await _context.BranchMaster_SalesmanMappings.AddAsync(branchMaster_SalesmanMapping);
            await _context.SaveChangesAsync();
        }

        #endregion

        #endregion
    }
}
