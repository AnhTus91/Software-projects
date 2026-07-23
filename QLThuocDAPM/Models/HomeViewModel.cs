using QLThuocDAPM.Data;

using System.Collections.Generic;

namespace QLThuocDAPM.ViewModels
{
    public class HomeViewModel
    {
        internal List<Benh> benhNoiBat;

        public List<DanhMuc> DanhMucs { get; set; } // Danh sách danh mục
        public Dictionary<int, List<SanPham>> SanPhams { get; set; } // Sản phẩm theo danh mục

        public List<NhaCungCap> NhaCungCap { get; set; }
        public List<SanPham> SanPhamNoiBat { get; internal set; }

        public SanPham SanPham { get; set; }

        public GiamGium GiamGium { get; set; }

        public double GetGiaSauGiam(SanPham sanPham)
        {
            if (sanPham.MaGiamGiaNavigation != null && sanPham.MaGiamGiaNavigation.GiaTri > 0)
            {
                double discountAmount = sanPham.GiaTien * sanPham.MaGiamGiaNavigation.GiaTri / 100;
                return sanPham.GiaTien - discountAmount;
            }
            return sanPham.GiaTien; // Trả về giá gốc nếu không có giảm giá
        }



    }
}