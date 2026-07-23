using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLThuocDAPM.Data;
using QLThuocDAPM.Common;
using iTextSharp.text;
using iTextSharp.text.pdf;
using QLThuocDAPM.Services.Builders;

namespace QLThuocDAPM.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DonHangsController : Controller
    {
        private readonly QlthuocDapm6Context _context;
        private readonly Common.Common _common;

        public DonHangsController(QlthuocDapm6Context context, Common.Common common)
        {
            _context = context;
            _common = common;
        }

        // GET: Admin/DonHangs
        //public async Task<IActionResult> Index()
        //{
        //    var qlthuocDapm2Context = _context.DonHangs.Include(d => d.MaKhuyenMaiNavigation).Include(d => d.MaNguoiDungNavigation);
        //    return View(await qlthuocDapm2Context.ToListAsync());
        //}



        public async Task<IActionResult> Index(int pageNumber = 1)
        {
            int pageSize = 5; // Số đơn hàng trên mỗi trang

            // Sắp xếp danh sách đơn hàng theo CreatedAt mới nhất
            var qlthuocDapm2Context = _context.DonHangs
                .Include(s => s.MaKhuyenMaiNavigation)
                .Include(s => s.MaNguoiDungNavigation)
                .OrderByDescending(s => s.CreatedAt) // Sắp xếp theo CreatedAt giảm dần
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            // Đếm tổng số đơn hàng
            int totalProducts = await _context.DonHangs.CountAsync();
            int totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);

            // Truyền dữ liệu phân trang cho View
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;

            return View(await qlthuocDapm2Context.ToListAsync());
        }


        // GET: Admin/DonHangs/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var donHang = await _context.DonHangs
                .Include(d => d.MaKhuyenMaiNavigation)
                .Include(d => d.MaNguoiDungNavigation)
                .FirstOrDefaultAsync(m => m.MaDh == id);
            if (donHang == null)
            {
                return NotFound();
            }

            return View(donHang);
        }

        // GET: Admin/DonHangs/Create
        public IActionResult Create()
        {
            ViewData["MaKhuyenMai"] = new SelectList(_context.KhuyenMais, "MaKhuyenMai", "MaKhuyenMai");
            ViewData["MaNguoiDung"] = new SelectList(_context.NguoiDungs, "MaNguoiDung", "MaNguoiDung");
            return View();
        }

        // POST: Admin/DonHangs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("MaDh,Username,Diachi,MaKhuyenMai,TongTien,SoLuong,TrangThai,CreatedAt,UpdatedAt,MaNguoiDung,HoTen,Sdt")] DonHang donHang)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(donHang);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    ViewData["MaKhuyenMai"] = new SelectList(_context.KhuyenMais, "MaKhuyenMai", "MaKhuyenMai", donHang.MaKhuyenMai);
        //    ViewData["MaNguoiDung"] = new SelectList(_context.NguoiDungs, "MaNguoiDung", "MaNguoiDung", donHang.MaNguoiDung);
        //    return View(donHang);
        //}

        // Bulider
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string MaDh, string Username, string Diachi, string MaKhuyenMai, double TongTien, int SoLuong, string TrangThai, int? MaNguoiDung, string HoTen, string Sdt)
        {
            if (ModelState.IsValid)
            {
                var donHang = new DonHangBuilder()
                    .SetMaDh(MaDh)
                    .SetUsername(Username)
                    .SetDiaChi(Diachi)
                    .SetMaKhuyenMai(MaKhuyenMai)
                    .SetTongTien(TongTien)
                    .SetSoLuong(SoLuong)
                    .SetTrangThai(TrangThai)
                    .SetCreatedAt(DateTime.Now)
                    .SetUpdatedAt(DateTime.Now)
                    .SetMaNguoiDung(MaNguoiDung)
                    .SetHoTen(HoTen)
                    .SetSdt(Sdt)
                    .Build();

                _context.Add(donHang);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View();
        }


        // GET: Admin/DonHangs/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var donHang = await _context.DonHangs.FindAsync(id);
            if (donHang == null)
            {
                return NotFound();
            }
            ViewData["MaKhuyenMai"] = new SelectList(_context.KhuyenMais, "MaKhuyenMai", "MaKhuyenMai", donHang.MaKhuyenMai);
            ViewData["MaNguoiDung"] = new SelectList(_context.NguoiDungs, "MaNguoiDung", "MaNguoiDung", donHang.MaNguoiDung);
            return View(donHang);
        }

        // POST: Admin/DonHangs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(string id, [Bind("MaDh,Username,Diachi,MaKhuyenMai,TongTien,SoLuong,TrangThai,CreatedAt,UpdatedAt,MaNguoiDung,HoTen,Sdt")] DonHang donHang)
        //{
        //    if (id != donHang.MaDh)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(donHang);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!DonHangExists(donHang.MaDh))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //    ViewData["MaKhuyenMai"] = new SelectList(_context.KhuyenMais, "MaKhuyenMai", "MaKhuyenMai", donHang.MaKhuyenMai);
        //    ViewData["MaNguoiDung"] = new SelectList(_context.NguoiDungs, "MaNguoiDung", "MaNguoiDung", donHang.MaNguoiDung);
        //    return View(donHang);
        //}

        //Builder

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, string Username, string Diachi, string MaKhuyenMai, double TongTien, int SoLuong, string TrangThai, int? MaNguoiDung, string HoTen, string Sdt)
        {
            var donHang = await _context.DonHangs.FindAsync(id);
            if (donHang == null) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    donHang = new DonHangBuilder()
                        .SetMaDh(id)
                        .SetUsername(Username)
                        .SetDiaChi(Diachi)
                        .SetMaKhuyenMai(MaKhuyenMai)
                        .SetTongTien(TongTien)
                        .SetSoLuong(SoLuong)
                        .SetTrangThai(TrangThai)
                        .SetUpdatedAt(DateTime.Now)
                        .SetMaNguoiDung(MaNguoiDung)
                        .SetHoTen(HoTen)
                        .SetSdt(Sdt)
                        .Build();

                    _context.Update(donHang);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DonHangExists(id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(donHang);
        }

        // GET: Admin/DonHangs/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var donHang = await _context.DonHangs
                .Include(d => d.MaKhuyenMaiNavigation)
                .Include(d => d.MaNguoiDungNavigation)
                .FirstOrDefaultAsync(m => m.MaDh == id);
            if (donHang == null)
            {
                return NotFound();
            }

            return View(donHang);
        }

        // POST: Admin/DonHangs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var donHang = await _context.DonHangs.FindAsync(id);
            if (donHang != null)
            {
                _context.DonHangs.Remove(donHang);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DonHangExists(string id)
        {
            return _context.DonHangs.Any(e => e.MaDh == id);
        }
        [HttpPost]
        public IActionResult UpdateOrderStatus(string id)
        {
            var donHang = _context.DonHangs.FirstOrDefault(d => d.MaDh == id); // Tìm đơn hàng theo ID

            if (donHang != null)
            {
                donHang.TrangThai = "Đã giao"; // Cập nhật trạng thái đơn hàng
                _context.SaveChanges(); // Lưu thay đổi vào DB

                // Gửi email thông báo
                string subject = "Thông báo giao hàng thành công";
                string content = $"Xin chào {donHang.HoTen},<br><br>Đơn hàng mã {donHang.MaDh} của bạn đã được giao thành công.<br><br>Cảm ơn bạn đã tin tưởng và sử dụng dịch vụ của chúng tôi!";

                // Gọi phương thức gửi email
                if (Common.Common.SendMail("Nhà Thuốc Long Châu", subject, content, "dongtrieudeptraizodich@gmail.com")) // Email người dùng nằm ở trường `Email`
                {
                    ViewBag.Message = "Thông báo giao hàng đã được gửi qua email của khách hàng.";
                }
                else
                {
                    ViewBag.Error = "Có lỗi xảy ra trong quá trình gửi email thông báo.";
                }

                return RedirectToAction("Index"); // Chuyển hướng về trang danh sách đơn hàng
            }

            return NotFound(); // Trả về lỗi nếu không tìm thấy đơn hàng
        }

        // Chưa sửa
        [HttpPost]
        public IActionResult XacNhanDon(string id)
        {
            var donHang = _context.DonHangs.FirstOrDefault(d => d.MaDh == id); // Tìm đơn hàng theo ID

            if (donHang != null)
            {
                donHang.TrangThai = "Đã xác nhận đơn hàng sẽ sớm được giao đến bạn"; // Cập nhật trạng thái đơn hàng
                _context.SaveChanges(); // Lưu thay đổi vào DB
                return RedirectToAction("Index"); // Chuyển hướng về trang danh sách đơn hàng
            }

            return NotFound(); // Trả về lỗi nếu không tìm thấy đơn hàng
        }


        [HttpPost]
        public IActionResult ChapNhanHuy(string id)
        {
            var donHang = _context.DonHangs.FirstOrDefault(d => d.MaDh == id); // Tìm đơn hàng theo ID

            if (donHang != null)
            {
                donHang.TrangThai = "Hủy đơn hàng thành công"; // Cập nhật trạng thái đơn hàng
                _context.SaveChanges(); // Lưu thay đổi vào DB
                return RedirectToAction("Index"); // Chuyển hướng về trang danh sách đơn hàng
            }

            return NotFound(); // Trả về lỗi nếu không tìm thấy đơn hàng
        }
        





        public ActionResult XuatHoaDonPDF(string maDH)
        {
            var donhang = _context.DonHangs
                .Include(dh => dh.ChiTietDonHangs)
                    .ThenInclude(ct => ct.MaSpNavigation)  // Ensure product navigation property is loaded
                .FirstOrDefault(dh => dh.MaDh == maDH /*&& dh.Username == userID*/);

            if (donhang == null)
            {
                return NotFound(); // Trả về 404 nếu không tìm thấy đơn hàng
            }

            using (MemoryStream stream = new MemoryStream())
            {
                Document pdfDoc = new Document(PageSize.A4, 30f, 30f, 30f, 30f); // Set margin for the document
                PdfWriter.GetInstance(pdfDoc, stream);

                // Use "arialuni.ttf" for Vietnamese support
                string fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");
                var baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                var fontNormal = new Font(baseFont, 12, Font.NORMAL);
                var fontBold = new Font(baseFont, 14, Font.BOLD);

                pdfDoc.Open();

                // Add Title
                Paragraph title = new Paragraph("HÓA ĐƠN", fontBold)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 20f
                };
                pdfDoc.Add(title);

                // Add order information
                PdfPTable infoTable = new PdfPTable(2)
                {
                    WidthPercentage = 100,
                    SpacingAfter = 20f
                };
                infoTable.SetWidths(new float[] { 1, 2 }); // Adjust column width ratio
                infoTable.AddCell(CreateCell("Mã Hóa Đơn:", fontBold, Element.ALIGN_LEFT, PdfPCell.NO_BORDER));
                infoTable.AddCell(CreateCell(donhang.MaDh, fontNormal, Element.ALIGN_LEFT, PdfPCell.NO_BORDER));
                infoTable.AddCell(CreateCell("Tên Khách Hàng:", fontBold, Element.ALIGN_LEFT, PdfPCell.NO_BORDER));
                infoTable.AddCell(CreateCell(donhang.HoTen, fontNormal, Element.ALIGN_LEFT, PdfPCell.NO_BORDER));
                infoTable.AddCell(CreateCell("Số Điện Thoại:", fontBold, Element.ALIGN_LEFT, PdfPCell.NO_BORDER));
                infoTable.AddCell(CreateCell(donhang.Sdt, fontNormal, Element.ALIGN_LEFT, PdfPCell.NO_BORDER));
                infoTable.AddCell(CreateCell("Địa Chỉ:", fontBold, Element.ALIGN_LEFT, PdfPCell.NO_BORDER));
                infoTable.AddCell(CreateCell(donhang.Diachi, fontNormal, Element.ALIGN_LEFT, PdfPCell.NO_BORDER));
                pdfDoc.Add(infoTable);

                // Add table for order details
                PdfPTable table = new PdfPTable(5)
                {
                    WidthPercentage = 100,
                    SpacingBefore = 10f,
                    SpacingAfter = 20f
                };
                table.SetWidths(new float[] { 3, 1, 1, 1, 1 }); // Adjust column width ratio

                // Add table header
                AddTableHeader(table, new string[] { "Tên Sản Phẩm", "Số Lượng", "Giá", "Giảm Giá", "Tổng" }, fontBold);

                foreach (var item in donhang.ChiTietDonHangs)
                {
                    string tenSp = item.MaSpNavigation?.TenSp ?? "Sản phẩm không tồn tại";
                    string gia = $"{item.MaSpNavigation?.GiaTien:N0}.000 VNĐ";
                    string soLuong = item.SoLuong.ToString();
                    string giamgia = $"{((item.MaSpNavigation?.GiaTien * item.SoLuong) - donhang.TongTien):N0}.000 VNĐ";
                    string tongTien = $"{(item.MaSpNavigation?.GiaTien * item.SoLuong):N0}.000 VNĐ";

                    table.AddCell(CreateCell(tenSp, fontNormal, Element.ALIGN_LEFT));
                    table.AddCell(CreateCell(soLuong, fontNormal, Element.ALIGN_CENTER));
                    table.AddCell(CreateCell(gia, fontNormal, Element.ALIGN_RIGHT));
                    table.AddCell(CreateCell(giamgia, fontNormal, Element.ALIGN_RIGHT));
                    table.AddCell(CreateCell(tongTien, fontNormal, Element.ALIGN_RIGHT));
                }

                pdfDoc.Add(table);

                // Add total amount
                Paragraph total = new Paragraph($"Tổng Giá Trị Đơn Hàng: {donhang.TongTien:N0}.000 VNĐ", fontBold)
                {
                    Alignment = Element.ALIGN_RIGHT,
                    SpacingBefore = 10f
                };
                pdfDoc.Add(total);

                pdfDoc.Close();

                // Return PDF to user
                byte[] bytes = stream.ToArray();
                return File(bytes, "application/pdf", "HoaDon_" + donhang.MaDh + ".pdf");
            }
        }

        // Helper method to create table cell
        private PdfPCell CreateCell(string content, Font font, int alignment, int border = PdfPCell.BOX)
        {
            return new PdfPCell(new Phrase(content, font))
            {
                HorizontalAlignment = alignment,
                Border = border,
                Padding = 5f
            };
        }

        // Helper method to add table header
        private void AddTableHeader(PdfPTable table, string[] headers, Font font)
        {
            foreach (var header in headers)
            {
                PdfPCell cell = new PdfPCell(new Phrase(header, font))
                {
                    BackgroundColor = new BaseColor(240, 240, 240), // Light gray background
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 5f
                };
                table.AddCell(cell);
            }
        }



        //public IActionResult DoanhThu()
        //{
        //    var doanhThu = _context.ChiTietDonHangs
        //        .Join(_context.DonHangs, cdh => cdh.MaDh, dh => dh.MaDh, (cdh, dh) => new { cdh, dh })
        //        .Join(_context.SanPhams, combined => combined.cdh.MaSp, sp => sp.MaSp, (combined, sp) => new { combined.cdh, combined.dh, sp })
        //        .Join(_context.DanhMucs, combined => combined.sp.MaDm, dm => dm.MaDm, (combined, dm) => new
        //        {
        //            DanhMuc = dm.TenDm,
        //            DoanhThu = combined.cdh.TongTien
        //        })
        //        .GroupBy(x => x.DanhMuc)
        //        .Select(g => new
        //        {
        //            DanhMuc = g.Key,
        //            DoanhThu = g.Sum(x => x.DoanhThu)
        //        })
        //        .ToList();

        //    return View(doanhThu);
        //}

        public IActionResult DoanhThu1()
        {
            var doanhThuNam = _context.DonHangs
                .Where(d => d.TrangThai == "Đã giao" && d.CreatedAt.HasValue)
                .GroupBy(d => d.CreatedAt.Value.Year)
                .Select(g => new
                {
                    Nam = g.Key,
                    DoanhThu = g.Sum(d => d.TongTien)
                })
                .OrderBy(d => d.Nam)
                .ToList();

            return View(doanhThuNam);
        }
        // Action để lấy đơn hàng theo ngày trong tháng
        public IActionResult DoanhThuTheoThang(int nam)
        {
            var doanhThuThang = _context.DonHangs
                .Where(d => d.TrangThai == "Đã giao" && d.CreatedAt.HasValue && d.CreatedAt.Value.Year == nam)
                .GroupBy(d => d.CreatedAt.Value.Month)
                .Select(g => new
                {
                    Nam = nam,  // Thêm năm vào trong đối tượng ẩn danh
                    Thang = g.Key,
                    DoanhThu = g.Sum(d => d.TongTien)
                })
                .OrderBy(d => d.Thang)
                .ToList();

            if (doanhThuThang.Count == 0)
            {
                ViewBag.Message = "Không có dữ liệu doanh thu cho năm này!";
            }

            return View(doanhThuThang);
        }


        // Action để lấy thống kê doanh thu theo ngày
        public IActionResult DoanhThuTheoNgay(int nam, int thang)
        {
            var doanhThuNgay = _context.DonHangs
                .Where(d => d.TrangThai == "Đã giao"
                            && d.CreatedAt.HasValue
                            && d.CreatedAt.Value.Year == nam
                            && d.CreatedAt.Value.Month == thang)
                .GroupBy(d => d.CreatedAt.Value.Day)
                .Select(g => new
                {
                    Nam = nam,  // Thêm năm vào đối tượng ẩn danh
                    Thang = thang,  // Thêm tháng vào đối tượng ẩn danh
                    Ngay = g.Key,  // Ngày
                    DoanhThu = g.Sum(d => d.TongTien)
                })
                .OrderBy(d => d.Ngay)
                .ToList();

            if (doanhThuNgay.Count == 0)
            {
                ViewBag.Message = "Không có dữ liệu doanh thu cho tháng này!";
            }

            return View(doanhThuNgay);
        }

        // Action để lấy danh sách các đơn hàng theo ngày
        public IActionResult DonHangTheoNgay(int nam, int thang, int ngay)
        {
            var donHangsTheoNgay = _context.DonHangs
                .Where(d => d.TrangThai == "Đã giao"
                            && d.CreatedAt.HasValue
                            && d.CreatedAt.Value.Year == nam
                            && d.CreatedAt.Value.Month == thang
                            && d.CreatedAt.Value.Day == ngay)
                .ToList();

            if (donHangsTheoNgay.Count == 0)
            {
                ViewBag.Message = "Không có đơn hàng trong ngày này!";
            }

            return View(donHangsTheoNgay);
        }

        public IActionResult SoLuongDaBan()
        {
            var soLuongBan = _context.ChiTietDonHangs
                .Join(_context.DonHangs, cdh => cdh.MaDh, dh => dh.MaDh, (cdh, dh) => new { cdh, dh })
                .Where(x => x.dh.TrangThai == "Đã Giao")
                .Join(_context.SanPhams, x => x.cdh.MaSp, sp => sp.MaSp, (x, sp) => new { x.cdh, sp })
                .GroupBy(x => new { x.sp.MaSp, x.sp.TenSp })
                .Select(g => new
                {
                    MaSanPham = g.Key.MaSp,
                    TenSanPham = g.Key.TenSp,
                    SoLuongDaBan = g.Sum(x => x.cdh.SoLuong)
                })
                .OrderByDescending(x => x.SoLuongDaBan)
                .ToList();

            // Xác định số lượng bán cao nhất và thấp nhất
            var maxSoLuongDaBan = soLuongBan.Max(x => x.SoLuongDaBan);
            var minSoLuongDaBan = soLuongBan.Min(x => x.SoLuongDaBan);

            ViewBag.TenSanPham = soLuongBan.Select(x => x.TenSanPham).ToArray();
            ViewBag.SoLuongDaBan = soLuongBan.Select(x => x.SoLuongDaBan).ToArray();

            var danhGiaSanPham = soLuongBan.Select(x => new
            {
                MaSanPham = x.MaSanPham,
                TenSanPham = x.TenSanPham,
                SoLuongDaBan = x.SoLuongDaBan,
                NhanXet = GetProductComment(x.SoLuongDaBan, maxSoLuongDaBan, minSoLuongDaBan), // Truyền maxSoLuongDaBan và minSoLuongDaBan vào
                Url = Url.Action("ChiTietSanPham", "SanPham", new { area = "user", id = x.MaSanPham })

            }).ToList();

            ViewBag.DanhGiaSanPham = danhGiaSanPham;

            return View();
        }

        // Hàm nhận xét sản phẩm, với maxSoLuongDaBan và minSoLuongDaBan để xác định sản phẩm bán chạy nhất và ít được mua nhất
        private string GetProductComment(int soLuongDaBan, int maxSoLuongDaBan, int minSoLuongDaBan)
        {
            if (soLuongDaBan == maxSoLuongDaBan)
                return "Sản phẩm bán chạy nhất!";
            else if (soLuongDaBan == minSoLuongDaBan)
                return "Sản phẩm ít được mua.";
            else if (soLuongDaBan >= 50)
                return "Sản phẩm khá phổ biến.";
            else if (soLuongDaBan >= 20)
                return "Sản phẩm đang được ưa chuộng.";
            else
                return "Sản phẩm ít được mua.";
        }

    }
}
