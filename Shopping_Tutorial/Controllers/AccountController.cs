using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Shopping_Tutorial.Areas.Admin.Repository;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Models.ViewModels;
using Shopping_Tutorial.Repository;
using System.Security.Claims;
using Shopping_Tutorial.Services.Cart;
using System.Globalization;

namespace Shopping_Tutorial.Controllers
{
    public class AccountController : Controller
    {
        private UserManager<AppUserModel> _userManage; //Quản lý user
        private SignInManager<AppUserModel> _signInManager; //Quản lý đăng nhập
        private readonly IEmailSender _emailSender; //Gửi email
        private readonly DataContext _dataContext;
        private readonly ICompositeViewEngine _viewEngine; //Render view to string
        private readonly ITempDataProvider _tempDataProvider; //Render view to string
        private readonly ICartService _cartService;


        public AccountController(ICartService cartService, UserManager<AppUserModel> userManager, SignInManager<AppUserModel> signInManager, DataContext dataContext, IEmailSender emailSender, ICompositeViewEngine viewEngine, ITempDataProvider tempDataProvider)
        {
            _cartService = cartService;
            _userManage = userManager;
            _signInManager = signInManager;
            _dataContext = dataContext;
            _emailSender = emailSender;
            _viewEngine = viewEngine;
            _tempDataProvider = tempDataProvider;

        }

        public IActionResult Login(string returnUrl)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl});
        }

        //Đăng nhập
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel loginVM)
        {
            if (ModelState.IsValid)
            {
                Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(loginVM.Username, loginVM.Password, false, false);
                if (result.Succeeded)
                {
                    await _cartService.MergeCartFromCookieToDb(HttpContext);
                    return Redirect(loginVM.ReturnUrl ?? "/");
                }
                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
            }
                return View(loginVM);
        }


        public async Task<IActionResult> History(int? status,DateTime? fromDate, DateTime? toDate)
        {
            //Kiểm tra nếu chưa đăng nhập quay về trang đăng nhập
            if ((bool)!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            //Lấy id của người dùng đang đăng nhập
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            //Lấy email của người dùng đang đăng nhập
            var userEmail = User.FindFirstValue(ClaimTypes.Email);



            //Lấy danh sách đơn hàng của người dùng
            var orders = _dataContext.Orders
                .Where(o => o.UserName == userEmail)
                .OrderByDescending(o => o.Id)
                .AsQueryable();
            //Lọc theo trạng thái
            if (status.HasValue)
            {
                orders = orders.Where(o => o.Status == status.Value);
                ViewBag.Filter = status;
            }

            //Lọc theo ngày mua
            if (fromDate.HasValue)
            {
                orders = orders.Where(o => o.CreatedDate >= fromDate.Value);
                ViewBag.FromDate = fromDate.Value.ToString("yyyy-MM-dd");
            }

            if (toDate.HasValue)
            {
                var endOfDay = toDate.Value.Date.AddDays(1).AddSeconds(-1);
                orders = orders.Where(o => o.CreatedDate <= endOfDay);
                ViewBag.ToDate = toDate.Value.ToString("yyyy-MM-dd");
            }

            var ordersQuery = await orders.ToListAsync();


            ViewBag.UserEmail = userEmail;
            

            return View(ordersQuery);
        }

        //Hủy đơn hàng
        public async Task<IActionResult> CancelOrder(string ordercode)
        {
            //Kiểm tra nếu chưa đăng nhập quay về trang đăng nhập
            if ((bool)!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            try
            {
                //Lấy đơn hàng theo id
                var order = await _dataContext.Orders.Where(o => o.OrderCode == ordercode).FirstAsync();
                
                    order.Status = 3; //Đơn hàng đã hủy
                     _dataContext.Update(order);
                    await _dataContext.SaveChangesAsync();
            }
                catch (Exception ex)
            {
                return BadRequest("An error occurred while canceling the order.");
            }
            
            return RedirectToAction("History","Account", new {status = 3});
        }
      
        //Đã nhận
        public async Task<IActionResult> ReceiveOrder(string ordercode)
        {
            //Kiểm tra nếu chưa đăng nhập quay về trang đăng nhập
            if ((bool)!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            try
            {
                //Lấy đơn hàng theo id
                var order = await _dataContext.Orders.Where(o => o.OrderCode == ordercode).FirstAsync();

                order.Status = 4; //Đơn hàng đã nhận
                _dataContext.Update(order);
                await _dataContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest("An error occurred while canceling the order.");
            }

            return RedirectToAction("History", "Account", new { status = 4 });
        }

        //Nhận hàng
        public async Task<IActionResult> DetailOrder(string ordercode)
        {
            //Kiểm tra nếu chưa đăng nhập quay về trang đăng nhập
            if ((bool)!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            // Lấy đơn hàng từ cơ sở dữ liệu
            var order = await _dataContext.Orders.Where(o => o.OrderCode == ordercode).FirstAsync();

            return View(order);
        }
        //Chi tiết đơn hàng
        public async Task<IActionResult> ProductDetails(string ordercode)
        {
            var DetailsOrder = await _dataContext.OrderDetails.
                                AsNoTracking()
                                .Include(od => od.Product) // Join với bảng Product
                                .Where(o => o.OrderCode == ordercode)
                                .OrderByDescending(o => o.OrderDate)  // Sắp xếp giảm dần theo OrderDate
                                .ToListAsync();
            if (DetailsOrder == null || !DetailsOrder.Any())
            {
                TempData["errorr"] = "Không tìm thấy chi tiết đơn hàng!";
                return RedirectToAction("Index", "Order");  // Hoặc chuyển hướng về trang đơn hàng
            }
            // Lấy trạng thái đơn hàng từ bảng Orders
            var orderStatus = await _dataContext.Orders
                                 .Where(o => o.OrderCode == ordercode)
                                 .Select(o => o.Status)
                                 .FirstOrDefaultAsync();

            //Lấy phí shipping từ bảng Orders
            var ShippingCost = _dataContext.Orders
                .Where(o => o.OrderCode == ordercode)
                .Select(o => o.ShippingCost)
                .FirstOrDefault();
            var Discount = _dataContext.Orders
                .Where(o => o.OrderCode == ordercode)
                .Select(o => o.Discount)
                .FirstOrDefault();
            ViewBag.OrderStatus = orderStatus; // Truyền trạng thái sang View
            ViewBag.ShippingCost = ShippingCost; // Truyền phí shipping sang View
            ViewBag.Discount = Discount; // Truyền giảm giá sang View
            return View(DetailsOrder);
        }

        //Quên mật khẩu
        public async Task<IActionResult> ForgotPass()
        {
            return View();
        }

        //Gửi email quên mật khẩu
        [HttpPost]
        public async Task<IActionResult> SendMailForgotPass(AppUserModel user)
        {
            //Kiểm tra trong database có email này không
            var checkMail = await _userManage.Users.FirstOrDefaultAsync(u => u.Email == user.Email);

            if (checkMail == null)
            {
                ModelState.AddModelError("Email", "Email chưa được đăng ký hoặc không tồn tại.Vui lòng nhập lại.");
                return View("ForgotPass",user);
            }
            else
            {
                //Tạo token mới
                string token = Guid.NewGuid().ToString();
                //update token vào data của user
                checkMail.Token = token;
                _dataContext.Update(checkMail);
                await _dataContext.SaveChangesAsync();

                // Đọc file HTML template
                string message = await RenderViewToStringAsync("ResetPassEmail", checkMail); // Đường dẫn đến template email
                string resetLink = $"{Request.Scheme}://{Request.Host}/Account/NewPass?email={checkMail.Email}&token={token}";
                string linkHtml = $"<a href=\"{resetLink}\">Click here to reset your password</a>";

                message = message.Replace("{{ResetLinkHtml}}", linkHtml);
                message = message.Replace("@Model.Username", checkMail.UserName);  // Thay thế Username bằng tên người dùng thực tế


                //Gửi email
                var receiver = checkMail.Email;
                var subject = "Thay đổi mật khẩu " + checkMail.Email;
              //  var message = "Click vào đường link để thay đổi mật khẩu " +
              //$"<a href='{Request.Scheme}://{Request.Host}/Account/NewPass?email={checkMail.Email}&token={token}'>Click here to reset your password</a>";

                await _emailSender.SendEmailAsync(receiver, subject, message);
            }

            TempData["success"] = "Một email đã được gửi đến địa chỉ email đã đăng ký của bạn kèm theo hướng dẫn đặt lại mật khẩu.";
            return RedirectToAction("Notification", "Account", new { email = user.Email });
        }

        //Thông báo kiểm tra mật khẩu
        public async Task<IActionResult> Notification(string email)
        {
            var model = new AppUserModel
            {
                Email = email
            };
            return View(model);
        }

        //Mật khẩu mới
        public async Task<IActionResult> NewPass(AppUserModel user, string token)
        {

            //Check xem có user phù hợp với email và token không
            var checkuser = await _userManage.Users
                .Where(u => u.Email == user.Email)
                .Where(u => u.Token == user.Token).FirstOrDefaultAsync();
            if (checkuser != null)
            {
                ViewBag.Email = checkuser.Email;
                ViewBag.Token = token;
            }
            else
            {
                TempData["error"] = "Email hoặc Token không đúng.Vui lòng kiểm tra lại";
                return RedirectToAction("ForgotPass", "Account");
            }
            return View();
        }
        //Cập nhât mật khẩu mới
        public async Task<IActionResult> UpdateNewPass(AppUserModel user, string token)
        {
            if(user.PasswordHash != user.ConfirmPassword)
            {
                TempData["errorr"] = "Mật khẩu không khớp.Vui lòng nhập lại.";
                return RedirectToAction("NewPass", "Account", new { email = user.Email, token = token });
            }

            //Check xem có user phù hợp với email và token không
            var checkuser = await _userManage.Users
                .Where(u => u.Email == user.Email)
                .Where(u => u.Token == user.Token).FirstOrDefaultAsync();
            if (checkuser != null)
            {
                //Update user với mật khẩu mới và token mới
                string newToken = Guid.NewGuid().ToString(); //Tạo token mới
                //Mã hóa mật khẩu mới
                var passwordHasher = new PasswordHasher<AppUserModel>();
                var passwordHash = passwordHasher.HashPassword(checkuser, user.PasswordHash);

                checkuser.PasswordHash = passwordHash; //Cập nhật mật khẩu mới
                checkuser.Token = newToken; //Cập nhật token mới

                await _userManage.UpdateAsync(checkuser);
                //TempData["success"] = "Cập nhật mật khẩu thành công.";
                return RedirectToAction("PassChangeSuccess", "Account");
            }
            else
            {
                TempData["error"] = "Email hoặc Token không đúng.Vui lòng kiểm tra lại";
                return RedirectToAction("ForgotPass", "Account");
            }
                return View();
        }

        //Thông báo đổi mật khẩu thành công
        public IActionResult PassChangeSuccess()
        {
            return View();
        }

        //Update Tài khoản
        public async Task<IActionResult> UpdateAccount( )
        {
            //Kiểm tra nếu chưa đăng nhập quay về trang đăng nhập
            if ((bool)!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            //Lấy id của người dùng đang đăng nhập
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            //Lấy email của người dùng đang đăng nhập
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            var user = await _userManage.Users
                .FirstOrDefaultAsync(u => u.Email == userEmail);
            if (user == null)
            {
                return NotFound();
            }
            
            return View(user);
        }

        //Câp nhật thông tin cá nhân
        [HttpPost]
        public async Task<IActionResult> UpdateInfoAccount(AppUserModel appUser, string OldPass, string NewPass, string ConfirmNewPass)
        {
            //Lấy id của người dùng đang đăng nhập
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var userById = await _userManage.Users
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (userById == null)
            {
                return NotFound();
            }
            else
            {
                userById.PhoneNumber = appUser.PhoneNumber;
                //Nếu người dùng đổi mật khẩu
                if(!string.IsNullOrEmpty(OldPass) && !string.IsNullOrEmpty(NewPass))
                {
                    if(NewPass != ConfirmNewPass)
                    {
                        ModelState.AddModelError("", "Mật khẩu mới và xác nhận không trùng khớp.");
                        return View("UpdateAccount", userById);
                    }
                    var checkOldPass = await _userManage.CheckPasswordAsync(userById, OldPass);
                    if(!checkOldPass)
                    {
                        ModelState.AddModelError("", "Mật khẩu cũ không đúng.Vui lòng nhập lại.");
                        return View("UpdateAccount", userById);
                    }
                    var result = await _userManage.ChangePasswordAsync(userById, OldPass, NewPass);
                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                        return View("UpdateAccount", userById);
                    }

                }

                await _userManage.UpdateAsync(userById);
                TempData["success"] = "Cập nhật thông tin thành công.";
            }

            return RedirectToAction("UpdateAccount", "Account");
        }



        //Đăng kí
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserModel user)
        {
            if(ModelState.IsValid)
            {
                AppUserModel newUser = new AppUserModel
                {
                    UserName = user.Username,
                    Email = user.Email
                };
                IdentityResult result = await _userManage.CreateAsync(newUser,user.Password);
                if(result.Succeeded)
                {
                    //Thêm quyền mặc định là Customer
                    await _userManage.AddToRoleAsync(newUser, "Customer");

                    TempData["success"] = "Đăng ký thành công";
                    return Redirect("/account/login");
                }
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description); // Hiển thị lỗi từ Identity
                }

            }
            return View(user);
        }

        //Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(); //Đăng xuất khỏi Google or Fb
            await _signInManager.SignOutAsync(); //Đăng xuất tài khoản thường
            return RedirectToAction("Index", "Home");
        }

        //Để chuyển view thành string
        private async Task<string> RenderViewToStringAsync(string viewName, object model)
        {
            var httpContext = new DefaultHttpContext { RequestServices = HttpContext.RequestServices };
            var actionContext = new ActionContext(httpContext, RouteData, ControllerContext.ActionDescriptor);

            using var sw = new StringWriter();
            var viewResult = _viewEngine.FindView(actionContext, viewName, false);

            if (viewResult.View == null)
                throw new ArgumentNullException($"{viewName} không tồn tại.");

            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = model
            };

            var tempData = new TempDataDictionary(actionContext.HttpContext, _tempDataProvider);
            var viewContext = new ViewContext(
                actionContext,
                viewResult.View,
                viewData,
                tempData,
                sw,
                new HtmlHelperOptions()
            );

            await viewResult.View.RenderAsync(viewContext);
            return sw.ToString();
        }


        //Đăng nhập bằng tài khoản google
        public async Task LoginByGoogle()
        {
            // Dùng Google để xác thực
            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme,
                new AuthenticationProperties
                {
                    RedirectUri = Url.Action("GoogleResponse")
                });
        }

        //Nhận phản hồi từ Google
        public async Task<IActionResult> GoogleResponse()
        {
            // Mã xác thực từ Google
            var result = await HttpContext
                .AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

            if (!result.Succeeded)
            {
                //Nếu xác thực ko thành công quay về trang Login
                return RedirectToAction("Login");
            }

            //Lấy thông tin từ Google
            var claims = result.Principal.Identities.FirstOrDefault().Claims.Select(claim => new
            {
                claim.Issuer,
                claim.OriginalIssuer,
                claim.Type,
                claim.Value
            });

            //Lấy email từ claims
            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            string UserName = email.Split('@')[0]; //Lấy đối tượng email trước dấu "@"
            
            // Check user có tồn tại không
            var existingUser = await _userManage.FindByEmailAsync(email);


            if (existingUser == null)
            {
                //nếu user ko tồn tại trong db thì tạo user mới với password hashed mặc định 1-9
                var passwordHasher = new PasswordHasher<AppUserModel>();
                var hashedPassword = passwordHasher.HashPassword(null, "123456789");

                //Tạo user mới
                var newUser = new AppUserModel { UserName = UserName, Email = email };
                newUser.PasswordHash = hashedPassword; // gán password đã mã hóa cho user

                var createUserResult = await _userManage.CreateAsync(newUser);
                
                if (!createUserResult.Succeeded)
                {
                    TempData["errorr"] = "Đăng ký tài khoản thất bại. Vui lòng thử lại sau.";
                    return RedirectToAction("Login", "Account"); // Trả về trang đăng ký nếu fail

                }
                else
                {
                    // Nếu user tạo user thành công thì đăng nhập luôn 
                    await _signInManager.SignInAsync(newUser, isPersistent: false);
                    TempData["success"] = "Đăng ký tài khoản thành công.";
                    return RedirectToAction("Index", "Home");
                }

            }
            else
            {
                //Còn user đã tồn tại thì đăng nhập luôn với existingUser
                await _signInManager.SignInAsync(existingUser, isPersistent: false);
            }
            return RedirectToAction("Home","Account");

        }
    }

}
