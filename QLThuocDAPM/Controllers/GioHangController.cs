using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLThuocDAPM.Data;

using QLThuocDAPM.Helpers;
using QLThuocDAPM.Models;
using Microsoft.CodeAnalysis;
using static iTextSharp.text.pdf.AcroFields;
using QLThuocDAPM.Services;
using QLThuocDAPM.Services.VnPay;

namespace QLThuocDAPM.Controllers
{
    public class GioHangController : Controller
    {
        private readonly IVnPayService _vnPayService;

        private readonly QlthuocDapm6Context _context;

        private readonly PayPalService _payPalService;


        public GioHangController(QlthuocDapm6Context context, PayPalService payPalService, IVnPayService vnPayService)
        {
            _context = context;
            _vnPayService = vnPayService;

            _payPalService = payPalService;
        }

        const string CART_KEY = "MYCART";
        public List<CartItem> Cart => HttpContext.Session.Get<List<CartItem>>(CART_KEY) ?? new List<CartItem>();

        public IActionResult GetCartItemCount()
        {
            int itemCount = Cart.Sum(x => x.SoLuong);
            return Json(itemCount);
        }

        public IActionResult Index()
        {
            var cart = Cart; // Giả sử Cart là danh sách các sản phẩm trong giỏ hàng

            // Tính tổng tiền cho từng sản phẩm trong giỏ hàng
            foreach (var item in cart)
            {
                var product = _context.SanPhams
                    .Include(s => s.MaGiamGiaNavigation)
                    .FirstOrDefault(s => s.MaSp == item.MaHh);

                if (product != null)
                {
                    item.DonGia = product.GiaSauGiam ?? product.GiaTien; // Lấy giá sau giảm
                    item.TongTien = item.GiaSauGiam * item.SoLuong; // Tính tổng tiền cho sản phẩm
                    item.TongTienGoc = item.giaGoc * item.SoLuong; // Tính tổng tiền cho sản phẩm

                }
            }

            // Tính tổng tiền của giỏ hàng
            decimal totalAmount = (decimal)cart.Sum(item => item.TongTien);
            decimal totalAmountGoc = (decimal)cart.Sum(item => item.TongTienGoc);

            ViewData["TotalAmount"] = totalAmount; // Lưu tổng tiền vào ViewData để sử dụng trong view
            ViewData["TotalAmountGoc"] = totalAmountGoc;
            return View(cart); // Trả về view với danh sách sản phẩm trong giỏ hàng
        }

        public IActionResult AddToCart(int id, int quantity = 1, string type = "Normal")
        {
            var gioHang = Cart;

            var item = gioHang.SingleOrDefault(p => p.MaHh == id);
            if (item == null)
            {
                var hangHoa = _context.SanPhams
            .Include(p => p.MaGiamGiaNavigation) // Nạp thông tin giảm giá (nếu có)
            .SingleOrDefault(p => p.MaSp == id);
                ;
                if (hangHoa == null)
                {
                    TempData["Message"] = $"Không tìm thấy hàng hóa có mã {id}";
                    return Redirect("/404");
                }
                item = new CartItem
                {
                    MaHh = hangHoa.MaSp,
                    TenHH = hangHoa.TenSp,
                    giaGoc = hangHoa.GiaTien,
                    GiaTriGiamGia = hangHoa.MaGiamGiaNavigation.GiaTri,
                    NgayHetHanGiamGia = hangHoa.MaGiamGiaNavigation.ThoiGianKetThuc,
                    DonGia = hangHoa.GiaTien,
                    DonVi = hangHoa.DonVi,
                    Hinh = hangHoa.HinhAnh1 ?? string.Empty,
                    SoLuong = quantity,
                    TongTien = quantity * hangHoa.GiaTien
                };
                gioHang.Add(item);
            }
            else
            {
                item.SoLuong += quantity;
            }


            //TempData["SuccessMessage"] = $"Sản phẩm đã ở trong giỏ hàng của bạn";

            // Tính lại tổng số lượng trong giỏ

            HttpContext.Session.Set(CART_KEY, gioHang);

            var cartItemCount = gioHang.Sum(item => item.SoLuong);

            return RedirectToAction("Index");
        }
        // Lấy số lượng sản phẩm trong giỏ

        public IActionResult RemoveCart(int id)
        {
            var gioHang = Cart;
            var item = gioHang.SingleOrDefault(p => p.MaHh == id);
            if (item != null)
            {
                gioHang.Remove(item);
                HttpContext.Session.Set(CART_KEY, gioHang);
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        public IActionResult UpdateQuantity(int id, int quantity)
        {
            var gioHang = Cart; // Lấy giỏ hàng từ session
            var item = gioHang.SingleOrDefault(p => p.MaHh == id); // Tìm sản phẩm trong giỏ hàng

            if (item != null)
            {
                // Lấy số lượng sản phẩm từ database
                var sanPham = _context.SanPhams.SingleOrDefault(p => p.MaSp == id);
                if (sanPham == null)
                {
                    TempData["Error"] = "Sản phẩm không tồn tại.";
                    return RedirectToAction("Index");
                }
                int soLuongSanPham = sanPham?.SoLuong ?? 0; // Sử dụng null conditional và null coalescing để xử lý giá trị null
                                                            // Lấy số lượng sản phẩm thực tế từ database

                if (quantity > soLuongSanPham)
                {
                    TempData["Error"] = $"Không thể cập nhật. Sản phẩm chỉ còn {soLuongSanPham} sản phẩm.";
                    return RedirectToAction("Index");
                }

                if (quantity <= 0) // Nếu số lượng <= 0, xóa sản phẩm khỏi giỏ hàng
                {
                    gioHang.Remove(item);
                }
                else // Cập nhật số lượng hợp lệ
                {
                    item.SoLuong = quantity;
                    item.TongTien = item.DonGia * quantity; // Cập nhật tổng tiền
                }
            }

            HttpContext.Session.Set(CART_KEY, gioHang); // Lưu lại giỏ hàng vào session

            return RedirectToAction("Index"); // Quay lại trang giỏ hàng
        }

        public IActionResult ThanhToan()
        {


            // Kiểm tra xem người dùng đã đăng nhập chưa
            string username = HttpContext.Session.GetString("userLogin");

            if (username == null)
            {
                return RedirectToAction("Login", "User");
            }

            NguoiDung user = _context.NguoiDungs.FirstOrDefault(u => u.Username == username);

            if (user == null)
            {
                return NotFound();
            }

            ViewData["HoTen"] = user.HoTen;
            ViewData["Email"] = user.Email;
            ViewData["Sdt"] = user.Sdt;

            // Lấy giỏ hàng từ session
            var cart = HttpContext.Session.Get<List<CartItem>>(CART_KEY);
            // Nếu giỏ hàng rỗng, có thể hiển thị thông báo hoặc chuyển hướng
            if (cart == null || !cart.Any())
            {
                ViewData["ThongBao"] = "Giỏ hàng của bạn đang trống!";
                return View();
            }
            foreach (var item in cart)
            {
                var product = _context.SanPhams
                    .Include(s => s.MaGiamGiaNavigation)
                    .FirstOrDefault(s => s.MaSp == item.MaHh);

                if (product != null)
                {
                    item.DonGia = product.GiaSauGiam ?? product.GiaTien; // Lấy giá sau giảm
                    item.TongTien = item.GiaSauGiam * item.SoLuong; // Tính tổng tiền cho sản phẩm
                }
            }
            // Tính toán tổng tiền


            decimal totalAmount = (decimal)cart.Sum(item => item.TongTien);
            // Kiểm tra mã giảm giá (nếu có)
            var discountCode = HttpContext.Session.GetString("DiscountCode");
            if (!string.IsNullOrEmpty(discountCode))
            {
                var khuyenMai = _context.KhuyenMais.FirstOrDefault(km => km.MaKhuyenMai == discountCode &&
                    km.TrangThai &&
                    km.ThoiGianBatDau <= DateTime.Now &&
                    km.ThoiGianKetThuc >= DateTime.Now);

                if (khuyenMai != null)
                {
                    totalAmount -= khuyenMai.GiaTri; // Giảm giá từ tổng tiền
                }
            }

            // Đưa tổng tiền vào ViewData để sử dụng trong view
            ViewData["TotalAmount"] = totalAmount;
            ViewData["giamgia"] = 0;
            ViewData["Tongtien"] = cart.Sum(item => item.TongTien); // Tính tổng tiền đơn hàng

            return View(cart); // Trả về giỏ hàng để hiển thị
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LuuDonHang(string address, string tennguoinhan, string sdtnguoinhan)
        {
            // Lấy giỏ hàng từ session
            var shoppingCart = HttpContext.Session.Get<List<CartItem>>(CART_KEY);

            // Kiểm tra xem giỏ hàng có rỗng không
            if (shoppingCart == null || shoppingCart.Count == 0)
            {
                return Json(new { success = false, message = "Giỏ hàng của bạn đang trống!" });
            }

            // Kiểm tra xem người dùng đã đăng nhập chưa
            string username = HttpContext.Session.GetString("userLogin") ?? string.Empty;
            var cart = HttpContext.Session.Get<List<CartItem>>(CART_KEY);

            if (string.IsNullOrEmpty(username))
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập để thanh toán!" });
            }
            foreach (var item in cart)
            {
                var product = _context.SanPhams
                    .Include(s => s.MaGiamGiaNavigation)
                    .FirstOrDefault(s => s.MaSp == item.MaHh);

                if (product != null)
                {
                    item.DonGia = product.GiaSauGiam ?? product.GiaTien; // Lấy giá sau giảm
                    item.TongTien = item.DonGia * item.SoLuong; // Tính tổng tiền cho sản phẩm
                }
            }
            // Tính tổng tiền và số lượng sản phẩm trong giỏ hàng
            decimal totalAmount = (decimal)cart.Sum(item => item.TongTien);
            int soLuong = shoppingCart.Sum(item => item.SoLuong);
            string maDonHang = Guid.NewGuid().ToString(); // Tạo mã đơn hàng duy nhất



            // Kiểm tra mã khuyến mãi từ session (không bắt buộc)
            var discountCode = HttpContext.Session.GetString("DiscountCode");
            if (!string.IsNullOrEmpty(discountCode))
            {
                var khuyenMai = _context.KhuyenMais.FirstOrDefault(km => km.MaKhuyenMai == discountCode);
                if (discountCode == "abc" || khuyenMai == null)
                {
                    return Json(new { success = false, message = "Mã giảm giá không hợp lệ!" });
                }

                // Nếu mã hợp lệ, áp dụng giảm giá vào tổng tiền
                totalAmount -= khuyenMai.GiaTri;
            }
            // Đảm bảo tổng tiền không âm
            if (totalAmount < 0)
            {
                totalAmount = 0;
            }

            // Tạo đối tượng đơn hàng
            var donhang = new DonHang
            {
                MaKhuyenMai = string.IsNullOrEmpty(discountCode) ? "abc" : discountCode,
                TrangThai = "Đang chờ",
                TongTien = (double)totalAmount,
                Username = username,
                HoTen = tennguoinhan, // Gán tên người dùng
                Sdt = sdtnguoinhan,
                SoLuong = soLuong,
                Diachi = address,
                MaDh = maDonHang,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };

            // Lưu đơn hàng vào cơ sở dữ liệu
            _context.DonHangs.Add(donhang);
            _context.SaveChanges();

            // Lưu chi tiết đơn hàng
            foreach (var item in shoppingCart)
            {
                var chitiet = new ChiTietDonHang
                {
                    MaDh = maDonHang,
                    MaSp = item.MaHh,
                    SoLuong = item.SoLuong,
                    TongTien = item.TongTien2 // Đảm bảo rằng giá trị này đã được tính toán đúng
                };

                _context.ChiTietDonHangs.Add(chitiet);

                // Cập nhật sản phẩm trong kho
                var sanPham = _context.SanPhams.FirstOrDefault(sp => sp.MaSp == item.MaHh);
                if (sanPham != null)
                {
                    sanPham.SoLuongMua += item.SoLuong; // Cộng số lượng mua
                    sanPham.SoLuong -= item.SoLuong; // Giảm số lượng tồn kho
                    if (sanPham.SoLuong < 0)
                    {
                        sanPham.SoLuong = 0; // Đảm bảo không âm
                    }
                    _context.SanPhams.Update(sanPham); // Cập nhật sản phẩm
                }
            }

            _context.SaveChanges(); // Lưu tất cả thay đổi vào cơ sở dữ liệu

            // Xóa giỏ hàng sau khi thanh toán thành công
            HttpContext.Session.Remove(CART_KEY);
            var nguoidung = _context.NguoiDungs
    .FirstOrDefault(u => u.Username == username);

            // trường
            var orderItems = _context.ChiTietDonHangs
.Include(i => i.MaSpNavigation).Include(i => i.MaDhNavigation)
.Where(i => i.MaDh == maDonHang)
.ToList();
            // Gửi email thông báo
            string subject = "Thông báo đặt hàng thành công";

            // Tạo nội dung email thông báo thành công với màu xanh và cỡ chữ lớn cho lời chào, và mã đơn hàng in đậm
            string content = $"<p style='color: #4CAF50; font-size: 20px;'>Xin chào {nguoidung.HoTen},</p>" +
                             $"<p>Đơn hàng mã <strong>{donhang.MaDh}</strong> của bạn đã đặt thành công.<br><br>" +
                             $"Đơn hàng sẽ sớm được giao đến bạn.<br><br>Cảm ơn bạn đã tin tưởng và sử dụng dịch vụ của chúng tôi!</p>";

            // Khởi tạo chuỗi thông tin đơn hàng
            content += $"<p>Thông tin đơn hàng của bạn: <br><br> Tên người nhận: {donhang.HoTen}<br>";

            // Tạo bảng hiển thị các sản phẩm trong đơn hàng với màu sắc
            content += "<table border='1' cellpadding='5' cellspacing='0' style='border-collapse: collapse; width: 100%;'>";
            content += "<tr style='background-color: #4CAF50; color: white;'>";
            content += "<th>Tên Sản Phẩm</th><th>Số Lượng</th><th>Đơn Giá</th><th>Thành Tiền</th>";
            content += "</tr>";

            // Lặp qua danh sách các mặt hàng trong đơn hàng và hiển thị chúng trong bảng
            foreach (var item in orderItems)
            {
                var productName = item.MaSpNavigation.TenSp;
                var quantity = item.SoLuong;
                var price = item.MaSpNavigation.GiaSauGiam;
                var total = price * quantity;

                // Dòng dữ liệu sản phẩm với màu nền xen kẽ
                content += $"<tr style='background-color: #f9f9f9;'>";
                content += $"<td style='text-align: center' >{productName}</td><td style='text-align: center' >{quantity}</td><td style='text-align: center' >{price}.000 VNĐ</td><td style='text-align: center' >{total}.000 VNĐ</td>";
                content += "</tr>";
            }

            // Đóng bảng
            content += "</table>";


            // Thêm thông tin liên lạc và tổng tiền
            content += $"<br>Số điện thoại: {donhang.Sdt}<br>Địa chỉ: {donhang.Diachi}<<br>Giá trị giảm giá: {donhang.MaKhuyenMaiNavigation.GiaTri}.000 VNĐ<br>Tổng tiền sau khi giảm giá: {donhang.TongTien}.000 VNĐ";
                

            // Bây giờ bạn có thể gửi email với subject và content này


            // Gọi phương thức gửi email
            if (Common.Common.SendMail("Nhà Thuốc Long Châu", subject, content, nguoidung.Email)) // Email người dùng nằm ở trường `Email`
            {
                ViewBag.Message = "Thông báo giao hàng đã được gửi qua email của khách hàng.";
            }
            else
            {
                ViewBag.Error = "Có lỗi xảy ra trong quá trình gửi email thông báo.";
            }

            return View("ThanhCong", "GioHang"); // Chuyển hướng đến trang thành công
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ApDungMaGiamGia(string maKhuyenMai)
        {
            var cart = Cart;
            foreach (var item in cart)
            {
                var product = _context.SanPhams
                    .Include(s => s.MaGiamGiaNavigation)
                    .FirstOrDefault(s => s.MaSp == item.MaHh);

                if (product != null)
                {
                    item.DonGia = product.GiaSauGiam ?? product.GiaTien; // Lấy giá sau giảm
                    item.TongTien = item.DonGia * item.SoLuong; // Tính tổng tiền cho sản phẩm
                }
            }
            // Tính tổng tiền và số lượng sản phẩm trong giỏ hàng
            decimal totalAmount = (decimal)cart.Sum(item => item.TongTien);

            var khuyenMai = _context.KhuyenMais.FirstOrDefault(km => km.MaKhuyenMai == maKhuyenMai);



            if (khuyenMai == null)
            {
                ViewData["QuaHan"] = "Mã giảm giá không tồn tại!";
                ViewData["TotalAmount"] = totalAmount;
                ViewData["giamgia"] = 0;
                ViewData["Tongtien"] = cart.Sum(item => item.TongTien);
                return View("ThanhToan", cart);

            }

            if (khuyenMai.ThoiGianKetThuc < DateTime.Now) // Ngày kết thúc nhỏ hơn hiện tại
            {
                // Gán thông báo lỗi vào ViewData
                ViewData["QuaHan"] = "Mã giảm giá đã quá hạn!";
                ViewData["TotalAmount"] = totalAmount;
                ViewData["giamgia"] = 0;
                ViewData["Tongtien"] = cart.Sum(item => item.TongTien);
                return View("ThanhToan", cart); // Trả về view cùng với thông báo lỗi trong ViewData

            }

            // Lưu mã giảm giá vào session
            HttpContext.Session.SetString("DiscountCode", maKhuyenMai);


            totalAmount = (decimal)(cart.Sum(item => item.TongTien) - khuyenMai.GiaTri); // Giảm giá từ tổng tiền

            // Trả về view Index với tổng tiền đã được giảm
            ViewData["TotalAmount"] = totalAmount;
            ViewData["Tongtien"] = cart.Sum(item => item.TongTien);
            ViewData["giamgia"] = khuyenMai.GiaTri;
            ViewData["DiscountMessage"] = $"Mã giảm giá {maKhuyenMai} đã được áp dụng!";

            return View("ThanhToan", cart); // Trả về view ThanhToan với giỏ hàng và tổng tiền đã giảm
        }        
            

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePayment(decimal amount, string address, string tennguoinhan, string sdtnguoinhan)
        {
            var cart = HttpContext.Session.Get<List<CartItem>>(CART_KEY);



            foreach (var item in cart)
            {
                var product = _context.SanPhams
                    .Include(s => s.MaGiamGiaNavigation)
                    .FirstOrDefault(s => s.MaSp == item.MaHh);

                if (product != null)
                {
                    item.DonGia = product.GiaSauGiam ?? product.GiaTien; // Lấy giá sau giảm
                    item.TongTien = item.DonGia * item.SoLuong; // Tính tổng tiền cho sản phẩm
                }
                amount = (decimal)item.TongTien;
                amount = amount / 25000;


            }
            if (amount <= 0)
            {
                return BadRequest("Amount must be greater than zero.");
            }
            // Tính tổng tiền và số lượng sản phẩm trong giỏ hàng
            decimal totalAmount = (decimal)cart.Sum(item => item.TongTien);
            // Tạo đơn hàng trên PayPal và lấy đường dẫn duyệt thanh toán
            var approvalLink = await _payPalService.CreateOrderAsync(amount, "USD");
            var shoppingCart = HttpContext.Session.Get<List<CartItem>>(CART_KEY);

            // Kiểm tra xem giỏ hàng có rỗng không
            if (shoppingCart == null || shoppingCart.Count == 0)
            {
                return Json(new { success = false, message = "Giỏ hàng của bạn đang trống!" });
            }

            // Kiểm tra xem người dùng đã đăng nhập chưa
            string username = HttpContext.Session.GetString("userLogin") ?? string.Empty;

            if (string.IsNullOrEmpty(username))
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập để thanh toán!" });
            }
            foreach (var item in cart)
            {
                var product = _context.SanPhams
                    .Include(s => s.MaGiamGiaNavigation)
                    .FirstOrDefault(s => s.MaSp == item.MaHh);

                if (product != null)
                {
                    item.DonGia = product.GiaSauGiam ?? product.GiaTien; // Lấy giá sau giảm
                    item.TongTien = item.DonGia * item.SoLuong; // Tính tổng tiền cho sản phẩm
                }
            }
            // Tính tổng tiền và số lượng sản phẩm trong giỏ hàng
            int soLuong = shoppingCart.Sum(item => item.SoLuong);
            string maDonHang = Guid.NewGuid().ToString(); // Tạo mã đơn hàng duy nhất



            // Kiểm tra mã khuyến mãi từ session (không bắt buộc)
            var discountCode = HttpContext.Session.GetString("DiscountCode");
            if (!string.IsNullOrEmpty(discountCode))
            {
                var khuyenMai = _context.KhuyenMais.FirstOrDefault(km => km.MaKhuyenMai == discountCode);
                if (discountCode == "abc" || khuyenMai == null)
                {
                    return Json(new { success = false, message = "Mã giảm giá không hợp lệ!" });
                }

                // Nếu mã hợp lệ, áp dụng giảm giá vào tổng tiền
                totalAmount -= khuyenMai.GiaTri;
            }
            // Đảm bảo tổng tiền không âm
            if (totalAmount < 0)
            {
                totalAmount = 0;
            }

            // Tạo đối tượng đơn hàng
            var donhang = new DonHang
            {
                MaKhuyenMai = string.IsNullOrEmpty(discountCode) ? "abc" : discountCode,
                TrangThai = "Đang chờ",
                TongTien = (double)amount,
                Username = username,
                HoTen = tennguoinhan, // Gán tên người dùng
                Sdt = sdtnguoinhan,
                SoLuong = soLuong,
                Diachi = address,
                MaDh = maDonHang,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,

            };



            // Lưu đơn hàng vào cơ sở dữ liệu
            _context.DonHangs.Add(donhang);
            _context.SaveChanges();

            // Lưu chi tiết đơn hàng
            foreach (var item in shoppingCart)
            {
                var chitiet = new ChiTietDonHang
                {
                    MaDh = maDonHang,
                    MaSp = item.MaHh,
                    SoLuong = item.SoLuong,
                    TongTien = item.TongTien2 // Đảm bảo rằng giá trị này đã được tính toán đúng
                };

                _context.ChiTietDonHangs.Add(chitiet);

                // Cập nhật sản phẩm trong kho
                var sanPham = _context.SanPhams.FirstOrDefault(sp => sp.MaSp == item.MaHh);
                if (sanPham != null)
                {
                    sanPham.SoLuongMua += item.SoLuong; // Cộng số lượng mua
                    sanPham.SoLuong -= item.SoLuong; // Giảm số lượng tồn kho
                    if (sanPham.SoLuong < 0)
                    {
                        sanPham.SoLuong = 0; // Đảm bảo không âm
                    }
                    _context.SanPhams.Update(sanPham); // Cập nhật sản phẩm
                }
            }

            _context.SaveChanges(); // Lưu tất cả thay đổi vào cơ sở dữ liệu

            // Xóa giỏ hàng sau khi thanh toán thành công
            HttpContext.Session.Remove(CART_KEY);
            // Nếu có link để duyệt thanh toán, chuyển hướng đến PayPal
            if (!string.IsNullOrEmpty(approvalLink))
            {
                return Redirect(approvalLink); // Chuyển hướng đến PayPal



            }

            return BadRequest("Unable to create PayPal payment.");
        }

        public IActionResult PaymentSuccess(string token, string PayerID)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(PayerID))
            {
                // Nếu không nhận được thông tin từ PayPal, trả về lỗi
                return RedirectToAction("PaymentFailure");
            }

            else
            {


                return View("PaymentSuccess");
            }
        }


        [HttpGet]

        public async Task<IActionResult> PaymentCallbackVnpay()
        {


            string address = HttpContext.Session.GetString("address");
            string tennguoinhan = HttpContext.Session.GetString("tennguoinhan");
            string sdtnguoinhan = HttpContext.Session.GetString("sdtnguoinhan");


            if (string.IsNullOrEmpty(address) || string.IsNullOrEmpty(tennguoinhan) || string.IsNullOrEmpty(sdtnguoinhan))
            {
                return Json(new { success = false, message = "Vui lòng nhập đầy đủ thông tin!" });
            }
            var response = _vnPayService.PaymentExecute(Request.Query);

            var shoppingCart = HttpContext.Session.Get<List<CartItem>>(CART_KEY);

            // Kiểm tra xem giỏ hàng có rỗng không
            if (shoppingCart == null || shoppingCart.Count == 0)
            {
                return Json(new { success = false, message = "Giỏ hàng của bạn đang trống!" });
            }

            // Kiểm tra xem người dùng đã đăng nhập chưa
            string username = HttpContext.Session.GetString("userLogin") ?? string.Empty;
            var cart = HttpContext.Session.Get<List<CartItem>>(CART_KEY);

            if (string.IsNullOrEmpty(username))
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập để thanh toán!" });
            }
            foreach (var item in cart)
            {
                var product = _context.SanPhams
                    .Include(s => s.MaGiamGiaNavigation)
                    .FirstOrDefault(s => s.MaSp == item.MaHh);

                if (product != null)
                {
                    item.DonGia = product.GiaSauGiam ?? product.GiaTien; // Lấy giá sau giảm
                    item.TongTien = item.DonGia * item.SoLuong; // Tính tổng tiền cho sản phẩm
                }
            }
            // Tính tổng tiền và số lượng sản phẩm trong giỏ hàng
            decimal totalAmount = (decimal)cart.Sum(item => item.TongTien);
            int soLuong = shoppingCart.Sum(item => item.SoLuong);
            string maDonHang = Guid.NewGuid().ToString(); // Tạo mã đơn hàng duy nhất



            // Kiểm tra mã khuyến mãi từ session (không bắt buộc)
            var discountCode = HttpContext.Session.GetString("DiscountCode");
            if (!string.IsNullOrEmpty(discountCode))
            {
                var khuyenMai = _context.KhuyenMais.FirstOrDefault(km => km.MaKhuyenMai == discountCode);
                if (discountCode == "abc" || khuyenMai == null)
                {
                    return Json(new { success = false, message = "Mã giảm giá không hợp lệ!" });
                }

                // Nếu mã hợp lệ, áp dụng giảm giá vào tổng tiền
                totalAmount -= khuyenMai.GiaTri;
            }
            // Đảm bảo tổng tiền không âm
            if (totalAmount < 0)
            {
                totalAmount = 0;
            }

            // Tạo đối tượng đơn hàng
            var donhang = new DonHang
            {
                MaKhuyenMai = string.IsNullOrEmpty(discountCode) ? "abc" : discountCode,
                TrangThai = "Đang chờ",
                TongTien = (double)totalAmount,
                Username = username,
                HoTen = tennguoinhan, // Gán tên người dùng
                Sdt = sdtnguoinhan,
                SoLuong = soLuong,
                Diachi = address,
                MaDh = maDonHang,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };

            // Lưu đơn hàng vào cơ sở dữ liệu
            _context.DonHangs.Add(donhang);
            _context.SaveChanges();

            // Lưu chi tiết đơn hàng
            foreach (var item in shoppingCart)
            {
                var chitiet = new ChiTietDonHang
                {
                    MaDh = maDonHang,
                    MaSp = item.MaHh,
                    SoLuong = item.SoLuong,
                    TongTien = item.TongTien2 // Đảm bảo rằng giá trị này đã được tính toán đúng
                };

                _context.ChiTietDonHangs.Add(chitiet);

                // Cập nhật sản phẩm trong kho
                var sanPham = _context.SanPhams.FirstOrDefault(sp => sp.MaSp == item.MaHh);
                if (sanPham != null)
                {
                    sanPham.SoLuongMua += item.SoLuong; // Cộng số lượng mua
                    sanPham.SoLuong -= item.SoLuong; // Giảm số lượng tồn kho
                    if (sanPham.SoLuong < 0)
                    {
                        sanPham.SoLuong = 0; // Đảm bảo không âm
                    }
                    _context.SanPhams.Update(sanPham); // Cập nhật sản phẩm
                }
            }

            _context.SaveChanges(); // Lưu tất cả thay đổi vào cơ sở dữ liệu

            // Xóa giỏ hàng sau khi thanh toán thành công
            HttpContext.Session.Remove(CART_KEY);
            var nguoidung = _context.NguoiDungs
    .FirstOrDefault(u => u.Username == username);

            // trường
            var orderItems = _context.ChiTietDonHangs
.Include(i => i.MaSpNavigation).Include(i => i.MaDhNavigation)
.Where(i => i.MaDh == maDonHang)
.ToList();
            // Gửi email thông báo
            string subject = "Thông báo đặt hàng thành công";

            // Tạo nội dung email thông báo thành công với màu xanh và cỡ chữ lớn cho lời chào, và mã đơn hàng in đậm
            string content = $"<p style='color: #4CAF50; font-size: 20px;'>Xin chào {nguoidung.HoTen},</p>" +
                             $"<p>Đơn hàng mã <strong>{donhang.MaDh}</strong> của bạn đã đặt thành công.<br><br>" +
                             $"Đơn hàng sẽ sớm được giao đến bạn.<br><br>Cảm ơn bạn đã tin tưởng và sử dụng dịch vụ của chúng tôi!</p>";

            // Khởi tạo chuỗi thông tin đơn hàng
            content += $"<p>Thông tin đơn hàng của bạn: <br><br> Tên người nhận: {donhang.HoTen}<br>";

            // Tạo bảng hiển thị các sản phẩm trong đơn hàng với màu sắc
            content += "<table border='1' cellpadding='5' cellspacing='0' style='border-collapse: collapse; width: 100%;'>";
            content += "<tr style='background-color: #4CAF50; color: white;'>";
            content += "<th>Tên Sản Phẩm</th><th>Số Lượng</th><th>Đơn Giá</th><th>Thành Tiền</th>";
            content += "</tr>";

            // Lặp qua danh sách các mặt hàng trong đơn hàng và hiển thị chúng trong bảng
            foreach (var item in orderItems)
            {
                var productName = item.MaSpNavigation.TenSp;
                var quantity = item.SoLuong;
                var price = item.MaSpNavigation.GiaSauGiam;
                var total = price * quantity;

                // Dòng dữ liệu sản phẩm với màu nền xen kẽ
                content += $"<tr style='background-color: #f9f9f9;'>";
                content += $"<td style='text-align: center' >{productName}</td><td style='text-align: center' >{quantity}</td><td style='text-align: center' >{price}.000 VNĐ</td><td style='text-align: center' >{total}.000 VNĐ</td>";
                content += "</tr>";
            }

            // Đóng bảng
            content += "</table>";


            if (Common.Common.SendMail("Nhà Thuốc Long Châu", subject, content, nguoidung.Email)) // Email người dùng nằm ở trường `Email`
            {
                ViewBag.Message = "Thông báo giao hàng đã được gửi qua email của khách hàng.";
            }
            else
            {
                ViewBag.Error = "Có lỗi xảy ra trong quá trình gửi email thông báo.";
            }

            return View("ThanhCong", "GioHang"); // Chuyển hướng đến trang thành công
        }
        [HttpGet]
        public IActionResult ThanhCong()
        {
            return View();
        }

        public async Task<IActionResult> PaymentFailure()
        {
            // Kiểm tra thông tin từ PayPal
            var token = HttpContext.Request.Query["token"].ToString();
            if (string.IsNullOrEmpty(token))
            {
                TempData["Message"] = "Giao dịch đã bị huỷ. Vui lòng thử lại.";
                return RedirectToAction("Index", "GioHang");
            }

            // Thực hiện kiểm tra với PayPal để xác nhận trạng thái
            var capturedOrder = await _payPalService.CaptureOrderAsync(token);

            if (capturedOrder.Status != "COMPLETED")
            {
                TempData["Message"] = "Giao dịch tạm hoãn. Bạn có thể thử lại.";
            }
            else
            {
                TempData["Message"] = "Giao dịch đã hoàn tất!";
            }

            return RedirectToAction("Index", "GioHang");
        }


    }
}