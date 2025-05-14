using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Repository;

namespace Shopping_Tutorial.Controllers;

public class HomeController : Controller
{
    private readonly DataContext _dataContext;

    private readonly ILogger<HomeController> _logger;
    private readonly UserManager<AppUserModel> _userManager;

    public HomeController(ILogger<HomeController> logger, DataContext context, UserManager<AppUserModel> userManager )
    {
        _logger = logger;
        _dataContext = context;
        _userManager = userManager;
    }

    public IActionResult Index(int pg = 1)
    {
        int pageSize = 9; // số sản phẩm trên mỗi trang
        var totalProducts = _dataContext.Products.Count(); // tổng số sản phẩm

        var pager = new Paginate(totalProducts, pg, pageSize);

        //var products = _dataContext.Products.Include("Category").Include("Brand").ToList();

        var products = _dataContext.Products            
            .OrderBy(p => p.Id)
            .Skip((pg - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        var sliders = _dataContext.Sliders.Where(s => s.Status == 1).ToList();
        ViewBag.Sliders = sliders;
        ViewBag.Pager = pager; // Gửi đối tượng pager về view
        return View(products);
    }

    // Trang liên h?
    public IActionResult Contact()
    {
        var contact = _dataContext.Contact.FirstOrDefault();
        return View(contact);
    }

    // Trang yêu thích
    public async Task<IActionResult> WishList()
    {
        // L?y danh sách sản phẩm trong wishlist
        var wishlist_product = await (from w in _dataContext.Wishlists
                                      join p in _dataContext.Products on w.ProductId equals p.Id
                                      select new { ProductId = p.Id, Product = p, Wishlists = w })
                               .ToListAsync();

        return View(wishlist_product);
    }
    [HttpPost]
    public async Task<IActionResult> AddWishList(long Id, WishlistModel wishlist)
    {
        var user = await _userManager.GetUserAsync(User); //Lấy người dùng hiện tại

        // Kiểm tra xem sản phẩm đã có trong danh sách so sánh chưa
        bool exists = await _dataContext.Wishlists
            .AnyAsync(c => c.ProductId == Id && c.UserId == user.Id);
        if (exists)
        {
            return Json(new { success = false, message = "Bạn đã thêm sản phẩm vào yêu thích trước đó" });
        }

        //Tạo một đối tượng wishlist mới
        var wishlistProduct = new WishlistModel
        {
            ProductId = Id,
            UserId = user.Id
        };

        _dataContext.Wishlists.Add(wishlistProduct);
        try
        {
            await _dataContext.SaveChangesAsync();
            return Json(new { success = true, message = "Thêm yêu thích thành công!" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Đã lỗi khi thêm sản phẩm vào yêu thích");
        }
    }
    public async Task<IActionResult> DeleteWishlist(int Id)
    {
        WishlistModel wishlist = await _dataContext.Wishlists.FindAsync(Id);

        _dataContext.Wishlists.Remove(wishlist);

        await _dataContext.SaveChangesAsync();
        TempData["success"] = "Sản phẩm đã được xóa thành công";
        return RedirectToAction("Wishlist", "Home");
    }



    // Trang so sánh
    public async Task<IActionResult> Compare()
    {
        // L?y danh sách sản phẩm trong compare
        var compare_products = await (from c in _dataContext.Compares
                                      join p in _dataContext.Products on c.ProductId equals p.Id
                                      join u in _dataContext.Users on c.UserId equals u.Id
                                      select new { ProductId = p.Id,User = u, Product = p, Compares = c })
                                      .ToListAsync();
        return View(compare_products);
    }
    [HttpPost]
    public async Task<IActionResult> AddCompare(long Id)
    {
        var user = await _userManager.GetUserAsync(User); //Lấy người dùng hiện tại
        
        // Kiểm tra xem sản phẩm đã có trong danh sách so sánh chưa
        bool exists = await _dataContext.Compares
            .AnyAsync(c => c.ProductId == Id && c.UserId == user.Id);
        if (exists)
        {
            return Json(new { success = false, message = "Bạn đã thêm sản phẩm vào so sánh trước đó" });
        }

        //Tạo một đối tượng wishlist mới
        var compareProduct = new CompareModel
        {
            ProductId = Id,
            UserId = user.Id
        };

        _dataContext.Compares.Add(compareProduct);
        try
        {
            await _dataContext.SaveChangesAsync();
            return Json(new { success = true, message = "Thêm so sánh thành công!" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Đã lỗi khi thêm sản phẩm vào so sánh");
        }
    }
    public async Task<IActionResult> DeleteCompare(int Id)
    {
        CompareModel compare = await _dataContext.Compares.FindAsync(Id);

        _dataContext.Compares.Remove(compare);

        await _dataContext.SaveChangesAsync();
        TempData["success"] = "Sản phẩm đã được xóa thành công";
        return RedirectToAction("Compare", "Home");
    }

    public IActionResult Privacy()
    {
        return View();
    }


    // Trang 404

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(int statuscode)
    {
        if (statuscode == 404)
        {
            return View("NotFound");
        }
        else
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
