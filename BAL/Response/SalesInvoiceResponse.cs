
namespace BAL.Response
{
    public class SalesInvoiceResponse : ResponseModel
    {
        public SalesInvoiceResponse()
        {
            Data = new List<SalesInvoiceResponseModel>();
        }
        public List<SalesInvoiceResponseModel> Data { get; set; }
    }
    
}
