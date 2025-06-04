
namespace BAL.Response
{
    public class SalesmansResponse : ResponseModel
    {
        public SalesmansResponse()
        {
            Data = new SalesManPaginationModel();
        }
        public SalesManPaginationModel Data { get; set; }
    }

    public class SalesManPaginationModel : PaginationModel
    {
        public SalesManPaginationModel()
        {
            SalesmanModel = new List<SalesmanResponseModel>();
        }
        public List<SalesmanResponseModel> SalesmanModel { get; set; }
    }

}
