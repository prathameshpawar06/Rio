namespace BAL.Response
{
    public class OrderResponse : ResponseModel
    {
        public OrderResponse()
        {
            Data = new OrderPaginationResponse();
        }
        public OrderPaginationResponse Data { get; set; }
    }

    public class OrderPaginationResponse : PaginationModel
    {
        public OrderPaginationResponse()
        {
            OrderResponseModels = new List<OrderResponseModel>();
        }
        public IList<OrderResponseModel> OrderResponseModels { get; set; }

    }

}
