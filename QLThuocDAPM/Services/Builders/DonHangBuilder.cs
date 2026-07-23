using QLThuocDAPM.Data;

namespace QLThuocDAPM.Services.Builders
{
    public class DonHangBuilder
    {
        private readonly DonHang _donHang;

        public DonHangBuilder()
        {
            _donHang = new DonHang();
        }

        public DonHangBuilder SetMaDh(string maDh)
        {
            _donHang.MaDh = maDh;
            return this;
        }

        public DonHangBuilder SetUsername(string username)
        {
            _donHang.Username = username;
            return this;
        }

        public DonHangBuilder SetDiaChi(string diaChi)
        {
            _donHang.Diachi = diaChi;
            return this;
        }

        public DonHangBuilder SetMaKhuyenMai(string maKhuyenMai)
        {
            _donHang.MaKhuyenMai = maKhuyenMai;
            return this;
        }

        public DonHangBuilder SetTongTien(double tongTien)
        {
            _donHang.TongTien = tongTien;
            return this;
        }

        public DonHangBuilder SetSoLuong(int soLuong)
        {
            _donHang.SoLuong = soLuong;
            return this;
        }

        public DonHangBuilder SetTrangThai(string trangThai)
        {
            _donHang.TrangThai = trangThai;
            return this;
        }

        public DonHangBuilder SetCreatedAt(DateTime? createdAt)
        {
            _donHang.CreatedAt = createdAt;
            return this;
        }

        public DonHangBuilder SetUpdatedAt(DateTime? updatedAt)
        {
            _donHang.UpdatedAt = updatedAt;
            return this;
        }

        public DonHangBuilder SetMaNguoiDung(int? maNguoiDung)
        {
            _donHang.MaNguoiDung = maNguoiDung;
            return this;
        }

        public DonHangBuilder SetHoTen(string hoTen)
        {
            _donHang.HoTen = hoTen;
            return this;
        }

        public DonHangBuilder SetSdt(string sdt)
        {
            _donHang.Sdt = sdt;
            return this;
        }

        public DonHang Build()
        {
            return _donHang;
        }
    }
}
