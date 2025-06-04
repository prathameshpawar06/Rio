using System.ComponentModel.DataAnnotations;

namespace DAL.Models
{
    public class SalesInvoice
    {
        public int ItemCode { get; set; }
        public string Name { get; set;}
        public string NameAR { get; set;}
        public int SalesPrice { get; set; }
        public string Image { get; set;}
        public int Qty { get; set; }
        public string Amount { get; set; }
        public string TAXAmount { get; set; }
        [Key]
        public int ApplicationSrNo { get; set; }
        public string COCODE { get; set; }
        public string SerialNO { get; set; }
    }
}
