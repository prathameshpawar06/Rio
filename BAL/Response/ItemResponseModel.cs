namespace BAL.Response
{
    public class ItemResponseModel
    {
        public int Department { get; set; }
        public int Category { get; set; }
        public string ItemCode { get; set; }
        public string Name { get; set; }
        public string NameAR { get; set; }
        public string SalesPrice { get; set; }
        public bool IncludingVat { get; set; }
        public decimal PercentageVat { get; set; }
        public string TotalPrice { get; set; }
        public string Image { get; set; }
        public int ApplicationSrNo { get; set; }
        public string COCODE { get; set; }
        public string SerialNO { get; set; }
        public int Quantity { get; set; }
        public decimal PriceTotal { get; set; }
    }
}
