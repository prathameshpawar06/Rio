using System.ComponentModel.DataAnnotations;

namespace DAL.Models
{
    public class Category
    {
        public string CategoryName { get; set; }
        public string CategoryNameAR { get; set; }
        [Key]
        public int ApplicationSrNo { get; set; }
        public string COCODE { get; set; }
        public string SerialNO { get; set; }
        public bool Deleted { get; set; }
    }
}
