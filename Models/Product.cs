
namespace OnlineShoppingSystem.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public string Category { get; set; } = "";
        public decimal Price { get; set; }
        public decimal DiscountPercentage { get; set; }

        public decimal GetDiscountedPrice()
        {
            return Price - (Price * DiscountPercentage / 100);
        }
    }
}
