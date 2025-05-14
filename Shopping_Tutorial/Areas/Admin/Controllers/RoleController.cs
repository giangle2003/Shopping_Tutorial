using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Repository;
using System.Threading.Tasks;

namespace Shopping_Tutorial.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Role")]
    [Authorize(Roles = "Admin")]
    public class RoleController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleController(DataContext context, RoleManager<IdentityRole> roleManager)
        {
            _dataContext = context;
            _roleManager = roleManager;
        }

        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            return View(await _dataContext.Roles.OrderBy(p=>p.Id).ToListAsync());
        }
        [HttpGet]
        [Route("Create")]
        public async Task<IActionResult> Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Create")]
        public async Task<IActionResult> Create(IdentityRole model)
        {
            //nếu ko tồn tại role thì tạo mới
            if (!_roleManager.RoleExistsAsync(model.Name).Result) 
            {
                _roleManager.CreateAsync(new IdentityRole(model.Name)).GetAwaiter().GetResult();
            }
            TempData["Success"] = "Tạo mới quyền thành công";
            return Redirect("Index");
        }

        [HttpGet]
        [Route("Edit")]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            var role = await _roleManager.FindByIdAsync(id);
            return View(role);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Edit")]
        public async Task<IActionResult> Edit(string id,IdentityRole model)
        {
            if(string.IsNullOrEmpty(id))
            {
                return NotFound(); //nếu id rỗng thì trả về trang 404
            }

            if (ModelState.IsValid)//nếu model hợp lệ
            {
                var role = await _roleManager.FindByIdAsync(model.Id); //tìm role theo id
                if(role == null) {
                    return NotFound(); //nếu ko tìm thấy role thì trả về trang 404
                }

                role.Name = model.Name; //cập nhật tên role
                try
                {
                    await _roleManager.UpdateAsync(role); //cập nhật role
                    TempData["Success"] = "Cập nhật quyền thành công";
                    return RedirectToAction("Index");
                }
                catch(Exception)
                {
                    ModelState.AddModelError("", "Một số lỗi xảy ra khi thực hiện cập nhật");
                }
            }
            return View(model ?? new IdentityRole { Id = id}); //nếu model ko hợp lệ thì trả về view với model đã nhập
        }



        [HttpGet]
        [Route("Delete")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound(); 
            }

            var role = await _roleManager.FindByIdAsync(id);

            if (role == null)
            {
                return NotFound(); 
            }

            try
            {
                await _roleManager.DeleteAsync(role);
                TempData["success"] = "Xóa quyền thành công";
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Một số lỗi xảy ra khi thực hiện xóa");
            }

            return Redirect("Index");
        }

    }
}
