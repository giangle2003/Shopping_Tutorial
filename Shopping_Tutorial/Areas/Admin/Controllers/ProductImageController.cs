using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Repository;

namespace Shopping_Tutorial.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("[area]/[controller]")]
    [Authorize(Roles = "Admin")]
    public class ProductImageController : Controller
    {
        private readonly DataContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductImageController(DataContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: admin/productimage/index
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            // Lấy danh sách sản phẩm kèm ảnh chính
            var products = await _context.Products
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Image
                })
                .ToListAsync();

            return View(products);
        }

        // GET: admin/productimage/manageimages/{productId}
        [Route("ManageImages/{productId}")]
        public async Task<IActionResult> ManageImages(long productId)
        {
            var product = await _context.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null) return NotFound();

            ViewBag.ProductName = product.Name;
            ViewBag.ProductId = product.Id;
            return View(product.ProductImages);
        }

        // POST: admin/productimage/manageimages/{productId}
        [HttpPost]
        [Route("ManageImages/{productId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageImages(long productId, List<IFormFile> images)
        {
            if (images == null || images.Count == 0)
            {
                ModelState.AddModelError("", "Vui lòng chọn ảnh để tải lên");
                return RedirectToAction(nameof(ManageImages), new { productId });
            }

            var product = await _context.Products.FindAsync(productId);
            if (product == null) return NotFound();

            string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");

            foreach (var image in images)
            {
                if (image.Length > 0)
                {
                    var imageName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                    var filePath = Path.Combine(uploadDir, imageName);

                    using (var fs = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(fs);
                    }

                    var productImage = new ProductImage
                    {
                        ProductId = productId,
                        ImagePath = "/media/products/" + imageName
                    };

                    _context.ProductImages.Add(productImage);
                }
            }

            await _context.SaveChangesAsync();
            TempData["success"] = "Tải ảnh thành công";
            return RedirectToAction(nameof(ManageImages), new { productId });
        }

        // POST: admin/productimage/deleteimage
        [HttpPost]
        [Route("DeleteImage")]
        public async Task<JsonResult> DeleteImage(int id)
        {
            var image = await _context.ProductImages.FindAsync(id);
            if (image == null) return Json(new { success = false, message = "Ảnh không tồn tại" });

            // Xóa file trên server
            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, image.ImagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            _context.ProductImages.Remove(image);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
    }
}

