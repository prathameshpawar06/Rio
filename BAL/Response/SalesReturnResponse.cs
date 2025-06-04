
namespace BAL.Response
{
    public class SalesReturnResponse : ResponseModel
    {
        public SalesReturnResponse()
        {
            Data = new List<SalesReturnResponseModel>(); 
        }
        public List<SalesReturnResponseModel> Data { get; set; }
    }
   
}
