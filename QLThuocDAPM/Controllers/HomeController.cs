using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using QLThuocDAPM.Data;
using QLThuocDAPM.Models;
using QLThuocDAPM.ViewModels;
using System.Diagnostics;
using System.Drawing.Printing;
using iTextSharp.text;
using iTextSharp.text.pdf;


namespace QLThuocDAPM.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly QlthuocDapm6Context _context;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, QlthuocDapm6Context context, IConfiguration configuration)
        {
            _logger = logger;
            _context = context;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            var danhMucList = _context.DanhMucs.ToList(); // Lấy danh sách danh mục
            var danhSachSanPham = _context.SanPhams
                .Include(sp => sp.MaGiamGiaNavigation) // Load thông tin giảm giá
                .Take(8)
                .ToList();
            var danhsachbenh = _context.Benhs.Take(4).ToList(); // Lấy 4 sản phẩm mới nhất



            var viewModel = new HomeViewModel
            {
                DanhMucs = danhMucList,
                SanPhams = new Dictionary<int, List<SanPham>>(),
                NhaCungCap = _context.NhaCungCaps.ToList(),

                SanPhamNoiBat = danhSachSanPham,
                benhNoiBat = danhsachbenh,

            };

            // Lặp qua từng danh mục và lấy sản phẩm theo mã danh mục
            foreach (var danhMuc in danhMucList)
            {
                var sanPhamTheoDanhMuc = _context.SanPhams
                    .Where(sp => sp.MaDm == danhMuc.MaDm) // Lọc sản phẩm theo mã danh mục
                    .ToList();

                viewModel.SanPhams[danhMuc.MaDm] = sanPhamTheoDanhMuc; // Thêm sản phẩm vào từ điển
            }


            return View(viewModel);
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public JsonResult GetData()
        {
            int cartCount = 0;

            var shoppingCart = HttpContext.Session.GetString("ShoppingCart");
            if (shoppingCart != null)
            {
                var ShoppingCart = JsonConvert.DeserializeObject<List<CartItem>>(shoppingCart);
                cartCount = ShoppingCart.Count();
            }

            return Json(new { cartCount = cartCount });
        }


        //public IActionResult DonHang()
        //{
        //    if (HttpContext.Session.GetString("userLogin") == null)
        //    {
        //        return RedirectToAction("Login", "User");
        //    }
        //    else
        //    {
        //        string userID = HttpContext.Session.GetString("userLogin");

        //        var donhang = _context.DonHangs
        //                             .Where(dh => dh.Username == userID)
        //                             .Include(dh => dh.ChiTietDonHangs)
        //                             .ThenInclude(ct => ct.MaSpNavigation) // Load sản phẩm liên quan
        //                             .OrderByDescending(dh => dh.TrangThai)
        //                             .ToList();

        //        return View(donhang);
        //    }
        //}

        public IActionResult DonHang()
        {
            if (HttpContext.Session.GetString("userLogin") == null)
            {
                return RedirectToAction("Login", "User");
            }

            string userID = HttpContext.Session.GetString("userLogin");
            var donhang = _context.DonHangs
                .Include(dh => dh.ChiTietDonHangs)
                .ThenInclude(ct => ct.MaSpNavigation) // Bao gồm thông tin sản phẩm
                .Where(dh => dh.Username == userID)
                .OrderByDescending(dh => dh.TrangThai)
                .ToList();

            return View(donhang);
        }




        public IActionResult HuyDonHang(string maDH)
        {
            var userLogin = HttpContext.Session.GetString("userLogin");
            if (userLogin == null)
            {
                return RedirectToAction("Login", "User");
            }

            var donhang = _context.DonHangs
                            .Where(dh => dh.MaDh == maDH && dh.Username == userLogin)
                            .FirstOrDefault();

            if (donhang != null)
            {
                donhang.UpdatedAt = DateTime.Now;
                donhang.TrangThai = "Chờ xác nhận hủy đơn ";
                _context.SaveChanges();
            }

            return RedirectToAction("DonHang", "Home");
        }



        public IActionResult StoreLocation()
        {
            ViewBag.ApiKey = _configuration["GoogleMaps:ApiKey"];
            return View();
        }
        public IActionResult FeedbackHistory()
        {
            var username = HttpContext.Session.GetString("userLogin");
            if (username == null)
            {
                return RedirectToAction("Login", "User");
            }

            // Lấy người dùng dựa trên Username
            var user = _context.NguoiDungs.FirstOrDefault(u => u.Username == username);
            if (user == null)
            {
                return RedirectToAction("Login", "User");
            }

            // Lấy toàn bộ phản hồi và trả lời liên quan đến người dùng
            var feedbackHistory = _context.PhanHoiBaoCaos
                                        .Where(ph => ph.MaNguoiDung == user.MaNguoiDung ||
                                                     ph.MaBaoCaoNavigation.MaNguoiDung == user.MaNguoiDung)
                                        .OrderByDescending(ph => ph.NgayPhanHoi)
                                        .ToList();

            return View(feedbackHistory);
        }

    }

}
