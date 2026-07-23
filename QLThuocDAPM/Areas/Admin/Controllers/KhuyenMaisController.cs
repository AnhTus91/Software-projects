using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLThuocDAPM.Data;
using QLThuocDAPM.Models;
using System.Threading.Tasks;

namespace QLThuocDAPM.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class KhuyenMaisController : Controller
    {
        private readonly QlthuocDapm6Context db;

        public KhuyenMaisController(QlthuocDapm6Context context)
        {
            db = context;
        }

        // GET: KhuyenMais
        public async Task<IActionResult> Index()
        {
            return View(await db.KhuyenMais.ToListAsync());
        }

        // GET: Admin/KhuyenMais/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            var khuyenMai = await db.KhuyenMais.FindAsync(id);
            if (khuyenMai == null)
            {
                return NotFound();
            }
            return View(khuyenMai);
        }

        // GET: Admin/KhuyenMais/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/KhuyenMais/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaKhuyenMai,GiaTri,ThoiGianBatDau,ThoiGianKetThuc,TrangThai,NgayTao,DieuKienApDung,SoLuong")] KhuyenMai khuyenMai)
        {



            if (khuyenMai.ThoiGianKetThuc > DateTime.Now)
            {
                khuyenMai.TrangThai = true;
                await db.KhuyenMais.AddAsync(khuyenMai);
                await db.SaveChangesAsync();
            }
            else
            {
                khuyenMai.TrangThai = false;
                await db.KhuyenMais.AddAsync(khuyenMai);
                await db.SaveChangesAsync();
            }



            return View(Index);
        }

        // GET: Admin/KhuyenMais/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            var khuyenMai = await db.KhuyenMais.FindAsync(id);
            if (khuyenMai == null)
            {
                return NotFound();
            }
            return View(khuyenMai);
        }
        // Action Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Tìm mã khuyến mãi theo id
            var khuyenMai = await db.KhuyenMais.FindAsync(id);

            if (khuyenMai == null)
            {
                return NotFound();
            }

            // Xóa mã khuyến mãi
            db.KhuyenMais.Remove(khuyenMai);
            await db.SaveChangesAsync();
            // Chuyển hướng về trang danh sách khuyến mãi sau khi xóa thành công
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/KhuyenMais/Edit/5
    }
}