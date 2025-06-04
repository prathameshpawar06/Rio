using System;
using System.ComponentModel.DataAnnotations;

namespace DAL.Models
{
    public class Item
    {
        public int Department { get; set; }
        public int Category { get; set; }
        public string ItemCode { get; set; }
        public string Name { get; set; }
        public string NameAR { get; set; }
        public string SalesPrice { get; set; }
        public decimal PercentageVat { get; set; }
        public bool IncludingVat { get; set; }
        public string Image { get; set; }
        [Key]
        public int ApplicationSrNo { get; set; }
        public string COCODE { get; set; }
        public string SerialNO { get; set; }
        public bool Deleted { get; set; }
    }
}
