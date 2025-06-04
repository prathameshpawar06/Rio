namespace DAL.Models
{
    public class BranchMaster_CategoryMapping
    {
        public int Id { get; set; }

        public int branchMasterId { get; set; }

        public int CategoryId { get; set; }
    }
}
