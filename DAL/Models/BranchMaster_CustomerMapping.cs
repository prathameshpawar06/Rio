namespace DAL.Models
{
    public class BranchMaster_CustomerMapping
    {
        public int Id { get; set; }

        public int branchMasterId { get; set; }

        public int CustomerId { get; set; }
    }
}
