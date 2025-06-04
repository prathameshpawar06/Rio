
namespace BAL.Response
{
    public class BranchMastersResponse : ResponseModel
    {
        public BranchMastersResponse()
        {
            Data = new BranchMasterPaginetion();
        }
        public BranchMasterPaginetion Data { get; set; }
    }

    public class BranchMasterPaginetion : PaginationModel
    {
        public BranchMasterPaginetion()
        {
            BranchModels = new List<BranchMasterResponseModel>();
        }
        public List<BranchMasterResponseModel> BranchModels { get; set; }
    }
}
