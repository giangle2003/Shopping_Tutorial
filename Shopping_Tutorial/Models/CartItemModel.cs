using System.ComponentModel.DataAnnotations;

namespace Shopping_Tutorial.Models
{
    public class CartItemModel
    {
        [Key]
        public long Id { get; set; } // Khóa chính của bảng CartItem
        public long ProductId { get; set; }
        public string ProductName { get; set; }

        public int Quantity { get; set; }

        public decimal Price { get; set; }

        public decimal Total
        {
            get
            {
                return Quantity * Price;
            }
        }

        public string Image { get; set; }

        public int ColorId { get; set; } // Mã màu sắc của sản phẩm
        public string ColorName { get; set; } // Tên màu sắc của sản phẩm
        public string UserId { get; set; } // Mã người dùng (người mua hàng)
        public CartItemModel()
        {
        }
        public CartItemModel(ProductModel product, int colorId, string colorName, int quantity)
        {
            ProductId = product.Id;
            ProductName = product.Name;
            //Quantity = 1;
            Quantity = quantity;
            Price = product.Price;
            Image = product.Image;
            ColorId = colorId;
            ColorName = colorName;
            
        }
    }
}
