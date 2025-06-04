using DAL.Context;
using DAL.Helper;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace BAL.Services
{
    public class CategoryService
    {
        #region Fields

        private readonly RioContext _context;

        #endregion

        #region Ctor

        public CategoryService(RioContext context)
        {
            _context = context;
        }

        #endregion

        #region Methods 

        public async Task CreateCategoryAsync(Category category)
        {
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
        }

        public async Task<Category> GetCategoryBySrNoAsync(int applicationSrNo)
        {
            var categoryBySrno = await _context.Categories.FirstOrDefaultAsync(x => x.ApplicationSrNo == applicationSrNo && !x.Deleted);
            return categoryBySrno;
        }

        public async Task UpdateCategoryAsync(Category category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCategoryAsync(Category category)
        {
            category.Deleted = true;
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
        }

        public async Task<IPagedList<Category>> GetAllCategoryAsync(string categoryName = null, int companyId = 0, int branchId = 0, int categoryId = 0, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var categoryList = _context.Categories.Where(x => !x.Deleted);

            var category = from Category in categoryList
                           join bc in _context.BranchMaster_CategoryMappings
                           on Category.ApplicationSrNo equals bc.CategoryId
                           join cm in _context.CompanyMaster_BranchMasterMappings
                           on bc.branchMasterId equals cm.BranchMasterId
                           select new
                           {
                               category = Category,
                               compnayBrachMapping = cm
                           };

            if (companyId > 0)
                category = category.Where(x => x.compnayBrachMapping.ComanyMasterId == companyId);

            if (branchId > 0)
                category = category.Where(x => x.compnayBrachMapping.BranchMasterId == branchId);

            if (categoryId > 0)
                category = category.Where(x => x.category.ApplicationSrNo == categoryId);

            var result = category.Select(x => x.category);

            //By Category Name
            if (!string.IsNullOrWhiteSpace(categoryName))
                result = result.Where(x => x.CategoryName.ToLower().Contains(categoryName.Trim().ToLower()));

            return new PagedList<Category>(await result.ToListAsync(), pageIndex, pageSize);
        }

        #endregion
    }
}
