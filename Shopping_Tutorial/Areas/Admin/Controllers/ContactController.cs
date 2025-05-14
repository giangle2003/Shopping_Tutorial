using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Repository;

namespace Shopping_Tutorial.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Contact")]
    [Authorize(Roles = "Admin")]
    public class ContactController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IWebHostEnvironment _webHostEnviroment;
        public ContactController(DataContext context, IWebHostEnvironment webHostEnviroment)
        {
            _dataContext = context;
            _webHostEnviroment = webHostEnviroment;
        }

        [Route("Index")]
        public IActionResult Index()
        {
            var contact = _dataContext.Contact.ToList();

            return View(contact);
        }

        [Route("Edit")]
        public async Task<IActionResult> Edit()
        {
            ContactModel contact = await _dataContext.Contact.FirstOrDefaultAsync();
            ViewBag.CurrentImageName = contact.LogoImg;

            return View(contact);
        }

        [Route("Edit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ContactModel contact)
        {
            var contact_existed = _dataContext.Contact.FirstOrDefault(); //tim contact dau tien
            if (ModelState.IsValid)
            {
                if (contact.ImageUpload != null)
                {
                    string uploadDir = Path.Combine(_webHostEnviroment.WebRootPath, "media/logo");
                    string imageName = Guid.NewGuid().ToString() + "-" + contact.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadDir, imageName);

                    // Xóa ảnh cũ nếu có
                    if (!string.IsNullOrEmpty(contact_existed.LogoImg))
                    {
                        string oldImagePath = Path.Combine(uploadDir, contact_existed.LogoImg);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath); // Xóa ảnh cũ
                        }
                    }
                    // Lưu ảnh mới
                    using (FileStream fs = new FileStream(filePath, FileMode.Create))
                    {
                        await contact.ImageUpload.CopyToAsync(fs);
                    }
                    contact_existed.LogoImg = imageName;
                }

                // Cập nhật thông tin
                contact_existed.Name = contact.Name;
                contact_existed.Email = contact.Email;
                contact_existed.Description = contact.Description;
                contact_existed.Phone = contact.Phone;
                contact_existed.Map = contact.Map;

                _dataContext.Update(contact_existed);
                await _dataContext.SaveChangesAsync();

                // Cập nhật ViewBag để hiển thị ảnh mới
                ViewBag.CurrentImageName = contact_existed.LogoImg;

                TempData["success"] = "Cập nhật thông tin liên hệ thành công";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["errorr"] = "Có lỗi xảy ra khi sửa thông tin liên hệ";
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
            return View(contact);
        }

    }
}
