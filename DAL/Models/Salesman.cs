using System.ComponentModel.DataAnnotations;

namespace DAL.Models
{
    public class Salesman
    {
        public string BranchCode { get; set; }
        public string BranchName { get; set; }
        [Key]
        public int EmpCode { get; set; }
        public string SalesMan { get; set; }
        public string SalesManAR { get; set; }
        public string COCODE { get; set; }
        public string SerialNO { get; set; }
        public string GPS { get; set; }
        public string MobileNo { get; set;}
        public string MobileNoOTP { get; set; }
        public bool Deleted { get; set; }
    }
}
