using System.ComponentModel.DataAnnotations;

namespace DAL.Models
{
    public class Department
    {
        public string DepartmentName { get; set; }
        public string DepartmentNameAR { get; set; }
        [Key]
        public int ApplicationSrNo { get; set; }
        public string COCODE { get; set; }
        public string SerialNO { get; set; }
    }
}
