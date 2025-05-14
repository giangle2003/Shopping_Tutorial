using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Shopping_Tutorial.Migrations;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Models.ViewModels;
using Shopping_Tutorial.Repository;
using System.Drawing;

namespace Shopping_Tutorial.Controllers
{
    public class CartController : Controller
    {
        private readonly DataContext _dataContext;

        public CartController(DataContext context)
        {
            _dataContext = context;
        }

        public async Task<IActionResult> Cart()
        {
            List<CartItemModel> cartItems;

            if (User.Identity.IsAuthenticated)
            {
                // 🗄️ Lấy giỏ hàng từ DB nếu đã đăng nhập
                string userId = User.Identity.Name;

                var dbCartItems = await _dataContext.CartItems
                    .Where(c => c.UserId == userId)
                    .ToListAsync();

                cartItems = dbCartItems.Select(c => new CartItemModel
                {
                    ProductId = c.ProductId,
                    ProductName = c.ProductName,
                    Quantity = c.Quantity,
                    Price = c.Price,
                    Image = c.Image,
                    ColorId = c.ColorId,
                    ColorName = c.ColorName
                }).ToList();
            }
            else
            {
                // Nếu chưa đăng nhập, lấy từ cookie
                cartItems = Request.Cookies.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
            }

            //Gợi ý sản phẩm từ danh mục phụ kiên laptop
            var sussgestProduct = await _dataContext.Products
                .Where(p => p.CategoryId == 17 && p.Quantity > 0) // Chỉ lấy sản phẩm có số lượng lớn hơn 0
                .OrderBy(r => Guid.NewGuid()) //Theo thứ tự đã bán
                .Take(4)
                .ToListAsync();




            //  ViewModel
            CartItemViewModel cartVM = new()
            {
                CartItems = cartItems,
                GrandTotal = cartItems.Sum(c => c.Price * c.Quantity),
                SuggestedProducts = sussgestProduct
            };

            return View(cartVM);
        }

        public async Task<IActionResult> Index(ShippingModel shippingModel)
        {
            List<CartItemModel> cartItems;

            if (User.Identity.IsAuthenticated)
            {
                // 🗄️ Lấy giỏ hàng từ DB nếu đã đăng nhập
                string userId = User.Identity.Name;

                var dbCartItems = await _dataContext.CartItems
                    .Where(c => c.UserId == userId)
                    .ToListAsync();

                cartItems = dbCartItems.Select(c => new CartItemModel
                {
                    ProductId = c.ProductId,
                    ProductName = c.ProductName,
                    Quantity = c.Quantity,
                    Price = c.Price,
                    Image = c.Image,
                    ColorId = c.ColorId,
                    ColorName = c.ColorName
                }).ToList();
            }
            else
            {
                // Nếu chưa đăng nhập, lấy từ cookie
                cartItems = Request.Cookies.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
            }

            // Nhận phí vận chuyển từ cookie
            var shippingPriceCookie = Request.Cookies["ShippingPrice"];
            decimal shippingPrice = 0;
            if (shippingPriceCookie != null)
            {
                shippingPrice = JsonConvert.DeserializeObject<decimal>(shippingPriceCookie);
            }

            // Xử lý mã giảm giá

            var coupon_code = Request.Cookies["CouponTitle"];
            //Lấy danh mục các sản phẩm trong giỏ
            var productIds = cartItems.Select(c => c.ProductId).Distinct().ToList();
            var categoryIdsInCart = await _dataContext.Products
                .Where(p => productIds.Contains(p.Id))
                .Select(p => p.CategoryId)
                .Distinct()
                .ToListAsync();

            var now = DateTime.Now;
            var availableCoupons = await _dataContext.Coupons
                .Where(c => c.DateExpired >= now && c.Quantity > 0 && c.Status == 1)
                .Where(c=> _dataContext.CouponCategories
                    .Any(cc => cc.couponId == c.Id && categoryIdsInCart.Contains(cc.categoryId)))
                .ToListAsync();

            decimal? discountPercent = null;
            decimal? discountAmount = null;
            decimal? maxDiscount = null;

            if (!string.IsNullOrEmpty(coupon_code))
            {
                var coupon = availableCoupons
                    .FirstOrDefault(c => (c.Name + " | " + c.Description) == coupon_code);
                if (coupon != null)
                {
                    discountPercent = coupon.DiscountPercent;
                    discountAmount = coupon.DiscountPrice;
                    maxDiscount = coupon.MaxDiscountAmount;
                }
            }

            //  ViewModel
            CartItemViewModel cartVM = new()
            {
                CartItems = cartItems,
                GrandTotal = cartItems.Sum(c => c.Price * c.Quantity),
                ShippingCost = shippingPrice,
                CouponCode = coupon_code,
                AvailableCoupons = availableCoupons,
                DiscountPercent = discountPercent ?? 0,
                DiscountPrice = discountAmount ?? 0,
                MaxDiscountAmount = maxDiscount ?? 0
            };

            return View(cartVM);
        }
        public async Task<IActionResult> AvailableCoupons()
        {
            List<CartItemModel> cartItems;
            if (User.Identity.IsAuthenticated)
            {
                // 🗄️ Lấy giỏ hàng từ DB nếu đã đăng nhập
                string userId = User.Identity.Name;

                var dbCartItems = await _dataContext.CartItems
                    .Where(c => c.UserId == userId)
                    .ToListAsync();

                cartItems = dbCartItems.Select(c => new CartItemModel
                {
                    ProductId = c.ProductId,
                    ProductName = c.ProductName,
                    Quantity = c.Quantity,
                    Price = c.Price,
                    Image = c.Image,
                    ColorId = c.ColorId,
                    ColorName = c.ColorName
                }).ToList();
            }
            else
            {
                // Nếu chưa đăng nhập, lấy từ cookie
                cartItems = Request.Cookies.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
            }
            var productIds = cartItems.Select(c => c.ProductId).Distinct().ToList();
            var categoryIdsInCart = await _dataContext.Products
                .Where(p => productIds.Contains(p.Id))
                .Select(p => p.CategoryId)
                .Distinct()
                .ToListAsync();

            var now = DateTime.Now;
            var coupons = await _dataContext.Coupons
                .Where(c => c.DateExpired >= now && c.Quantity > 0 && c.Status == 1)
                .Where(c => _dataContext.CouponCategories
                    .Any(cc => cc.couponId == c.Id && categoryIdsInCart.Contains(cc.categoryId)))
                .ToListAsync();

            return View(coupons);
        }
        public IActionResult Checkout()
        {
            return View("~/Views/Checkout/Index.cshtml");
        }

        public async Task<IActionResult> Add(long Id,int colorId, int quantity)
        {
            //ProductModel product = await _dataContext.Products.FindAsync(Id);
            ProductModel product = await _dataContext.Products
                    .Include(p => p.ProductColors)
                    .ThenInclude(pc => pc.Color)
                    .FirstOrDefaultAsync(p => p.Id == Id);
            if (product != null)
            {
                var selectedColor = product.ProductColors.FirstOrDefault(pc => pc.ColorId == colorId);
                if (selectedColor == null || selectedColor.Quantity < quantity)
                {
                    return Json(new { success = false, message = "Màu sắc không hợp lệ hoặc không đủ số lượng" });
                }

                if (User.Identity.IsAuthenticated)
                {
                    string userId = User.Identity.Name;
                    var existingCartItem = await _dataContext.CartItems
                        .FirstOrDefaultAsync(c => c.ProductId == Id && c.ColorId == colorId && c.UserId == userId);
                    if (existingCartItem == null)
                    {
                        var newCartItem = new CartItemModel
                        {
                            ProductId = product.Id,
                            ProductName = product.Name,
                            Quantity = quantity,
                            Price = product.Price,
                            Image = product.Image,
                            ColorId = colorId,
                            ColorName = selectedColor.Color.Name,
                            UserId = userId
                        };
                        _dataContext.CartItems.Add(newCartItem);
                    }
                    else
                    {
                        existingCartItem.Quantity += quantity;
                        _dataContext.CartItems.Update(existingCartItem);
                    }
                    await _dataContext.SaveChangesAsync();
                }
                else
                {
                    //List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
                    // Lấy giỏ hàng từ cookies, nếu không có thì tạo mới
                    List<CartItemModel> cart = Request.Cookies.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
                    //CartItemModel cartItems = cart.Where(c => c.ProductId == Id).FirstOrDefault();
                    var cartItems = cart.FirstOrDefault(c => c.ProductId == Id && c.ColorId == colorId);

                    if (cartItems == null)
                    {
                        cart.Add(new CartItemModel(product, colorId, selectedColor.Color.Name, quantity));
                    }
                    else
                    {
                        cartItems.Quantity += quantity;
                        //cartItems.Quantity++;
                    }
                    //HttpContext.Session.SetJson("Cart", cart);
                    // Lưu giỏ hàng vào cookies
                    Response.Cookies.SetJson("Cart", cart);
                }
                TempData["success"] = "Sản phẩm đã được thêm vào giỏ hàng";
            }
            return RedirectToAction("Cart", "Cart");
        }

        public async Task<IActionResult> Decrease(int Id, int colorId)
        {
            if (User.Identity.IsAuthenticated)
            {
                string userId = User.Identity.Name;
                var cartItem = await _dataContext.CartItems
                    .FirstOrDefaultAsync(c => c.ProductId == Id && c.ColorId == colorId && c.UserId == userId);

                if (cartItem != null)
                {
                    // Kiểm tra tồn kho
                    var product = await _dataContext.Products
                        .Include(p => p.ProductColors)
                        .ThenInclude(pc => pc.Color)
                        .FirstOrDefaultAsync(p => p.Id == Id);

                    var selectedColor = product.ProductColors.FirstOrDefault(pc => pc.ColorId == colorId);

                    if (cartItem.Quantity > 1)
                    {
                        cartItem.Quantity--;
                        _dataContext.CartItems.Update(cartItem);
                    }
                    else
                    {
                        if (cartItem.Quantity > 1)
                        {
                            cartItem.Quantity--;
                            _dataContext.CartItems.Update(cartItem);
                        }
                        else
                        {
                            // Nếu chỉ còn 1 -> sau khi giảm thì xóa luôn khỏi giỏ
                            _dataContext.CartItems.Remove(cartItem);
                        }
                    }

                    await _dataContext.SaveChangesAsync();
                }
            }
            else
            {
                List<CartItemModel> cart = Request.Cookies.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
                //CartItemModel cartItem = cart.Where(c => c.ProductId == Id).FirstOrDefault();
                var cartItem = cart.FirstOrDefault(c => c.ProductId == Id && c.ColorId == colorId);

                if (cartItem != null)
                {
                    // Kiểm tra tồn kho
                    var product = await _dataContext.Products
                        .Include(p => p.ProductColors)
                        .ThenInclude(pc => pc.Color)
                        .FirstOrDefaultAsync(p => p.Id == Id);

                    var selectedColor = product.ProductColors.FirstOrDefault(pc => pc.ColorId == colorId);

                    if (cartItem.Quantity > 1)
                    {
                        cartItem.Quantity--;
                    }
                    else
                    {
                        if (cartItem.Quantity > 1)
                        {
                            cartItem.Quantity--;
                            _dataContext.CartItems.Update(cartItem);
                        }
                        else
                        {
                            // Nếu chỉ còn 1 -> sau khi giảm thì xóa luôn khỏi giỏ
                            _dataContext.CartItems.Remove(cartItem);
                        }
                    }
                }
                if (cart.Count == 0)
                {
                    Response.Cookies.Delete("Cart");
                }
                else
                {
                    Response.Cookies.SetJson("Cart", cart);
                }
            }
            return RedirectToAction("Cart");
        }

        public async Task<IActionResult> Increase(int Id, int colorId)
        {
            if (User.Identity.IsAuthenticated)
            {
                string userId = User.Identity.Name;
                var cartItem = await _dataContext.CartItems
                    .FirstOrDefaultAsync(c => c.ProductId == Id && c.ColorId == colorId && c.UserId == userId);

                if (cartItem != null)
                {
                    // Kiểm tra tồn kho
                    var product = await _dataContext.Products
                        .Include(p => p.ProductColors)
                        .ThenInclude(pc => pc.Color)
                        .FirstOrDefaultAsync(p => p.Id == Id);

                    var selectedColor = product.ProductColors.FirstOrDefault(pc => pc.ColorId == colorId);
                    if (cartItem != null)
                    {
                        if (selectedColor.Quantity >= cartItem.Quantity + 1)
                        {
                            cartItem.Quantity++;
                            _dataContext.CartItems.Update(cartItem);
                            await _dataContext.SaveChangesAsync();
                        }
                        else
                        {
                            TempData["errorr"] = "Sản phẩm không đủ số lượng trong kho!";
                        }
                    }
                }
            }
            else
            {
                List<CartItemModel> cart = Request.Cookies.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
                //CartItemModel cartItem = cart.Where(c => c.ProductId == Id).FirstOrDefault();
                var cartItem = cart.FirstOrDefault(c => c.ProductId == Id && c.ColorId == colorId);
                // Kiểm tra tồn kho
                var product = await _dataContext.Products
                    .Include(p => p.ProductColors)
                    .ThenInclude(pc => pc.Color)
                    .FirstOrDefaultAsync(p => p.Id == Id);

                var selectedColor = product.ProductColors.FirstOrDefault(pc => pc.ColorId == colorId);
                if (selectedColor.Quantity >= cartItem.Quantity + 1)
                {
                    cartItem.Quantity++;
                }
                else
                {
                    TempData["error"] = "Sản phẩm không đủ số lượng trong kho!";
                }
                if (cart.Count == 0)
                {
                    Response.Cookies.Delete("Cart");
                }
                else
                {
                    Response.Cookies.SetJson("Cart", cart);
                }
            }

            return RedirectToAction("Cart");
        }

        public async Task<IActionResult> Remove(int Id, int colorId)
        {
            // Kiểm tra xem người dùng đã đăng nhập chưa
            if (User.Identity.IsAuthenticated)
            {
                // 🗄️ Xóa sản phẩm khỏi cơ sở dữ liệu nếu người dùng đã đăng nhập
                string userId = User.Identity.Name;

                // Tìm sản phẩm trong giỏ hàng của người dùng
                var cartItem = await _dataContext.CartItems
                    .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == Id && c.ColorId == colorId);

                if (cartItem != null)
                {
                    // Xóa sản phẩm khỏi cơ sở dữ liệu
                    _dataContext.CartItems.Remove(cartItem);
                    await _dataContext.SaveChangesAsync();
                }
            }
            else
            {
                // Nếu chưa đăng nhập, xóa sản phẩm khỏi cookies
                List<CartItemModel> cart = Request.Cookies.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();

                // Xóa sản phẩm khỏi giỏ hàng
                cart.RemoveAll(p => p.ProductId == Id && p.ColorId == colorId);

                // Cập nhật lại cookies
                if (cart.Count == 0)
                {
                    Response.Cookies.Delete("Cart"); // Nếu giỏ hàng trống, xóa cookies
                }
                else
                {
                    Response.Cookies.SetJson("Cart", cart); // Cập nhật giỏ hàng vào cookies
                }
            }

            // Chuyển hướng về trang giỏ hàng sau khi xóa
            return RedirectToAction("Cart");
        }

        public async Task<IActionResult> Clear()
        {
            if (User.Identity.IsAuthenticated)
            {
                // Xóa giỏ hàng trong cơ sở dữ liệu cho người dùng đã đăng nhập
                string userId = User.Identity.Name;

                var cartItems = await _dataContext.CartItems
                    .Where(c => c.UserId == userId)
                    .ToListAsync();

                _dataContext.CartItems.RemoveRange(cartItems);
                await _dataContext.SaveChangesAsync();
            }
            else
            {
                // Xóa giỏ hàng trong cookies cho khách chưa đăng nhập
                Response.Cookies.Delete("Cart");
            }

            return RedirectToAction("Cart");
        }

        [HttpPost]
        [Route("Cart/GetShipping")]
        public async Task<IActionResult> GetShipping(ShippingModel shippingModel, string quan, string tinh, string phuong)
        {
            var existingShipping = await _dataContext.Shippings.
                FirstOrDefaultAsync(s => s.Ward == phuong && s.District == quan && s.City == tinh);
            decimal shippingPrice = 0; //set mặc định phí shipping là 0

            if(existingShipping != null)
            {
                shippingPrice = existingShipping.Price;
            }
            else
            {
                shippingPrice = 100000;
            }
            var shippingPriceJson = JsonConvert.SerializeObject(shippingPrice); // chuyển đổi thành chuỗi JSON

            try
            {
                var cookiesOptions = new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddDays(1), // Thời gian hết hạn là 1 ngày
                    HttpOnly = true, // Chỉ cho phép truy cập từ server
                    Secure = false, // Chỉ gửi cookie qua HTTPS
                    SameSite = SameSiteMode.Lax // Ngăn chặn gửi cookie trong các yêu cầu cross-site
                };
                //đẩy giá từ shippingPriceJson vào CookiesOption thông qua tên ShippingPrice
                Response.Cookies.Append("ShippingPrice", shippingPriceJson, cookiesOptions);
            }catch (Exception ex)
            {
               
                Console.WriteLine($"Error adding shipping price cookie: {ex.Message}");
            }
            return Json(new { shippingPrice });
        }
        [HttpPost]
        [Route("Cart/RemoveShippingCookie")]
        public IActionResult RemoveShippingCookie()
        {
            Response.Cookies.Delete("ShippingPrice");
            return Json(new { success = true });
        }


        [HttpPost]
        [Route("Cart/GetCoupon")]
        public async Task<IActionResult> GetCoupon(CouponModel couponModel, string coupon_value)
        {
            var validCoupon = await _dataContext.Coupons
                .FirstOrDefaultAsync(x => x.Name == coupon_value && x.Quantity >= 1);

            string couponTitle = validCoupon.Name + " | " + validCoupon?.Description;
            if (validCoupon != null)
            {
                if (couponTitle != null)
                {
                    TimeSpan remainingTime = validCoupon.DateExpired - DateTime.Now;
                    int daysRemaining = remainingTime.Days;

                    if (daysRemaining >= 0)
                    {
                        try
                        {
                            var cookieOptions = new CookieOptions
                            {
                                HttpOnly = true,
                                Expires = DateTimeOffset.UtcNow.AddDays(1),
                                Secure = false,
                                SameSite = SameSiteMode.Lax // Kiểm tra tính tương thích trình duyệt
                            };

                            Response.Cookies.Append("CouponTitle", couponTitle, cookieOptions);
                            return Ok(new { success = true, message = "Thêm mã giảm giá thành công" });
                        }
                        catch (Exception ex)
                        {
                            //trả về lỗi 
                            Console.WriteLine($"Có lỗi khi thêm mã giảm giá: {ex.Message}");
                            return Ok(new { success = false, message = "Thêm mã giảm giá thất bại" });
                        }
                    }
                    else
                    {

                        return Ok(new { success = false, message = "Mã giảm giá hết hạn" });
                    }

                }
                else
                {
                    return Ok(new { success = false, message = "Mã giảm giá ko tồn tại" });
                }
            }
            else
            {
                return Ok(new { success = false, message = "Mã giảm giá ko tồn tại" });
            }

            return Json(new { CouponTitle = couponTitle });
        }
    }

}
