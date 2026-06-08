using System;
using System.Collections.Generic;

namespace QLKhachSan.Models
{
    // 1. Vai Trò
    public class VaiTro
    {
        public int Id { get; set; }
        public string TenVaiTro { get; set; }

        public ICollection<TaiKhoan> TaiKhoans { get; set; }
    }

    // 2. Tài Khoản
    public class TaiKhoan
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string HoTen { get; set; }
        public int VaiTroId { get; set; }
        public bool TrangThai { get; set; }

        public VaiTro VaiTro { get; set; }
        public ICollection<PhieuDatPhong> PhieuDatPhongs { get; set; }
        public ICollection<HoaDon> HoaDons { get; set; }
    }

    // 3. Khách Hàng
    public class KhachHang
    {
        public int Id { get; set; }
        public string HoTen { get; set; }
        public string CCCD { get; set; }
        public string SDT { get; set; }
        public string DiaChi { get; set; }

        public ICollection<PhieuDatPhong> PhieuDatPhongs { get; set; }
    }

    // 4. Loại Phòng
    public class LoaiPhong
    {
        public int Id { get; set; }
        public string TenLoai { get; set; }
        public decimal GiaNgay { get; set; }
        public int SoNguoiToiDa { get; set; }

        public ICollection<Phong> Phongs { get; set; }
        public ICollection<LoaiPhong_TienNghi> LoaiPhong_TienNghis { get; set; }
    }

    // 5. Tiện Nghi
    public class TienNghi
    {
        public int Id { get; set; }
        public string TenTienNghi { get; set; }
        public string MoTa { get; set; }

        public ICollection<LoaiPhong_TienNghi> LoaiPhong_TienNghis { get; set; }
    }

    // 6. Loại Phòng - Tiện Nghi (N-N)
    public class LoaiPhong_TienNghi
    {
        public int LoaiPhongId { get; set; }
        public LoaiPhong LoaiPhong { get; set; }

        public int TienNghiId { get; set; }
        public TienNghi TienNghi { get; set; }
    }

    // 7. Phòng
    public class Phong
    {
        public int Id { get; set; }
        public string TenPhong { get; set; }
        public int LoaiPhongId { get; set; }
        public string TrangThai { get; set; } // Trong, DangSuDung, BaoTri

        public LoaiPhong LoaiPhong { get; set; }
        public ICollection<ChiTietDatPhong> ChiTietDatPhongs { get; set; }
    }

    // 8. Phiếu Đặt Phòng
    public class PhieuDatPhong
    {
        public int Id { get; set; }
        public int KhachHangId { get; set; }
        public int TaiKhoanId { get; set; }
        public DateTime NgayLap { get; set; }
        public decimal TienDatCoc { get; set; }
        public string TrangThai { get; set; } // DaDat, DangO, DaThanhToan, DaHuy

        public KhachHang KhachHang { get; set; }
        public TaiKhoan TaiKhoan { get; set; }
        public ICollection<ChiTietDatPhong> ChiTietDatPhongs { get; set; }
        public ICollection<HoaDon> HoaDons { get; set; }
    }

    // 9. Chi Tiết Đặt Phòng
    public class ChiTietDatPhong
    {
        public int Id { get; set; }
        public int PhieuDatPhongId { get; set; }
        public int PhongId { get; set; }
        public DateTime NgayNhan { get; set; }
        public DateTime NgayTra { get; set; }
        public decimal GiaThoaThuan { get; set; }

        public PhieuDatPhong PhieuDatPhong { get; set; }
        public Phong Phong { get; set; }
        public ICollection<PhieuSuDungDichVu> PhieuSuDungDichVus { get; set; }
    }

    // 10. Loại Dịch Vụ
    public class LoaiDichVu
    {
        public int Id { get; set; }
        public string TenLoai { get; set; }

        public ICollection<DichVu> DichVus { get; set; }
    }

    // 11. Dịch Vụ
    public class DichVu
    {
        public int Id { get; set; }
        public string TenDichVu { get; set; }
        public int LoaiDichVuId { get; set; }
        public decimal DonGia { get; set; }

        public LoaiDichVu LoaiDichVu { get; set; }
        public ICollection<ChiTietSuDungDichVu> ChiTietSuDungDichVus { get; set; }
    }

    // 12. Phiếu Sử Dụng Dịch Vụ
    public class PhieuSuDungDichVu
    {
        public int Id { get; set; }
        public int ChiTietDatPhongId { get; set; }
        public DateTime NgayTao { get; set; }
        public string TrangThai { get; set; }

        public ChiTietDatPhong ChiTietDatPhong { get; set; }
        public ICollection<ChiTietSuDungDichVu> ChiTietSuDungDichVus { get; set; }
    }

    // 13. Chi Tiết Sử Dụng Dịch Vụ
    public class ChiTietSuDungDichVu
    {
        public int Id { get; set; }
        public int PhieuSuDungDichVuId { get; set; }
        public int DichVuId { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien { get; set; }

        public PhieuSuDungDichVu PhieuSuDungDichVu { get; set; }
        public DichVu DichVu { get; set; }
    }

    // 14. Khuyến Mãi
    public class KhuyenMai
    {
        public int Id { get; set; }
        public string TenKM { get; set; }
        public double PhanTramGiam { get; set; }
        public DateTime NgayBatDau { get; set; }
        public DateTime NgayKetThuc { get; set; }

        public ICollection<HoaDon> HoaDons { get; set; }
    }

    // 15. Hóa Đơn
    public class HoaDon
    {
        public int Id { get; set; }
        public int PhieuDatPhongId { get; set; }
        public int TaiKhoanId { get; set; }
        public int? KhuyenMaiId { get; set; }
        public DateTime NgayLap { get; set; }
        public decimal TongTienPhong { get; set; }
        public decimal TongDichVu { get; set; }
        public decimal TongCong { get; set; }

        public PhieuDatPhong PhieuDatPhong { get; set; }
        public TaiKhoan TaiKhoan { get; set; }
        public KhuyenMai KhuyenMai { get; set; }
    }
}
