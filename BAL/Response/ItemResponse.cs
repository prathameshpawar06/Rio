namespace BAL.Response
{
    public class ItemResponse :ResponseModel
    {
        public ItemResponse()
        {
            Data = new ItemPaginationModel();
        }
        public ItemPaginationModel Data { get; set; }
    }

    public class ItemPaginationModel : PaginationModel 
    {
        public ItemPaginationModel()
        {
            ItemModel = new List<ItemResponseModel>();
        }
        public List<ItemResponseModel> ItemModel { get; set; }
    }

}
