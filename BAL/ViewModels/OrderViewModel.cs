namespace BAL.ViewModels
{
    public class OrderViewModel
    {
        public OrderViewModel()
        {
            OrderItems = new List<OrderItemsViewModel>();
        }
        public int SalesManId { get; set; }
        public int CustomerId { get; set; }
        public string PaymentMethod { get; set; }
        public IList<OrderItemsViewModel> OrderItems { get; set; }
    }

    public class OrderItemsViewModel
    {
        public int ItemId { get; set; }
        public int CategoryId { get; set; }
        public int Quantity { get; set; }
    }
}
