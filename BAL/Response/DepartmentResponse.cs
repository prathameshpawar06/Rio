
namespace BAL.Response
{
    public class DepartmentResponse : ResponseModel
    {
        public DepartmentResponse()
        {
            Data = new DepartmentPaginetion();
        }
        public DepartmentPaginetion Data { get; set; }
    }

    public class DepartmentPaginetion : PaginationModel
    {
        public DepartmentPaginetion()
        {
            DepartmentModels = new List<DepartmentResponseModel>();
        }
        public List<DepartmentResponseModel> DepartmentModels { get; set; }
    }

}
