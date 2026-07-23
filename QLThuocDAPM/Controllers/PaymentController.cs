using Microsoft.AspNetCore.Mvc;
using QLThuocDAPM.Models.VnPay;
using QLThuocDAPM.Services.VnPay;

namespace QLThuocDAPM.Controllers
{
    public class PaymentController : Controller
    {

        private readonly IVnPayService _vnPayService;

        public PaymentController(IVnPayService vnPayService)
        {

            _vnPayService = vnPayService;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult CreatePaymentUrlVnpay(PaymentInformationModel model, string sdtnguoinhan, string address, string tennguoinhan)
        {

            HttpContext.Session.SetString("tennguoinhan", tennguoinhan);
            HttpContext.Session.SetString("sdtnguoinhan", sdtnguoinhan);
            HttpContext.Session.SetString("address", address);
            var url = _vnPayService.CreatePaymentUrl(model, HttpContext);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

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
