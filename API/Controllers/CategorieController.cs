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
    public class CategorieController : Controller
    {
        #region Fileds

        private readonly IMapper _mapper;
        private readonly CategoryService _categoryService;
        private readonly BranchMasterService _branchMasterService;
        private readonly LoggerService _loggerService;

        #endregion

        #region Ctor

        public CategorieController(IMapper mapper,
            CategoryService categoryService,
            BranchMasterService branchMasterService,
            LoggerService loggerService)
        {
            _mapper = mapper;
            _categoryService = categoryService;
            _branchMasterService = branchMasterService;
            _loggerService = loggerService;
        }

        #endregion

        #region Methods 

        [HttpPost]
        [Route("category/create/{branchId}")]
        public async Task<CategoryResponse> CreateAsync([FromBody] CategoryModel categoryModel, int branchId)
        {
            try
            {
                CategoryResponse resp = new();
                var branch = await _branchMasterService.GetBranchMasterBySrNoAsync(branchId);
                if (branch == null)
                {
                    resp.Error = 1;
                    resp.Message = "Branch not found. Please enter a valid BranchId.";
                    return resp;
                }

                var category = _mapper.Map<Category>(categoryModel);
                category.COCODE = Guid.NewGuid().ToString();
                category.SerialNO = Guid.NewGuid().ToString();
                await _categoryService.CreateCategoryAsync(category);

                //insert the new branch category mapping
                await _branchMasterService.CreateBrachCategoryMappingAsync(new BranchMaster_CategoryMapping
                {
                    CategoryId = category.ApplicationSrNo,
                    branchMasterId = branchId
                });

                resp.Error = 0;
                resp.Data.CategoryResponseModels.Add(_mapper.Map<CategoryResponseModel>(category));
                resp.Message = "Category created successfully.";
                return resp;
            }
            catch (Exception ex)
            {
                await _loggerService.InsertLogAsync(ex.Message);
                return new CategoryResponse
                {
                    Error = 1,
                    Message = ex.Message
                };
            }
        }

        [HttpPost]
        [Route("category/edit/{categoryId}")]
        public async Task<CategoryResponse> EditAsync([FromBody] CategoryModel categoryModel, int categoryId)
        {
            try
            {
                CategoryResponse resp = new();
                var categoryBySrno = await _categoryService.GetCategoryBySrNoAsync(categoryId);
                if (categoryBySrno == null)
                {
                    resp.Message = "Category not found. Please enter the correct CategoryId.";
                    resp.Error = 1;
                    return resp;
                }

                categoryBySrno.CategoryName = categoryModel.CategoryName;
                categoryBySrno.CategoryNameAR = categoryModel.CategoryNameAR;
                await _categoryService.UpdateCategoryAsync(categoryBySrno);

                resp.Error = 0;
                resp.Data.CategoryResponseModels.Add(_mapper.Map<CategoryResponseModel>(categoryBySrno));
                resp.Message = "Record is successfully updated.";
                return resp;
            }
            catch (Exception ex)
            {
                await _loggerService.InsertLogAsync(ex.Message);
                return new CategoryResponse
                {
                    Error = 1,
                    Message = ex.Message
                };
            }
        }

        [HttpPost]
        [Route("category/delete/{categoryId}")]
        public async Task<ResponseModel> DeleteAsync(int categoryId)
        {
            try
            {
                ResponseModel resp = new();
                var categoryBySrno = await _categoryService.GetCategoryBySrNoAsync(categoryId);
                if (categoryBySrno == null)
                {
                    resp.Message = "Category not found";
                    resp.Error = 1;
                    return resp;
                }

                await _categoryService.DeleteCategoryAsync(categoryBySrno);

                resp.Error = 0;
                resp.Message = "Category has been deleted successfully.";
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

        [HttpGet]
        [Route("category/searchCategory/")]
        public async Task<CategoryResponse> SearchCategoryAsync(string categoryName, int pageNo = 1, int pageSize = 10, int companyId = 0, int branchId = 0, int categoryId = 0)
        {
            try
            {
                CategoryResponse resp = new();
                var customerList = await _categoryService.GetAllCategoryAsync(categoryName, companyId, branchId, categoryId, pageIndex: pageNo - 1, pageSize: pageSize);
                foreach (var item in customerList)
                {
                    var newCategory = _mapper.Map<CategoryResponseModel>(item);
                    resp.Data.CategoryResponseModels.Add(newCategory);
                }
                if (resp.Data.CategoryResponseModels.Count == 0)
                {
                    resp.Message = "Category record not found.";
                    resp.Error = 1;
                    return resp;
                }

                resp.Data.PageIndex = pageNo;
                resp.Data.TotalCount = customerList.TotalCount;
                resp.Data.HasNextPage = customerList.HasNextPage;
                resp.Message = "All category records successfully retrieved by CompanyID and BranchId.";
                return resp;
            }
            catch (Exception ex)
            {
                await _loggerService.InsertLogAsync(ex.Message);
                return new CategoryResponse
                {
                    Error = 1,
                    Message = ex.Message
                };
            }
        }

        #endregion
    }
}
