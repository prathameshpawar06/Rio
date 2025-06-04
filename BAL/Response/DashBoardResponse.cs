namespace BAL.Response
{
    public class DashBoardResponse : ResponseModel
    {
        public DashBoardResponse()
        {
            Data = new SalesReportPaginationModel();
        }
        public SalesReportPaginationModel Data { get; set; }
    }

    public class SalesReportPaginationModel : PaginationModel
    {
        public SalesReportPaginationModel()
        {
            SalesReportModels = new List<SalesReportModel>();
        }
        public int SaleQty { get; set; }
        public decimal TotalSaleAmount { get; set; }
        public IList<SalesReportModel> SalesReportModels { get; set; }
    }

}
