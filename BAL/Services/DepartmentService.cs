using DAL.Context;
using DAL.Helper;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace BAL.Services
{
    public class DepartmentService
    {
        #region Fields

        private readonly RioContext _context;

        #endregion

        #region Ctor

        public DepartmentService(RioContext context)
        {
            _context = context;
        }

        #endregion

        #region Methods

        public async Task CreateDepartmentAsync(Department department)
        {
            await _context.Departments.AddAsync(department);
            await _context.SaveChangesAsync();
        }

        public async Task<Department> GetDepartmentByApplicationSrNoAsync(int applicationSrNo)
        {
            var departmentSrNo = await _context.Departments.FirstOrDefaultAsync(x => x.ApplicationSrNo == applicationSrNo);

            return departmentSrNo;
        }

        public async Task UpdateDepartmentAsync(Department department)
        {
            _context.Departments.Update(department);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteDepartmentAsync(Department department)
        {
            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();
        }

        public async Task<IPagedList<Department>> GetAllDepartmentsAsync(int departmentId, int companyId, string departmentName, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var departmentList = _context.Departments.AsQueryable();

            if (companyId > 0)
                departmentList = (from dep in departmentList
                                  join cd in _context.CompanyMaster_DepartmentMapping
                                  on dep.ApplicationSrNo equals cd.DepartmentId
                                  where cd.ComanyMasterId == companyId
                                  select dep);

            if (departmentId > 0)
                departmentList = departmentList.Where(x => x.ApplicationSrNo == departmentId);

            if (!string.IsNullOrWhiteSpace(departmentName))
                departmentList = departmentList.Where(x => x.DepartmentName.Trim().ToLower().Contains(departmentName.Trim().ToLower()));

            return new PagedList<Department>(await departmentList.ToListAsync(), pageIndex, pageSize);
        }

        #endregion
    }
}
