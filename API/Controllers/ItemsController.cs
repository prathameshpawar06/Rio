using AutoMapper;
using BAL.Response;
using BAL.Services;
using BAL.ViewModels;
using DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize(Roles = "Salesman,Admin")]
    public class ItemsController : Controller
    {
        #region Fields

        private readonly IMapper _mapper;
        private readonly ItemService _itemService;
        private readonly CategoryService _categoryService;
        private readonly DepartmentService _departmentService;
        private readonly LoggerService _loggerService;

        #endregion

        #region Ctor

        public ItemsController(IMapper mapper,
            ItemService itemService,
            CategoryService categoryService,
            DepartmentService departmentService,
            LoggerService loggerService)
        {
            _mapper = mapper;
            _itemService = itemService;
            _categoryService = categoryService;
            _departmentService = departmentService;
            _loggerService = loggerService;
        }

        #endregion  

        #region Method

        [HttpPost]
        [Route("items/create/")]
        public async Task<ItemResponse> CreateAsync([FromBody] ItemModel itemModel)
        {
            try
            {
                ItemResponse resp = new();

                var category = await _categoryService.GetCategoryBySrNoAsync(itemModel.Category);
                var department = await _departmentService.GetDepartmentByApplicationSrNoAsync(itemModel.Department);
                if (category == null && department == null && itemModel == null)
                {
                    resp.Error = 1;
                    resp.Message = category == null ? "Category not found Please enter a valid CategoryId." : department == null ? "Department not found Please enter a valid DepartmentId." : "Please enter valid data.";
                    return resp;
                }

                if (!String.IsNullOrEmpty(itemModel.Image))
                {
                    string savedFilename = _itemService.SaveImage(itemModel.Image);
                    itemModel.Image = savedFilename ?? string.Empty;
                }

                var items = _mapper.Map<Item>(itemModel);
                items.ItemCode = itemModel.ItemCode;
                items.COCODE = Guid.NewGuid().ToString();
                items.SerialNO = Guid.NewGuid().ToString();
                await _itemService.CreateItemAsync(items);

                var itemCategoryMapping = new Item_CategoryMapping()
                {
                    CategoryId = items.Category,
                    ItemId = items.ApplicationSrNo
                };
                await _itemService.CreateCategoryItemMappingAsync(itemCategoryMapping);

                resp.Data.ItemModel.Add(_mapper.Map<ItemResponseModel>(items));
                resp.Error = 0;
                resp.Message = "Item created successfully.";
                return resp;

            }
            catch (Exception ex)
            {
                await _loggerService.InsertLogAsync(ex.Message);
                return new ItemResponse
                {
                    Error = 1,
                    Message = ex.Message
                };
            }
        }

        [HttpPost]
        [Route("items/edit/{itemId}")]
        public async Task<ItemResponse> EditAsync([FromBody] ItemModel itemModel, int itemId)
        {
            try
            {
                ItemResponse resp = new();
                var itemBySrNo = await _itemService.GetItemsByApplicationSrNoAsync(itemId);
                if (itemBySrNo == null)
                {
                    resp.Message = "Item not found";
                    resp.Error = 1;
                    return resp;
                }

                if (!String.IsNullOrEmpty(itemModel?.Image))
                {
                    string savedFilename = _itemService.SaveImage(itemModel.Image);
                    itemModel.Image = savedFilename ?? string.Empty;
                }

                _mapper.Map(itemModel, itemBySrNo);
                await _itemService.UpdateItemAsync(itemBySrNo);
                var itemMapping = await _itemService.GetCategoryItemMappingByItemAsync(itemBySrNo.ApplicationSrNo);
                if (itemMapping != null)
                {
                    itemMapping.CategoryId = itemBySrNo.Category;
                    await _itemService.UpdateCategoryItemMappingAsync(itemMapping);
                }

                resp.Error = 0;
                resp.Data.ItemModel.Add(_mapper.Map<ItemResponseModel>(itemBySrNo));
                resp.Message = "Item records updated successfully.";
                return resp;
            }
            catch (Exception ex)
            {
                await _loggerService.InsertLogAsync(ex.Message);
                return new ItemResponse
                {
                    Error = 1,
                    Message = ex.Message
                };
            }
        }

        [HttpPost]
        [Route("items/delete/{itemId}")]
        public async Task<ResponseModel> DeleteAsync(int itemId)
        {
            try
            {
                ResponseModel resp = new();
                var itemBySrno = await _itemService.GetItemsByApplicationSrNoAsync(itemId);
                if (itemBySrno == null)
                {
                    resp.Message = "Items not found";
                    resp.Error = 1;
                    return resp;
                }

                await _itemService.DeleteItemAsync(itemBySrno);

                resp.Error = 0;
                resp.Message = "Item deleted successfully.";
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
        [Route("item/searchItem/")]
        public async Task<ItemResponse> SearchItemAsync(string itemName, string itemCode, int itemId, int categoryId, int companyId, int pageNo = 1, int pageSize = 10)
        {
            try
            {
                ItemResponse resp = new();
                var itemsList = await _itemService.GetAllItemsAsync(itemName, itemCode, itemId, categoryId, companyId, pageIndex: pageNo - 1, pageSize: pageSize);
                foreach (var item in itemsList)
                {
                    var newItems = _mapper.Map<ItemResponseModel>(item);
                    resp.Data.ItemModel.Add(newItems);
                }

                resp.Data.PageIndex = pageNo;
                resp.Data.TotalCount = itemsList.TotalCount;
                resp.Data.HasNextPage = itemsList.HasNextPage;
                resp.Message = resp.Data.ItemModel.Count == 0 ? "Record not found" : "These are all the records of items.";
                resp.Error = resp.Data.ItemModel.Count == 0 ? 1 : 0;
                return resp;
            }
            catch (Exception ex)
            {
                await _loggerService.InsertLogAsync(ex.Message);
                return new ItemResponse
                {
                    Error = 1,
                    Message = ex.Message
                };
            }
        }

        #endregion
    }
}