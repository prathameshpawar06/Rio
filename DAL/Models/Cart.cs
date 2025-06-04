namespace DAL.Models
{
    public class Cart
    {
        public int Id { get; set; }

        public int SalesManId { get; set; }

        public int Quantity { get; set; }
        
        public DateTime CreatedOnUtc { get; set; }
    }
}
