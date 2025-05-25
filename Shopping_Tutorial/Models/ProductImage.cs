namespace Shopping_Tutorial.Models
{
    public class ProductImage
    {
        public int Id { get; set; }
        public string ImagePath { get; set; }
        public long ProductId { get; set; }

        public ProductModel Product { get; set; }
    }
}
