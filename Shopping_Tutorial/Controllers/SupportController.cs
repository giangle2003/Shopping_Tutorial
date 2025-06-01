using Microsoft.AspNetCore.Mvc;

namespace Shopping_Tutorial.Controllers
{
    public class SupportController : Controller
    {
        public IActionResult Introduce()
        {
            return View();
        }
        public IActionResult SupportBuyProduct()
        {
            return View();
        }
        public IActionResult TermsofUse()
        {
            return View();
        }
        public IActionResult ShippingPolicy()
        {
            return View();
        }
        public IActionResult GuaranteePolicy()
        {
            return View();
        }
    }
}
