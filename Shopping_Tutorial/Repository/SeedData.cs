using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Models;

namespace Shopping_Tutorial.Repository
{
    public class SeedData
    {
        public static void SeedingData(DataContext _context)
        {
            _context.Database.Migrate();
            if (!_context.Products.Any())
            {
                CategoryModel macbook = new CategoryModel { Name = "Macbook", Slug = "Macbook", Description = "Macbook is langre Brand in the world", Status = 1 };
                CategoryModel pc = new CategoryModel { Name = "Pc", Slug = "Pc", Description = "Pc is langre Brand in the world", Status = 1 };
                BrandModel apple = new BrandModel { Name = "Apple", Slug = "apple", Description = "Apple is langre Brand in the world", Status = 1 };
                BrandModel samsung = new BrandModel { Name = "Samsung", Slug = "samsung", Description = "Samsung is langre Brand in the world", Status = 1 };

                _context.Products.AddRange(
                    new ProductModel { Name = "Macbook", Slug = "Macbook", Description = "Macbook is the best", Price = 1000, Category = macbook, Image = "1.jpg", Brand = apple },
                    new ProductModel { Name = "Pc", Slug = "Pc", Description = "Pc is the best", Price = 1000, Category = pc, Image = "1.jpg", Brand = samsung }
                );
            }
            
            _context.SaveChanges();
        }

    }
}
