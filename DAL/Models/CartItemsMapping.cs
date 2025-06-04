namespace DAL.Models
{
    public class CartItemsMapping
    {
        public int Id { get; set; }
        public int CartId { get; set; }
        public int ItemsId { get; set; }
        public int Quantity { get; set; }
    }
}
