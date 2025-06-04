
namespace BAL.Response
{
    public class CompanyMasterResponse : ResponseModel
    {
        public CompanyMasterResponse()
        {
            Data = new CompanyMasterPaginetion();
        }
        public CompanyMasterPaginetion Data { get; set; }
    }

    public class CompanyMasterPaginetion : PaginationModel
    {
        public CompanyMasterPaginetion()
        {
            CompanyModels = new List<CompanyMasterResponseModel>();
        }
        public List<CompanyMasterResponseModel> CompanyModels { get; set; }
    }

}
