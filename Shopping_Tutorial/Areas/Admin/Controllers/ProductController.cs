using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Repository;
using System.Linq;

namespace Shopping_Tutorial.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("[area]/[controller]")]
    [Authorize(Roles ="Admin")]
    public class ProductController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(DataContext context,IWebHostEnvironment webHostEnvironment)
        {
            _dataContext = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [Route("Index")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
           return View(await _dataContext.Products.OrderByDescending(p => p.Id).Include(p => p.Category).Include(p => p.Brand).Include(p=>p.ProductColors).ThenInclude(pc => pc.Color).ToListAsync());
        }

        [Route("Create")]
        [HttpGet]
        public  IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name");
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name");
            return View();
        }

        [Route("Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductModel product)
        {
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name",product.CategoryId);
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name",product.BrandId);
            if(ModelState.IsValid)
            {
                product.Description = product.Description.Replace("<p>", "").Replace("</p>", "");

                //Thêm dữ liệu vào database
                product.Slug = product.Name.Replace(" ","-");
                var slug = await _dataContext.Products.FirstOrDefaultAsync(p => p.Slug == product.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "Sản phẩm đã tồn tại");
                    return View(product);
                }
               
                if(product.ImageUpload != null)
                {
                    string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
                    string imageName = Guid.NewGuid().ToString() + "-" + product.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadDir, imageName);

                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await product.ImageUpload.CopyToAsync(fs);
                    fs.Close();
                    product.Image = imageName;
                 }
                
                _dataContext.Add(product);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Thêm mới sản phẩm thành công";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["errorr"] = "Có lỗi xảy ra khi thêm mới sản phẩm";
                List<string> errors = new List<string>();
                foreach (var value in ModelState.Values)
                {
                    foreach(var error in value.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }
                string errorMessage = string.Join("\n", errors);
                return BadRequest(errorMessage);
            }
            return View(product);
        }

        [Route("Edit")]
        public async Task<IActionResult> Edit(long id)
        {
            ProductModel product = await _dataContext.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);

            // Truyền tên ảnh hiện tại vào ViewBag
            ViewBag.CurrentImageName = product.Image;

            return View(product);
        }

        [Route("Edit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long Id,ProductModel product)
        {
            var existed_product = _dataContext.Products.Find(product.Id); //tìm sp theo id product
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);
            if (ModelState.IsValid)
            {

                //Thêm dữ liệu vào database
                product.Slug = product.Name.Replace(" ", "-");

                if (product.ImageUpload != null)
                {
                    string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
                    string imageName = Guid.NewGuid().ToString() + "-" + product.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadDir, imageName);

                    // Xóa ảnh cũ nếu có
                    if (!string.IsNullOrEmpty(existed_product.Image))
                    {
                        string oldImagePath = Path.Combine(uploadDir, existed_product.Image);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath); // Xóa ảnh cũ
                        }
                    }

                    using (FileStream fs = new FileStream(filePath, FileMode.Create))
                    {
                        await product.ImageUpload.CopyToAsync(fs);
                    }
                    existed_product.Image = imageName;
                }

                // Cập nhật thông tin
                existed_product.Name = product.Name;
                existed_product.Description = product.Description;
                existed_product.Price = product.Price;
                existed_product.CategoryId = product.CategoryId;
                existed_product.BrandId = product.BrandId;
                existed_product.ImportPrice = product.ImportPrice;
                existed_product.OriginalPrice = product.OriginalPrice;

                _dataContext.Update(existed_product);
                await _dataContext.SaveChangesAsync();

                // Cập nhật ViewBag để hiển thị ảnh mới
                ViewBag.CurrentImageName = existed_product.Image;

                TempData["success"] = "Cập nhật sản phẩm thành công";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["errorr"] = "Có lỗi xảy ra khi sửa sản phẩm";
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
            return View(product);
        }

        public async Task<IActionResult> Delete(long id)
        {
            ProductModel product = await _dataContext.Products.FindAsync(id);

            // Kiểm tra xem sản phẩm đã có đơn hàng chưa
            bool hasOrder = await _dataContext.OrderDetails.AnyAsync(o => o.ProductId == id && o.Order.Status == 1);
            if (hasOrder)
            {
                TempData["errorr"] = "Không thể xóa sản phẩm vì còn đơn hàng chưa xử lý!";
                return RedirectToAction("Index");
            }

            if (!string.Equals(product.Image, "noimage.jpg"))
            {
                string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
                string oldfileImage = Path.Combine(uploadDir, product.Image);
                if (System.IO.File.Exists(oldfileImage))
                {
                    System.IO.File.Delete(oldfileImage);
                }
            }
            _dataContext.Products.Remove(product);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Xóa sản phẩm thành công";
            return RedirectToAction("Index");
        }
        
        //Thêm màu sắc
        [Route("AddColor")]
        public IActionResult AddColor(long id)
        {
            // Lấy sản phẩm từ cơ sở dữ liệu
            var product = _dataContext.Products
                .Include(p => p.ProductColors)
                .FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            // Lấy các màu sắc có sẵn từ cơ sở dữ liệu
            var colors = _dataContext.Colors.ToList();
            ViewData["Colors"] = new SelectList(colors, "Id", "Name");  // Truyền danh sách màu sắc vào ViewData

            return View(product);  // Trả về view với thông tin sản phẩm
        }

        // Action để xử lý việc thêm màu vào sản phẩm
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("AddColor")]
        public IActionResult AddColor(long id, int colorId, int quantity)
        {
            var product = _dataContext.Products
                .Include(p => p.ProductColors)
                .FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            // Kiểm tra xem màu sắc đã có trong sản phẩm hay chưa
            var existingColor = product.ProductColors
                .FirstOrDefault(pc => pc.ColorId == colorId);
            // Tạo mối quan hệ giữa sản phẩm và màu sắc
            if (existingColor == null)
            {
                // Thêm màu sắc mới vào sản phẩm
                product.ProductColors.Add(new ProductColor
                {
                    ProductId = product.Id,
                    ColorId = colorId,
                    Quantity = quantity
                });
            }
            else
            {
                // Nếu màu sắc đã có, cập nhật số lượng
                existingColor.Quantity += quantity;
            }

            // Cập nhật lại tổng số lượng sản phẩm
            product.Quantity = product.ProductColors.Sum(pc => pc.Quantity);

            // Cập nhật vào cơ sở dữ liệu
            _dataContext.Products.Update(product);
            _dataContext.SaveChanges();

            return RedirectToAction("AddColor", new { id = product.Id });
        }
        [HttpPost]
        [Route("RemoveColor")]
        public IActionResult RemoveColor(long productId, int colorId)
        {
            var productColor = _dataContext.ProductColors
                .FirstOrDefault(pc => pc.ProductId == productId && pc.ColorId == colorId);

            if (productColor != null)
            {
                _dataContext.ProductColors.Remove(productColor);

                // Sau khi xóa, cập nhật lại tổng số lượng sản phẩm
                var product = _dataContext.Products
                    .Include(p => p.ProductColors)
                    .FirstOrDefault(p => p.Id == productId);

                if (product != null)
                {
                    product.Quantity = product.ProductColors
                        .Where(pc => pc.ColorId != colorId) // loại bỏ màu đã bị xóa
                        .Sum(pc => pc.Quantity);
                    _dataContext.Products.Update(product);
                }

                _dataContext.SaveChanges();
                TempData["success"] = "Đã xóa màu khỏi sản phẩm.";
            }
            else
            {
                TempData["error"] = "Không tìm thấy màu cần xóa.";
            }

            return RedirectToAction("AddColor", new { id = productId });
        }





        //[HttpPost]
        //[Route("UpdateProductQuantity")]
        //public IActionResult UpdateProductQuantity()
        //{
        //    var products = _dataContext.Products
        //        .Include(p => p.ProductColors)
        //        .ToList();

        //    foreach (var product in products)
        //    {
        //        // Tính tổng số lượng đã bán của tất cả màu sắc
        //        product.Quantity = product.ProductColors.Sum(pc => pc.Quantity);

        //        // Cập nhật lại vào cơ sở dữ liệu
        //        _dataContext.Products.Update(product);
        //    }
        //    _dataContext.SaveChanges();  // Lưu thay đổi vào cơ sở dữ liệu

        //    // Quay lại trang hiển thị hoặc trả về một phản hồi thành công
        //    return RedirectToAction("Index");
        //}

        //[HttpPost]
        //[Route("UpdateProductQuantitySold")]
        //public IActionResult UpdateProductQuantitySold()
        //{
        //    var products = _dataContext.Products
        //        .Include(p => p.ProductColors)
        //        .ToList();

        //    foreach (var product in products)
        //    {
        //        // Tính tổng số lượng đã bán của tất cả màu sắc
        //        product.Sold = product.ProductColors.Sum(pc => pc.Sold);

        //        // Cập nhật lại vào cơ sở dữ liệu
        //        _dataContext.Products.Update(product);
        //    }
        //    _dataContext.SaveChanges();  // Lưu thay đổi vào cơ sở dữ liệu

        //    // Quay lại trang hiển thị hoặc trả về một phản hồi thành công
        //    return RedirectToAction("Index");
        //}


        //Thêm số lượng
        //[Route("AddQuantity")]
        //[HttpGet]

        //public async Task<IActionResult> AddQuantity(int Id)
        //{
        //    // Lấy ra sản phẩm từ có cùng Id sản phẩm từ bảng ProductQuantity
        //    var productbyquantity = await _dataContext.ProductQuantities.Where(pq => pq.ProductId == Id).ToListAsync();
        //    ViewBag.ProductByQuantity = productbyquantity;
        //    ViewBag.Id = Id;
        //    return View();
        //}
        //[Route("StoreProductQuantity")]
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult StoreProductQuantity(ProductQuantityModel productQuantityModel)
        //{
        //    // lây ra sản phẩm từ database
        //    var product = _dataContext.Products.Find(productQuantityModel.ProductId);

        //    if (product == null)
        //    {
        //        return NotFound(); 
        //    }
        //    // tăng số lượng sản phẩm sau khi thêm số lượng
        //    product.Quantity += productQuantityModel.Quantity;

        //    productQuantityModel.Quantity = productQuantityModel.Quantity;
        //    productQuantityModel.ProductId = productQuantityModel.ProductId;
        //    productQuantityModel.DateCreated = DateTime.Now;


        //    _dataContext.Add(productQuantityModel);
        //    _dataContext.SaveChangesAsync();
        //    TempData["success"] = "Thêm số lượng sản phẩm thành công";
        //    return RedirectToAction("AddQuantity", "Product", new { Id = productQuantityModel.ProductId });
        //}


    }

}
