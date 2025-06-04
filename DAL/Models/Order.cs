namespace DAL.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int SalesManId { get; set; }
        public int CustomerId { get; set; }
        public string PaymentMethod { get; set; }
        public int PaymentStatusId { get; set; }  
        public DateTime OrderDateTime { get; set; }
        public decimal OrderTotal { get; set; }
        public decimal VatAmount { get; set; }
    }
}
