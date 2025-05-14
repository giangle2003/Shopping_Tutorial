using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Shopping_Tutorial.Repository.Components
{
    public class BrandsViewComponent : ViewComponent
    {
        private readonly DataContext _dataContext;

            public BrandsViewComponent(DataContext context)
        {
            _dataContext = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var activeBrands = await _dataContext.Brands
                .Where(b => b.Status == 1)
                .ToListAsync();

            return View(activeBrands);
        }


    }
}
