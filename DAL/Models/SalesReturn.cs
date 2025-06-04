using System.ComponentModel.DataAnnotations;

namespace DAL.Models
{
    public class SalesReturn
    {
        public int ItemCode { get; set; }
        public string Name { get; set; }
        public string NameAR { get; set; }
        public string SalesPrice { get; set; }
        public string Image { get; set; }
        public int Qty { get; set; }
        public int Amount { get; set; }
        public int TAXAmount { get; set; }
        [Key]
        public int ApplicationSrNo { get; set; }
        public string COCODE { get; set; }
        public string SerialNO { get; set; }
    }
}