using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Shopping_Tutorial.Areas.Admin.Repository;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Repository;
using Shopping_Tutorial.Services.Momo;
using Shopping_Tutorial.Services.Vnpay;
using System;
using System.Security.Claims;

namespace Shopping_Tutorial.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IEmailSender _emailSender; //Gửi email
        private readonly ICompositeViewEngine _viewEngine; //Render view to string
        private readonly ITempDataProvider _tempDataProvider; //Render view to string
        private IMomoService _momoService;
        private readonly IVnPayService _vnPayService;

        public CheckoutController(IMomoService momoService,DataContext dataContext, IEmailSender emailSender, ICompositeViewEngine viewEngine, ITempDataProvider tempDataProvider, IVnPayService vnPayService)
        {
            _emailSender = emailSender;
            _dataContext = dataContext;
            _viewEngine = viewEngine;
            _tempDataProvider = tempDataProvider;
            _momoService = momoService;
            _vnPayService = vnPayService;
        }

        public async Task<IActionResult> Checkout(string OrderId, string fullAddress, string phoneNumber)
        {
            try
            {
                var userEmail = User.FindFirstValue(ClaimTypes.Email);
                var userName = User.FindFirstValue(ClaimTypes.Name);
                var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
                if (user == null)
                    return RedirectToAction("Login", "Account");

                // Lấy cart từ DB thay vì cookie
                var cartItems = await _dataContext.CartItems
                    .Where(c => c.UserId == userName)
                    .ToListAsync();

                if (cartItems == null || !cartItems.Any())
                {
                    Console.WriteLine("Giỏ hàng trống khi checkout");
                    return RedirectToAction("Fail", "Checkout");
                }

                // Tạo đơn hàng
                var ordercode = Guid.NewGuid().ToString();
                var orderItem = new OrderModel
                {
                    OrderCode = ordercode,
                    ShippingAddress = fullAddress,
                    PhonenumberDelivery = phoneNumber,
                    UserName = userEmail,
                    CreatedDate = DateTime.Now,
                    PaymentMethod = string.IsNullOrEmpty(OrderId) ? "COD" : OrderId,
                    Status = 1
                };

                // Nhận phí shipping từ cookie (nếu có)
                var shippingPriceCookie = Request.Cookies["ShippingPrice"];
                if (!string.IsNullOrEmpty(shippingPriceCookie))
                {
                    orderItem.ShippingCost = JsonConvert.DeserializeObject<decimal>(shippingPriceCookie);
                    Response.Cookies.Delete("ShippingPrice");
                }

                // Xử lý mã giảm giá
                var coupon_code = Request.Cookies["CouponTitle"];
                decimal discount = 0;
                decimal totalPrice = cartItems.Sum(item => item.Price * item.Quantity);
                orderItem.CouponCode = coupon_code;

                if (!string.IsNullOrEmpty(coupon_code))
                {
                    var coupon = await _dataContext.Coupons
                        .FirstOrDefaultAsync(c => (c.Name + " | " + c.Description) == coupon_code);

                    if (coupon != null && coupon.Quantity > 0)
                    {
                        coupon.Quantity -= 1;
                        _dataContext.Coupons.Update(coupon);

                        if (coupon.DiscountPercent > 0)
                        {
                            discount = (totalPrice * coupon.DiscountPercent.Value) / 100;
                            if (coupon.MaxDiscountAmount.HasValue && discount > coupon.MaxDiscountAmount.Value)
                                discount = coupon.MaxDiscountAmount.Value;
                        }
                        else if (coupon.DiscountPrice > 0)
                        {
                            discount = coupon.DiscountPrice.Value;
                        }
                        orderItem.Discount = discount;
                    }
                }

                _dataContext.Orders.Add(orderItem);
                await _dataContext.SaveChangesAsync();

                // Chi tiết đơn hàng
                var orderDetailsList = new List<OrderDetails>();
                foreach (var cart in cartItems)
                {
                    var product = await _dataContext.Products.FirstOrDefaultAsync(p => p.Id == cart.ProductId);
                    var orderdetails = new OrderDetails
                    {
                        UserName = user.UserName,
                        OrderId = orderItem.Id,
                        OrderCode = ordercode,
                        ProductId = cart.ProductId,
                        Price = cart.Price,
                        Quantity = cart.Quantity,
                        OrderDate = DateTime.Now,
                        Name = product?.Name ?? "N/A",
                        ImportPrice = product?.ImportPrice ?? 0
                    };

                    // Cập nhật số lượng tồn kho và đã bán
                    var productColor = await _dataContext.ProductColors
                        .FirstOrDefaultAsync(pc => pc.ProductId == cart.ProductId && pc.ColorId == cart.ColorId);
                    if (productColor != null)
                    {
                        productColor.Quantity -= cart.Quantity;
                        productColor.Sold += cart.Quantity;
                        _dataContext.ProductColors.Update(productColor);
                    }

                    if (product != null)
                    {
                        var allColors = await _dataContext.ProductColors
                            .Where(pc => pc.ProductId == product.Id).ToListAsync();
                        product.Sold = allColors.Sum(pc => pc.Sold);
                        _dataContext.Products.Update(product);
                    }

                    _dataContext.OrderDetails.Add(orderdetails);
                    orderDetailsList.Add(orderdetails);
                }

                // Xóa cart DB sau khi checkout
                _dataContext.CartItems.RemoveRange(cartItems);
                await _dataContext.SaveChangesAsync();

                // Gửi email xác nhận đơn hàng
                string emailBody = await RenderViewToStringAsync("Emails/OrderEmail", orderDetailsList);
                await _emailSender.SendEmailAsync(userEmail, "Đơn hàng mới", emailBody);

                return RedirectToAction("Success", "Checkout");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi checkout: {ex.Message}");
                return RedirectToAction("Fail", "Checkout");
            }
        }

        //Thanh toán thành công
        public IActionResult Success()
        {
            return View();
        }

        //Thanh toán thất bại
        public IActionResult Fail()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> PaymentCallBack(MomoInforModel model)
        {
            var requestQuery = HttpContext.Request.Query; //Lấy query string từ request
            var response =  _momoService.PaymentExecuteAsync(HttpContext.Request.Query);  // <- thêm await



            if (requestQuery["errorCode"] == "0")
            {
                var newMomoInsert = new MomoInforModel
                {
                    OrderId = requestQuery["orderId"],
                    Amount = decimal.Parse(requestQuery["amount"]),
                    OrderInfo = requestQuery["orderInfo"],
                    FullName = User.FindFirstValue(ClaimTypes.Email),
                    DatePaid = DateTime.Now,
                };
                _dataContext.Add(newMomoInsert);
                await _dataContext.SaveChangesAsync();

                var fullAddress = Request.Cookies["FullAddress"]?.Trim();
                var phoneNumber = Request.Cookies["PhoneNumber"]?.Trim();

                var orderId = requestQuery["orderId"];
                // Kiểm tra Session/Cookie trước khi gọi Checkout
                if (string.IsNullOrEmpty(fullAddress) || string.IsNullOrEmpty(phoneNumber) )
                {
                    return RedirectToAction("Fail", "Checkout");
                }
                //Đặt hàng sau khi thanh toán thành công

                var checkoutResult = await Checkout(orderId, fullAddress, phoneNumber);

                if (checkoutResult is RedirectToActionResult redirectResult)
                {
                    // Debug log để kiểm tra xem RedirectToActionResult có được trả về không
                    Console.WriteLine($"RedirectToActionResult: {redirectResult.ActionName}");

                    if (redirectResult.ActionName.Equals("Fail", StringComparison.OrdinalIgnoreCase))
                    {
                        // Nếu checkout thất bại
                        return RedirectToAction("PaymentFail", "Checkout");
                    }
                    else if (redirectResult.ActionName.Equals("Success", StringComparison.OrdinalIgnoreCase))
                    {
                        // Nếu checkout thành công
                        return RedirectToAction("Success", "Checkout");
                    }
                }

                // Nếu không có RedirectToActionResult trả về (không mong đợi)
                return RedirectToAction("PaymentFail", "Checkout");

            }
            else
            {
                return RedirectToAction("PaymentFail", "Checkout");
            }
            //return View(response);
        }
        //View thông báo thanh toán Fail của Momo
        public IActionResult PaymentFail()
        {
            return View();
        }
        [HttpGet]
        public IActionResult PaymentCallbackVnpay()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            return Json(response);
        }

        //Để chuyển view thành string
        private async Task<string> RenderViewToStringAsync(string viewName, object model)
        {
            var httpContext = new DefaultHttpContext { RequestServices = HttpContext.RequestServices };
            var actionContext = new ActionContext(httpContext, RouteData, ControllerContext.ActionDescriptor);

            using var sw = new StringWriter();
            var viewResult = _viewEngine.FindView(actionContext, viewName, false);

            if (viewResult.View == null)
                throw new ArgumentNullException($"{viewName} không tồn tại.");

            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = model
            };

            var tempData = new TempDataDictionary(actionContext.HttpContext, _tempDataProvider);
            var viewContext = new ViewContext(
                actionContext,
                viewResult.View,
                viewData,
                tempData,
                sw,
                new HtmlHelperOptions()
            );

            await viewResult.View.RenderAsync(viewContext);
            return sw.ToString();
        }

    }
}
