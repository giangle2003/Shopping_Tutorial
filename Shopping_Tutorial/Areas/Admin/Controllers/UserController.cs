using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Repository;
using System.Numerics;
using System.Security.Claims;

namespace Shopping_Tutorial.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/User")]
    [Authorize(Roles ="Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<AppUserModel> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly DataContext _dataContext;

        public UserController(DataContext context,UserManager<AppUserModel> userManager,RoleManager<IdentityRole> roleManager )
        {
            _dataContext = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        [Route("Index")]
        public async Task<IActionResult> Index(int pg = 1)
        {          
            var usersWithRoles = await (from u in _dataContext.Users 
                                        join ur in _dataContext.UserRoles on u.Id equals ur.UserId 
                                        join r in _dataContext.Roles on ur.RoleId equals r.Id 
                                        select new { User = u, RoleName = r.Name })
                                        .ToListAsync();
            return View(usersWithRoles);
        }


        [HttpGet]
        [Route("Create")]
        public async Task<IActionResult> Create()
        {
            var roles = await _roleManager.Roles.ToListAsync(); //Lấy danh sách role
            ViewBag.Roles = new SelectList(roles, "Id", "Name");
            return View(new AppUserModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Create")]
        public async Task<IActionResult> Create(AppUserModel user)
        {
            if(ModelState.IsValid)
            {
                var createUserResult = await _userManager.CreateAsync(user, user.PasswordHash); //Tạo user
                
                if (createUserResult.Succeeded) 
                {
                    var createUser = await _userManager.FindByEmailAsync(user.Email);//Tìm user theo email
                    var userId = createUser.Id; //Lấy id của user
                    var role = await _roleManager.FindByIdAsync(user.RoleId); //Tìm role theo id
                    if(role != null) //Kiểm tra role có tồn tại không
                    {
                        //gán quyền cho user
                        var addToRoleResult = await _userManager.AddToRoleAsync(createUser, role.Name);
                        if(!addToRoleResult.Succeeded)
                        {
                            foreach (var error in createUserResult.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                            return View(user);
                        }
                        return RedirectToAction("Index", "User");
                    }
                   
                }
                else
                {
                    foreach (var error in createUserResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(user);
                }
            }
            else
            {
                TempData["errorr"] = "Có lỗi xảy ra khi thêm mới sản phẩm";
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
            var roles = await _roleManager.Roles.ToListAsync(); //Lấy danh sách role
            ViewBag.Roles = new SelectList(roles, "Id", "Name");
            return View(user);
        }


        [HttpGet]
        [Route("Edit")]
        public async Task<IActionResult> Edit(string id)
        {
            if(string.IsNullOrEmpty(id)) //Kiểm tra id có rỗng không
            {
                return NotFound();
            }
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) //Kiểm tra user có tồn tại không
            {
                return NotFound();
            }
            var roles = await _roleManager.Roles.ToListAsync(); //Lấy danh sách role
            ViewBag.Roles = new SelectList(roles, "Id", "Name");
            return View(user);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Edit")]
        public async Task<IActionResult> Edit(string id, AppUserModel user)
        {
            var existingUser = await _userManager.FindByIdAsync(id); //Tìm user theo id

            if (existingUser == null) //Kiểm tra user có tồn tại không
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                //Cập nhật thông tin user
                existingUser.UserName = user.UserName;
                existingUser.Email = user.Email;
                existingUser.PhoneNumber = user.PhoneNumber;

                // Kiểm tra nếu RoleId thay đổi
                if (existingUser.RoleId != user.RoleId)
                {
                    var currentRoles = await _userManager.GetRolesAsync(existingUser); // Lấy danh sách role hiện tại của user
                    if (currentRoles.Any())
                    {
                        await _userManager.RemoveFromRolesAsync(existingUser, currentRoles); // Xóa tất cả role cũ của user
                    }

                    var newRole = await _roleManager.FindByIdAsync(user.RoleId); // Tìm role mới theo RoleId
                    if (newRole != null)
                    {
                        await _userManager.AddToRoleAsync(existingUser, newRole.Name); // Thêm role mới vào user
                    }

                    existingUser.RoleId = user.RoleId; // Cập nhật RoleId của user
                }


                var updateUserResult = await _userManager.UpdateAsync(existingUser); //Cập nhật user
                if (updateUserResult.Succeeded)
                {
                    return RedirectToAction("Index","User");
                }
                else
                {
                    foreach (var error in updateUserResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(existingUser);
                }
            }
            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");

            //Nếu thông tin không hợp lệ
            TempData["errorr"] = "Có lỗi xảy ra khi cập nhật user";
            var errors = ModelState.Values.SelectMany(p => p.Errors).Select(p => p.ErrorMessage).ToList();
            string errorMessage = string.Join("\n", errors);

            return View(existingUser);
        }

        [HttpGet]
        [Route("Delete")]
        public async Task<IActionResult> Delete(string id)
        {
            if(string.IsNullOrEmpty(id)) //Kiểm tra id có rỗng không
            {
                return NotFound();
            }
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) //Kiểm tra user có tồn tại không
            {
                return NotFound();
            }
            var deleteResult = await _userManager.DeleteAsync(user); //Xóa user
            if(!deleteResult.Succeeded)
            {
                return View("Errorr");
            }
            TempData["success"] = "Xóa user thành công";
            return RedirectToAction("Index");
        }

    }
}
