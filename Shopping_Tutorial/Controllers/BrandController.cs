using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Repository;
using System.Globalization;

namespace Shopping_Tutorial.Controllers
{
    public class BrandController : Controller
    {
        private readonly DataContext _dataContext;

        public BrandController(DataContext context)
        {
            _dataContext = context;
        }
        public async Task<IActionResult> Index(string Slug = "", string sort_by = "", string[] price_filter = null, string stock_status = "", int pg= 1)
        {
            int pageSize = 9;//số sản phẩm trên mỗi trang

            var brand = _dataContext.Brands.FirstOrDefault(c => c.Slug == Slug);

            if (brand == null)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Slug = Slug;
            ViewBag.SelectedPrices = price_filter?.ToArray();
            ViewBag.sort_key = sort_by;

            IQueryable<ProductModel> productsQuery = _dataContext.Products
                                .Where(p => p.BrandId == brand.Id);
            int count = await productsQuery.CountAsync();
            ViewBag.Count = count;


            // ✅ Lọc theo tồn kho
            if (stock_status == "in_stock")
            {
                productsQuery = productsQuery.Where(p => p.Quantity > 0);
            }
            else if (stock_status == "out_of_stock")
            {
                productsQuery = productsQuery.Where(p => p.Quantity == 0);
            }


            // Lọc theo giá
            if (price_filter != null && price_filter.Any())
            {
                IQueryable<ProductModel> priceFilteredQuery = productsQuery
       .Where(p => false); // để dễ kết hợp OR

                foreach (var range in price_filter)
                {
                    if (range == "0-1000")
                        priceFilteredQuery = priceFilteredQuery.Union(productsQuery.Where(p => p.Price >= 0 && p.Price <= 10000000));
                    else if (range == "1000-1500")
                        priceFilteredQuery = priceFilteredQuery.Union(productsQuery.Where(p => p.Price > 10000000 && p.Price <= 15000000));
                    else if (range == "1500-2000")
                        priceFilteredQuery = priceFilteredQuery.Union(productsQuery.Where(p => p.Price > 15000000 && p.Price <= 20000000));
                    else if (range == "2000-2500")
                        priceFilteredQuery = priceFilteredQuery.Union(productsQuery.Where(p => p.Price > 20000000 && p.Price <= 25000000));
                    else if (range == "2500+")
                        priceFilteredQuery = priceFilteredQuery.Union(productsQuery.Where(p => p.Price > 25000000));
                }

                productsQuery = priceFilteredQuery.Distinct();
            }

            // Sắp xếp
            if (count > 0)
            {
                // ✅ Sắp xếp sau khi lọc
                if (!string.IsNullOrEmpty(sort_by))
                {
                    if (sort_by == "price_increase")
                        productsQuery = productsQuery.OrderBy(p => p.Price);
                    else if (sort_by == "price_decrease")
                        productsQuery = productsQuery.OrderByDescending(p => p.Price);
                    else if (sort_by == "price_newest")
                        productsQuery = productsQuery.OrderByDescending(p => p.Id);
                    else if (sort_by == "price_oldest")
                        productsQuery = productsQuery.OrderBy(p => p.Id);
                }
            }

            int totalItems = await productsQuery.CountAsync();
            var pager = new Paginate(totalItems, pg, pageSize);
            ViewBag.Pager = pager;


            var productList = await productsQuery
                             .Skip((pg - 1) * pageSize)
                             .Take(pageSize)
                             .ToListAsync();

            return View(productList);
        }
    }
}
