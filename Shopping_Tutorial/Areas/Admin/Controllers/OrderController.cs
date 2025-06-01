using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Repository;

namespace Shopping_Tutorial.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Order")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly DataContext _dataContext;

        public OrderController(DataContext context)
        {
            _dataContext = context;
        }

        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            return View(await _dataContext.Orders.OrderByDescending(p => p.Id).ToListAsync());
        }
        [HttpGet]
        [Route("ViewOrder")]
        public async Task<IActionResult> ViewOrder(string ordercode)
        {
            var DetailsOrder = await _dataContext.OrderDetails.
                                AsNoTracking()
                                .Include(od => od.Product) // Join với bảng Product
                                .Where(o => o.OrderCode == ordercode)
                                .OrderByDescending(o => o.OrderDate)  // Sắp xếp giảm dần theo OrderDate
                                .ToListAsync();
            if (DetailsOrder == null || !DetailsOrder.Any())
            {
                TempData["errorr"] = "Không tìm thấy chi tiết đơn hàng!";
                return RedirectToAction("Index", "Order");  // Hoặc chuyển hướng về trang đơn hàng
            }
            // Lấy trạng thái đơn hàng từ bảng Orders
            var orderStatus = await _dataContext.Orders
                                 .Where(o => o.OrderCode == ordercode)
                                 .Select(o => o.Status)
                                 .FirstOrDefaultAsync();

            //Lấy phí shipping từ bảng Orders
            var ShippingCost = _dataContext.Orders
                .Where(o => o.OrderCode == ordercode)
                .Select(o => o.ShippingCost)
                .FirstOrDefault();
            var Discount = _dataContext.Orders
                .Where(o => o.OrderCode == ordercode)
                .Select(o => o.Discount)
                .FirstOrDefault();
            ViewBag.OrderStatus = orderStatus; // Truyền trạng thái sang View
            ViewBag.ShippingCost = ShippingCost; // Truyền phí shipping sang View
            ViewBag.Discount = Discount; // Truyền giảm giá sang View
            return View(DetailsOrder);
        }

        [HttpPost]
        [Route("UpdateOrder")]
        public async Task<IActionResult> UpdateOrder(string ordercode, int status)
        {
            var order = await _dataContext.Orders.FirstOrDefaultAsync(o => o.OrderCode == ordercode); // Tìm đơn hàng theo ordercode
            if (order == null)
            {
                return NotFound();
            }
            order.Status = status; // Cập nhật trạng thái đơn hàng
            if(status == 4) // Nếu trạng thái là đã giao hàng
            {
                var orderDetails = await _dataContext.OrderDetails.Where(od => od.OrderCode == ordercode).ToListAsync();
                if (orderDetails != null && orderDetails.Count > 0)
                {
                    UpdateStatisticalFromOrder(order, orderDetails); // Cập nhật thống kê
                }
            }

            try
            {
                await _dataContext.SaveChangesAsync();
                return Ok(new { success = true, message = "Update tình trạng đơn hàng thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Có lỗi xảy ra khi update đơn hàng.");
            }

        }
        //Cập nhật bảng thống kê
        private void UpdateStatisticalFromOrder(OrderModel order, List<OrderDetails> orderDetails)
        {
            decimal totalRevenue = 0;
            decimal totalProfit = 0;
            int totalQuantity = 0;
            foreach (var detail in orderDetails)
            {
                var revenue = detail.Price * detail.Quantity;
                var profit = (detail.Price - detail.ImportPrice) * detail.Quantity;

                totalRevenue += revenue;
                totalProfit += profit;
                totalQuantity += detail.Quantity;
            }

            totalRevenue -= order.Discount; // Trừ đi giảm giá
            totalProfit -= order.Discount; // Trừ đi giảm giá
            // Lấy ngày (loại bỏ giờ phút giây để so sánh đúng)
            var date = order.CreatedDate.Date;

            // Kiểm tra xem đã có thống kê cho ngày đó chưa
            var existingStat = _dataContext.Statisticals.FirstOrDefault(s => s.DateCreated.Date == date);

            if (existingStat != null)
            {
                // Cập nhật thống kê cũ
                existingStat.Revenue += totalRevenue;
                existingStat.Profit += totalProfit;
                existingStat.Quantity += totalQuantity;
                existingStat.Sold += 1;

                _dataContext.Statisticals.Update(existingStat);
            }
            else
            {
                // Tạo thống kê mới cho ngày đó
                var stat = new StatisticalModel
                {
                    Revenue = totalRevenue,
                    Profit = totalProfit,
                    Quantity = totalQuantity,
                    Sold = 1,
                    DateCreated = date
                };

                _dataContext.Statisticals.Add(stat);
            }

            _dataContext.SaveChanges();
        }

        public async Task<IActionResult> Delete(string ordercode)
        {
            var order = await _dataContext.Orders.FirstOrDefaultAsync(o => o.OrderCode == ordercode);
            if (order == null)
            {
                TempData["errorr"] = "Không tìm thấy đơn hàng!";
                return RedirectToAction("Index", "Order");  // Hoặc chuyển hướng về trang đơn hàng
            }

            try
            {
                _dataContext.Orders.Remove(order);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Xóa đơn hàng thành công!";
                return RedirectToAction("Index", "Order");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while deleting the order.");
            }

        }
    }
}
