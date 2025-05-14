using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Repository;

namespace Shopping_Tutorial.Services.Cart
{
    public class CartService : ICartService
    {
        private readonly DataContext _dataContext;

        public CartService(DataContext context)
        {
            _dataContext = context;
        }
        public async Task MergeCartFromCookieToDb(HttpContext httpContext)
        {
            if (httpContext.User.Identity.IsAuthenticated)
            {
                string userId = httpContext.User.Identity.Name;
                var cookieCart = httpContext.Request.Cookies.GetJson<List<CartItemModel>>("Cart");

                if (cookieCart != null && cookieCart.Any())
                {
                    foreach (var item in cookieCart)
                    {
                        var existing = await _dataContext.CartItems.FirstOrDefaultAsync(c =>
                            c.ProductId == item.ProductId &&
                            c.ColorId == item.ColorId &&
                            c.UserId == userId);

                        if (existing != null)
                        {
                            existing.Quantity += item.Quantity;
                        }
                        else
                        {
                            var newItem = new CartItemModel
                            {
                                ProductId = item.ProductId,
                                ProductName = item.ProductName,
                                Quantity = item.Quantity,
                                Price = item.Price,
                                Image = item.Image,
                                ColorId = item.ColorId,
                                ColorName = item.ColorName,
                                UserId = userId
                            };
                            _dataContext.CartItems.Add(newItem);
                        }
                    }

                    await _dataContext.SaveChangesAsync();
                    httpContext.Response.Cookies.Delete("Cart");
                }
            }
        }
    }
}
