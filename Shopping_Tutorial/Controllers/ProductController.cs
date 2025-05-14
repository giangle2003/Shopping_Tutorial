using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Models.ViewModels;
using Shopping_Tutorial.Repository;

namespace Shopping_Tutorial.Controllers
{
    public class ProductController : Controller
    {
        private readonly DataContext _dataContext;

        public ProductController(DataContext context)
        {
            _dataContext = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        //Chi tiết sản phẩm
        public async Task<IActionResult> Details(long Id)
        {     
            if(Id == 0)
            {
                return RedirectToAction("Index", "Home");
            }
            var productsById = _dataContext.Products
                .Include(p=>p.Ratings)
                .Include(p => p.ProductColors) // Bao gồm thông tin màu sắc
                 .ThenInclude(pc => pc.Color)  
                .Where(p => p.Id == Id)
                .FirstOrDefault();
        
            
            //SẢN PHẨM LIÊN QUAN
            //lấy các sản phẩm liên quan có cùng danh mục với sản phẩm đang xem
            var relatedProducts = await _dataContext.Products
                .Where(p => p.CategoryId == productsById.CategoryId && p.Id != Id) //Không lấy sản phẩm đang xem
                .Take(4) //Chỉ lấy 4 sản phẩm
                .ToListAsync();
            //Truyền sản phẩm liên quan qua view
            ViewBag.RelatedProducts = relatedProducts;

            /* RATING */
            //Lấy thông tin người dùng hiện tại
            var user = await _dataContext
                .Users
                .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (user != null)
            {
                ViewBag.UserName = user.UserName;
                ViewBag.UserEmail = user.Email;
            }

            //Kiểm tra xem người dùng đã đánh giá sản phẩm chưa
            bool hasRated = false;
            if(user != null)
            {
                hasRated = await _dataContext.Ratings
                    .AnyAsync(r => r.ProductId == Id && r.Name == user.UserName);
            }

            //Lấy đánh giá của sản phẩm
            var ratings = await _dataContext.Ratings
                .Where(r => r.ProductId == Id)
                .ToListAsync();

            // Kiểm tra nếu ProductColors là null hoặc không có màu nào
            if (productsById.ProductColors == null)
            {
                productsById.ProductColors = new List<ProductColor>();  // Khởi tạo danh sách rỗng nếu null
            }

            //Trả về view
            var viewModel = new ProductDetailsViewModel
            {
                ProductDetails = productsById,
                HasRated = hasRated,
                Ratings = ratings,
                 ProductColors = productsById.ProductColors.ToList() // Truyền màu sắc vào ViewModel

            };
            
            return View(viewModel);
        }

        public async Task<IActionResult> CommentProduct(RatingModel rating)
        {
            if (ModelState.IsValid)
            {
                //Lấy thông tin người dùng hiện tại
                var user = await _dataContext
                    .Users
                    .FirstOrDefaultAsync(u=>u.UserName == User.Identity.Name);
                if (user == null)
                {
                    TempData["Error"] = "Bạn cần đăng nhập để đánh giá sản phẩm";
                    return RedirectToAction("Login","Account");
                }
                //Lưu đánh giá vào database
                var ratingEntity = new RatingModel
                {
                    ProductId = rating.ProductId,
                    Comment = rating.Comment,
                    Name = user.UserName,
                    Email = user.Email,
                    Star = rating.Star,
                    CreatedAt = DateTime.Now
                };
                _dataContext.Ratings.Add(ratingEntity);
                await _dataContext.SaveChangesAsync();
                TempData["Success"] = "Đánh giá của bạn đã được gửi";
                return Redirect(Request.Headers["Referer"]);
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
                return RedirectToAction("Details", new {id = rating.ProductId});
            }
            return Redirect(Request.Headers["Referer"]);
        }

        //Tìm kiếm san pham
        public async Task<IActionResult> Search(string searchTerm)
        {
            //Nếu không có từ khóa tìm kiếm
            if (string.IsNullOrEmpty(searchTerm))
            {
                return RedirectToAction("Index", "Home");
            }
            //Tìm kiếm sản phẩm theo tên hoặc mô tả
            var products = await _dataContext.Products
                .Where(p => p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm))
                .ToListAsync();
            ViewBag.Keyword = searchTerm; //Truyền từ khóa tìm kiếm qua view
            return View(products);
        }
    }
}
