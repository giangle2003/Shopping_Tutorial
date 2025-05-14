using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Repository;

namespace Shopping_Tutorial.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Color")]
    [Authorize(Roles = "Admin,Author")]

    public class ColorController : Controller
    {
        private readonly DataContext _dataContext;

        public ColorController(DataContext context)
        {
            _dataContext = context;
        }

        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            return View(await _dataContext.Colors.OrderBy(p => p.Id).ToListAsync());
        }
        [Route("Create")]
        public IActionResult Create()
        {
            return View();
        }

        [Route("Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ColorModel color)
        {
            if (ModelState.IsValid)
            {

                _dataContext.Add(color);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Thêm mới màu sắc thành công";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["errorr"] = "Có lỗi xảy ra khi thêm mới màu sắc";
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
            return View(color);
        }
        public async Task<IActionResult> Delete(int id)
        {
            var color = await _dataContext.Colors.FindAsync(id);

            if (color == null)
            {
                return NotFound();
            }

            // Kiểm tra xem màu có đang được sử dụng trong sản phẩm không
            bool isUsed = await _dataContext.ProductColors.AnyAsync(pc => pc.ColorId == id);

            if (isUsed)
            {
                TempData["errorr"] = "Không thể xóa. Màu sắc này đang được sử dụng trong các sản phẩm.";
                return RedirectToAction("Index");
            }

            _dataContext.Colors.Remove(color);
            await _dataContext.SaveChangesAsync();

            TempData["success"] = "Xóa màu sắc thành công.";
            return RedirectToAction("Index");
        }



    }
}
