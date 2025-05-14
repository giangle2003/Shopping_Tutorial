using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Models.ViewModels;
using Shopping_Tutorial.Repository;

namespace Shopping_Tutorial.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Coupon")]
    [Authorize(Roles = "Admin,Author")]
    public class CouponController : Controller
    {
        private readonly DataContext _dataContext;
        public CouponController(DataContext context)
        {
            _dataContext = context;
        }

        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            var coupon_list = await _dataContext.Coupons.ToListAsync();
            ViewBag.Coupons = coupon_list;
            return View();
        }

        [Route("Create")]
        public IActionResult Create()
        {
            var model = new CouponViewModel
            {
                DateStar = DateTime.Now,  // Set a default value
                DateExpired = DateTime.Now.AddMonths(1), // Set a default value

                Categories = _dataContext.Categories.Where(c => c.Status == 1).ToList(),
                SelectedCategoryIds = new List<int>() // Khởi tạo danh sách rỗng
            };
           
            return View(model);
        }
        [Route("Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CouponViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Tạo Coupon mới
                var coupon = new CouponModel
                {
                    Name = model.Name,
                    Description = model.Description,
                    DateStar = model.DateStar,
                    DateExpired = model.DateExpired,
                    DiscountPercent = model.DiscountPercent,
                    DiscountPrice = model.DiscountPrice,
                    MaxDiscountAmount = model.MaxDiscountAmount,
                    Quantity = model.Quantity,
                    Status = model.Status
                };

                _dataContext.Coupons.Add(coupon);
                await _dataContext.SaveChangesAsync();

                // Lưu liên kết coupon với danh mục
                if (model.SelectedCategoryIds != null)
                {
                    foreach (var categoryId in model.SelectedCategoryIds)
                    {
                        var cc = new CouponCategory
                        {
                            couponId = coupon.Id,
                            categoryId = categoryId
                        };
                        _dataContext.CouponCategories.Add(cc);
                    }

                    await _dataContext.SaveChangesAsync();
                }
                TempData["success"] = "Thêm coupon thành công";
                return RedirectToAction("Index");

            }
            else
            {
                TempData["error"] = "Model có một vài thứ đang lỗi";
                List<string> errors = new List<string>();
                foreach (var value in ModelState.Values)
                {
                    foreach (var error in value.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }
                string errorMessage = string.Join("\n", errors);
                return BadRequest(errorMessage);
            }

            // Nếu có lỗi, load lại danh sách danh mục để hiển thị checkbox
            model.Categories = _dataContext.Categories.ToList();
            return View( model);
        }


        [Route("Edit")]
        public async Task<IActionResult> Edit(int Id)
        {
            var coupon = await _dataContext.Coupons.FindAsync(Id);

            if (coupon == null)
            {
                return NotFound();
            }

            var categories = await _dataContext.Categories.ToListAsync();

            var selectedCategoryIds = await _dataContext.CouponCategories
                .Where(cc => cc.couponId == Id)
                .Select(cc => cc.categoryId)
                .ToListAsync();

            var model = new CouponViewModel
            {
                Id = coupon.Id,
                Name = coupon.Name,
                Description = coupon.Description,
                DateStar = coupon.DateStar,
                DateExpired = coupon.DateExpired,
                Quantity = coupon.Quantity,
                DiscountPercent = coupon.DiscountPercent,
                MaxDiscountAmount = coupon.MaxDiscountAmount,
                DiscountPrice = coupon.DiscountPrice,
                Status = coupon.Status,

                Categories = categories,
                SelectedCategoryIds = selectedCategoryIds
            };

            return View(model);
        }

        [Route("Edit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CouponViewModel model)
        {
            if (ModelState.IsValid)
            {

                var coupon = await _dataContext.Coupons.FindAsync(model.Id);
                if (coupon == null) return NotFound();

                // Cập nhật coupon
                coupon.Name = model.Name;
                coupon.Description = model.Description;
                coupon.DateStar = model.DateStar;
                coupon.DateExpired = model.DateExpired;
                coupon.Quantity = model.Quantity;
                coupon.DiscountPercent = model.DiscountPercent;
                coupon.MaxDiscountAmount = model.MaxDiscountAmount;
                coupon.DiscountPrice = model.DiscountPrice;
                coupon.Status = model.Status;

                _dataContext.Coupons.Update(coupon);

                // Xóa danh mục cũ
                var existingCategories = _dataContext.CouponCategories.Where(cc => cc.couponId == coupon.Id);
                _dataContext.CouponCategories.RemoveRange(existingCategories);
                
                // Thêm lại danh mục mới
                if (model.SelectedCategoryIds != null)
                {
                    foreach (var categoryId in model.SelectedCategoryIds)
                    {
                        _dataContext.CouponCategories.Add(new CouponCategory
                        {
                            couponId = coupon.Id,
                            categoryId = categoryId
                        });
                    }
                }

                await _dataContext.SaveChangesAsync();

                TempData["success"] = "Sửa coupon thành công";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["errorr"] = "Có lỗi xảy ra khi thêm mới thương hiệu";
                List<string> errors = new List<string>();
                foreach (var value in ModelState.Values)
                {
                    foreach (var error in value.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }
                string errorMessage = string.Join("\n", errors);
                return BadRequest(errorMessage);
            }
            return View(model);
        }

        public async Task<IActionResult> Delete(int id)
        {
            CouponModel coupon = await _dataContext.Coupons.FindAsync(id);

           
            _dataContext.Coupons.Remove(coupon);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Xóa coupan thành công";
            return RedirectToAction("Index");
        }


    }
}
