using System.ComponentModel.DataAnnotations;

namespace DAL.Models
{
    public class BranchMaster
    {
        public string BranchCode { get; set; }
        public string BranchName { get; set; }
        public string BranchNameAR { get; set; }
        public string CRNO { get; set; }
        public string VATNO { get; set; }
        public int BuldingNo { get; set; }
        public string Street { get; set; }
        public string Country { get; set; }
        public string District { get; set; }
        public string City { get; set; }
        public string AdditionalNo { get; set; }
        public bool Status { get; set; }

        [Key]
        public int ApplicationSrNo { get; set; }
        public string COCODE { get; set; }
        public string SerialNO { get; set; }
        public string GPS { get; set; }
        public string MobileNo { get; set; }
        public string MobileNoOTP { get; set; }
        public bool Deleted { get; set; }
    }
}
