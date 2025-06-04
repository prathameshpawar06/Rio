using DAL.Context;
using DAL.Helper;
using DAL.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BAL.Services
{
    public class ItemService
    {

        #region Fields

        private readonly RioContext _context;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        #endregion

        #region Ctor

        public ItemService(RioContext context,
            IHostingEnvironment hostingEnvironment,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }

        #endregion

        #region Utility

        private bool AreImagesEqual(byte[] image1, byte[] image2)
        {
            // Check if the byte arrays have the same length
            if (image1.Length != image2.Length)
            {
                return false;
            }

            // Compare pixel data
            return image1.SequenceEqual(image2);
        }

        #endregion

        #region Methods

        public string SaveImage(string base64Data, string existingImagePath = null)
        {
            try
            {
                // Decode base64 data
                byte[] imageBytes = Convert.FromBase64String(base64Data);

                // Check if the byte array is empty
                if (imageBytes.Length == 0)
                {
                    Console.WriteLine("Error: Image upload failed. Please reupload the image.");
                    return null; // or return an appropriate message or indication
                }

                // Check if the newly saved image is the same as the existing image
                if (!string.IsNullOrEmpty(existingImagePath))
                {
                    using HttpClient client = new();
                    try
                    {
                        string domainUrl = $"{_httpContextAccessor.HttpContext?.Request.Scheme}://{_httpContextAccessor.HttpContext?.Request.Host}";
                        string completeUrl = $"{domainUrl}/{existingImagePath}";

                        byte[] existingImageBytes = client.GetByteArrayAsync(completeUrl).Result;

                        if (AreImagesEqual(imageBytes, existingImageBytes))
                        {
                            return existingImagePath;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Handle exceptions (e.g., web request failed)
                        Console.WriteLine($"Error downloading image: {ex.Message}");
                    }
                }

                // Get the path to the wwwroot folder within the MVC project
                string wwwrootPath = _hostingEnvironment.WebRootPath;
                string itemImagesPath = Path.Combine(wwwrootPath, "ItemImages");

                // Check if directory exists or create it
                if (!Directory.Exists(itemImagesPath))
                {
                    Directory.CreateDirectory(itemImagesPath);
                }

                // Generate a unique filename
                string imageName = $"image_{DateTime.Now.Ticks}.png";

                // Set the image path
                string imgPath = Path.Combine(itemImagesPath, imageName);

                // Save image to file
                File.WriteAllBytes(imgPath, imageBytes);

                imgPath = Path.Combine("ItemImages", imageName);

                // Replace backslashes with forward slashes in the file path
                imgPath = imgPath.Replace('\\', '/');

                // Return the full file path with forward slashes after successfully saving the image
                return imgPath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                // Return null or another indication of failure if needed
                return null;
            }
        }

        public async Task CreateItemAsync(Item item)
        {
            await _context.Items.AddAsync(item);
            await _context.SaveChangesAsync();
        }

        public async Task<Item> GetItemsByApplicationSrNoAsync(int applicationSrNo)
        {
            var itemSrNo = await _context.Items.FirstOrDefaultAsync(x => x.ApplicationSrNo == applicationSrNo && !x.Deleted);
            return itemSrNo;
        }

        public async Task UpdateItemAsync(Item item)
        {
            _context.Items.Update(item);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteItemAsync(Item item)
        {
            item.Deleted = true;
            _context.Items.Update(item);
            await _context.SaveChangesAsync();
        }

        public async Task<PagedList<Item>> GetAllItemsAsync(string itemName, string itemCode, int itemId = 0, int categoryId = 0, int companyId = 0, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var itemList = _context.Items.Where(x => !x.Deleted);

            var items = from item in itemList
                        join it in _context.Item_CategoryMappings
                        on item.ApplicationSrNo equals it.ItemId
                        join bc in _context.BranchMaster_CategoryMappings
                        on it.CategoryId equals bc.CategoryId
                        join cm in _context.CompanyMaster_BranchMasterMappings
                        on bc.branchMasterId equals cm.BranchMasterId
                        select new
                        {
                            Item = item,
                            CategoryId = bc,
                            CompanyId = cm,
                        };

            if (!string.IsNullOrWhiteSpace(itemCode))
                items = items.Where(c => c.Item.ItemCode.ToLower().Contains(itemCode.Trim().ToLower()));

            if (itemId > 0)
                items = items.Where(x => x.Item.ApplicationSrNo == itemId);

            if (categoryId > 0)
                items = items.Where(x => x.CategoryId.CategoryId == categoryId);

            if (companyId > 0)
                items = items.Where(x => x.CompanyId.ComanyMasterId == companyId);

            var result = items.Select(x => x.Item).Distinct();

            if (!string.IsNullOrWhiteSpace(itemName))
                result = result.Where(c => c.Name.ToLower().Contains(itemName.Trim().ToLower()));

            return new PagedList<Item>(await result.ToListAsync(), pageIndex, pageSize);
        }

        #region mapping 

        public async Task CreateCategoryItemMappingAsync(Item_CategoryMapping itemCategoryMapping)
        {
            await _context.Item_CategoryMappings.AddAsync(itemCategoryMapping);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCategoryItemMappingAsync(Item_CategoryMapping itemCategoryMapping)
        {
            _context.Item_CategoryMappings.Update(itemCategoryMapping);
            await _context.SaveChangesAsync();
        }

        public async Task<Item_CategoryMapping> GetCategoryItemMappingByItemAsync(int itemId)
        {
            return await _context.Item_CategoryMappings.FirstOrDefaultAsync(x => x.ItemId == itemId);
        }

        public async Task<IList<Item>> GetOrderItemsByIdAsync(int orderId)
        {
            var orderItemIds = await _context.OrderItems
                .Where(x => x.OrderId == orderId)
                .Select(x => x.ItemId)
                .ToListAsync();

            var items = await _context.Items
                .Where(x => orderItemIds.Contains(x.ApplicationSrNo))
                .ToListAsync();

            return items;
        }

        #endregion
    }
    #endregion
}