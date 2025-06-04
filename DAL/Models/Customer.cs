using System.ComponentModel.DataAnnotations;

namespace DAL.Models
{
    public class Customer
    {
        public string EstCoName { get; set; }
        public string EstCoNameAR { get; set; }
        public string CRNO { get; set; }
        public string VATNO { get; set; }
        public int BuldingNo { get; set; }
        public string Street { get; set; }
        public string District { get; set; }
        public string City { get; set; }
        public int AdditionalNo { get; set; }
        public string Country { get; set; }
        public string ContactPerson { get; set; }
        public string EmailID { get; set; }
        public string MobileNo { get; set; }
        public bool Status { get; set; }
        [Key]
        public int ApplicationSrNo { get; set; }
        public string COCODE { get; set; }
        public string SerialNO { get; set; }
        public bool Deleted { get; set; }
    }
}
