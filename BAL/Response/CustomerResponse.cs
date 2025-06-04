
namespace BAL.Response
{
    public class CustomerResponse : ResponseModel
    {
        public CustomerResponse()
        {
            Data = new CustomerPaginationResponse();
        }
        public CustomerPaginationResponse Data { get; set; }

    }

    public class CustomerPaginationResponse : PaginationModel
    {
        public CustomerPaginationResponse()
        {
            CustomerModel = new List<CustomerResponseModel>();
        }
        public List<CustomerResponseModel> CustomerModel { get; set; }
    }

}
