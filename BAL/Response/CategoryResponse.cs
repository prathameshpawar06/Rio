
namespace BAL.Response
{
    public class CategoryResponse : ResponseModel
    {
        public CategoryResponse()
        {
            Data = new CategoryPagesResponse();
        }
        public CategoryPagesResponse Data { get; set; }
       
    }

    public class CategoryPagesResponse : PaginationModel
    {
        public CategoryPagesResponse()
        {
            CategoryResponseModels = new List<CategoryResponseModel>();
        }
        public List<CategoryResponseModel> CategoryResponseModels { get; set; }
    }
}
