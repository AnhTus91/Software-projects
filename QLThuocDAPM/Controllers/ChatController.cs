using Microsoft.AspNetCore.Mvc;

namespace QLThuocDAPM.Controllers
{
    public class ChatController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
