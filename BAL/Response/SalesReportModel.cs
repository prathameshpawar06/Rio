namespace BAL.Response
{
    public class SalesReportModel
    {
        public int SalesManId { get; set; }
        public string SalesManName { get; set; }
        public int TotalNotrans { get; set; }
        public decimal MadaAmount { get; set; }
        public decimal CashAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal VATAmount { get; set; }
        public int Noitems { get; set; }
        public int ItemsQuantity { get; set; }
    }
}
