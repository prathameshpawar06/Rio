namespace BAL.Response
{
    public class OrderResponseModel
    {
        public OrderResponseModel()
        {
            OrderItems = new List<ItemResponseModel>();
            Company = new CompanyMasterResponseModel();
            Branch = new BranchMasterResponseModel();
            SalesMan = new SalesmanResponseModel();
            Customer = new CustomerOrderResponseModel();
        }
        public int Id { get; set; }
        public CompanyMasterResponseModel Company { get; set; }
        public BranchMasterResponseModel Branch { get; set; }
        public SalesmanResponseModel SalesMan { get; set; }
        public CustomerOrderResponseModel Customer { get; set; }
        public string PaymentMethod { get; set; }
        public int PaymentStatusId { get; set; }
        public DateTime OrderDateTime { get; set; }
        public decimal OrderTotal { get; set; }
        public decimal VatAmount { get; set; }
        public IList<ItemResponseModel> OrderItems { get; set; }
    }
}
