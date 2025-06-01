using Microsoft.AspNetCore.Mvc;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Models.Vnpay;
using Shopping_Tutorial.Services.Momo;
using Shopping_Tutorial.Services.Vnpay;

namespace Shopping_Tutorial.Controllers
{
    public class PaymentController : Controller
    {
        private IMomoService _momoService;
        private readonly IVnPayService _vnPayService;

        public PaymentController(IMomoService momoService, IVnPayService vnPayService)
        {
            _momoService = momoService;
            _vnPayService = vnPayService;
        }
        [HttpPost]
        public async Task<IActionResult> CreatePaymentMomo(OrderInfor model, string fullAddress, string phoneNumber)
        {
            Response.Cookies.Append("FullAddress", fullAddress, new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddMinutes(30)
            });
            Response.Cookies.Append("PhoneNumber", phoneNumber, new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddMinutes(30)
            });
            var response = await _momoService.CreatePaymentAsync(model);
            //return Json(response);
            return Redirect(response.PayUrl);
        }

        [HttpGet]
        public IActionResult PaymentCallBack()
        {
            var response = _momoService.PaymentExecuteAsync(HttpContext.Request.Query);
            return View(response);
        }
        public IActionResult CreatePaymentUrlVnpay(PaymentInformationModel model, string fullAddress, string phoneNumber)
        {
            Response.Cookies.Append("FullAddress", fullAddress, new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddMinutes(30)
            });
            Response.Cookies.Append("PhoneNumber", phoneNumber, new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddMinutes(30)
            });
            var url = _vnPayService.CreatePaymentUrl(model, HttpContext);

            return Redirect(url);
        }
        [HttpGet]
        public IActionResult PaymentCallbackVnpay()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            return Json(response);
        }

    }
}
