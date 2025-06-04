using DAL.Context;
using DAL.Helper;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace BAL.Services
{
    public class CompanyMastersService
    {
        #region Fields 

        private readonly RioContext _context;

        #endregion

        #region Ctor

        public CompanyMastersService(RioContext context)
        {
            _context = context;
        }

        #endregion

        #region Methods

        public async Task<CompanyMaster> GetCompanyMasterBySalesManAsync(Salesman salesman)
        {
            var salesmanMapping = await _context.BranchMaster_SalesmanMappings
                .FirstOrDefaultAsync(mapping => mapping.SalesManId == salesman.EmpCode);

            if (salesmanMapping == null)
                return null;

            var branchMapping = await _context.CompanyMaster_BranchMasterMappings
                .FirstOrDefaultAsync(branch => branch.BranchMasterId == salesmanMapping.BranchMasterId);

            if (branchMapping == null)
                return null;

            var companyMaster = await _context.CompanyMasters
                .FirstOrDefaultAsync(company => company.ApplicationSrNo == branchMapping.ComanyMasterId);

            return companyMaster;
        }

        public async Task CreateCompanyMastersAsync(CompanyMaster companyMaster)
        {
            await _context.CompanyMasters.AddAsync(companyMaster);
            await _context.SaveChangesAsync();
        }

        public async Task<CompanyMaster> GetCompanyMasterByMobileNoAsync(string mobile)
        {
            var customer = await _context.CompanyMasters.FirstOrDefaultAsync(x => x.MobileNo == mobile);

            return customer;
        }

        public async Task<CompanyMaster> GetCompanyMasterBySrNoAsync(int applicationSrNo)
        {
            var companyMaster = await _context.CompanyMasters.FindAsync(applicationSrNo);

            return companyMaster;
        }

        public async Task UpdateCompanyMastersAsync(CompanyMaster companyMaster)
        {
            _context.CompanyMasters.Update(companyMaster);
            await _context.SaveChangesAsync();
        }

        public async Task<IPagedList<CompanyMaster>> GetAllCompanyMasterAsync(int companyId = 0, string companyName = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var companyList = _context.CompanyMasters.AsQueryable();

            if (!string.IsNullOrWhiteSpace(companyName))
                companyList = companyList.Where(x => x.EstCoName.ToLower().Contains(companyName.Trim().ToLower()));

            if (companyId > 0)
                companyList = companyList.Where(x => x.ApplicationSrNo == companyId);

            return new PagedList<CompanyMaster>(await companyList.ToListAsync(), pageIndex, pageSize);
        }

        #region Mapping 

        // Comapny Branch Mapping 
        public async Task CreateCompanyBrachMappingAsync(BranchMaster branchMaster, int comapnySrNo)
        {
            var branchMasterMapping = new CompanyMaster_BranchMasterMapping()
            {
                ComanyMasterId = comapnySrNo,
                BranchMasterId = branchMaster.ApplicationSrNo
            };
            await _context.CompanyMaster_BranchMasterMappings.AddAsync(branchMasterMapping);
            await _context.SaveChangesAsync();
        }

        // Company Department mapping 
        public async Task CreateComapnyDepartmentMapping(Department department, int companyId)
        {
            var companyDepartmentMapping = new CompanyMaster_DepartmentMapping()
            {
                DepartmentId = department.ApplicationSrNo,
                ComanyMasterId = companyId,
            };
            await _context.CompanyMaster_DepartmentMapping.AddAsync(companyDepartmentMapping);
            await _context.SaveChangesAsync();
        }

        #endregion

        #endregion
    }
}
