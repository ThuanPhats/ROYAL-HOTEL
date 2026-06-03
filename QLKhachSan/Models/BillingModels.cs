using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLKhachSan.Models
{
    [Table("HinhThucThanhToan")]
    public class HinhThucThanhToan
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên hình thức thanh toán không được để trống")]
        [StringLength(50)]
        [Display(Name = "Hình thức thanh toán")]
        public string TenHinhThuc { get; set; } = string.Empty; // TienMat, ChuyenKhoan, The, ViDienTu, DirectBilling
    }

    [Table("PhieuThanhToan")]
    public class PhieuThanhToan
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Phiếu lưu trú (Folio)")]
        public int? PhieuLuuTruId { get; set; }

        [ForeignKey("PhieuLuuTruId")]
        public virtual PhieuLuuTru? PhieuLuuTru { get; set; }

        [Display(Name = "Phiếu dịch vụ thanh toán riêng")]
        public int? PhieuSuDungDichVuId { get; set; }

        [ForeignKey("PhieuSuDungDichVuId")]
        public virtual PhieuSuDungDichVu? PhieuSuDungDichVu { get; set; }

        [Required]
        [Display(Name = "Ngày thanh toán")]
        public DateTime NgayThanhToan { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Hình thức thanh toán")]
        public int HinhThucThanhToanId { get; set; }

        [ForeignKey("HinhThucThanhToanId")]
        [Display(Name = "Hình thức thanh toán")]
        public virtual HinhThucThanhToan? HinhThucThanhToan { get; set; }

        [Required(ErrorMessage = "Số tiền thanh toán không được để trống")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(1000, 1000000000)]
        [Display(Name = "Số tiền thanh toán")]
        [DisplayFormat(DataFormatString = "{0:N0} đ")]
        public decimal SoTien { get; set; }

        [StringLength(250)]
        [Display(Name = "Ghi chú thanh toán")]
        public string? GhiChu { get; set; }
    }

    [Table("HoaDon")]
    public class HoaDon
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Số hóa đơn không được để trống")]
        [StringLength(50)]
        [Display(Name = "Số hóa đơn")]
        public string SoHoaDon { get; set; } = string.Empty;

        [Display(Name = "Phiếu lưu trú (Folio)")]
        public int? PhieuLuuTruId { get; set; }

        [ForeignKey("PhieuLuuTruId")]
        public virtual PhieuLuuTru? PhieuLuuTru { get; set; }

        [Required]
        [Display(Name = "Khách hàng")]
        public int KhachHangId { get; set; }

        [ForeignKey("KhachHangId")]
        [Display(Name = "Khách hàng")]
        public virtual KhachHang? KhachHang { get; set; }

        [Required]
        [Display(Name = "Ngày xuất hóa đơn")]
        public DateTime NgayXuat { get; set; } = DateTime.Now;

        [StringLength(20)]
        [Display(Name = "Mã số thuế")]
        public string? MST { get; set; }

        [StringLength(150)]
        [Display(Name = "Tên công ty")]
        public string? TenCongTy { get; set; }

        [StringLength(250)]
        [Display(Name = "Địa chỉ công ty")]
        public string? DiaChiCongTy { get; set; }

        [Required]
        [Display(Name = "Thuế suất (%)")]
        public double ThueSuat { get; set; } = 0.08; // 8% mặc định

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Tiền thuế VAT")]
        [DisplayFormat(DataFormatString = "{0:N0} đ")]
        public decimal TienThue { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Tổng số tiền (gồm VAT)")]
        [DisplayFormat(DataFormatString = "{0:N0} đ")]
        public decimal TongTien { get; set; }

        [Required]
        [Display(Name = "Hóa đơn điện tử")]
        public bool IsEInvoice { get; set; } = true;

        // Navigation properties
        public virtual ICollection<ChiTietHoaDon> ChiTietHoaDons { get; set; } = new List<ChiTietHoaDon>();
    }

    [Table("ChiTietHoaDon")]
    public class ChiTietHoaDon
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int HoaDonId { get; set; }

        [ForeignKey("HoaDonId")]
        public virtual HoaDon? HoaDon { get; set; }

        [Required(ErrorMessage = "Tên hạng mục không được để trống")]
        [StringLength(250)]
        [Display(Name = "Tên hạng mục dịch vụ")]
        public string TenMuc { get; set; } = string.Empty;

        [Required]
        [Range(1, 1000)]
        [Display(Name = "Số lượng")]
        public int SoLuong { get; set; } = 1;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Đơn giá")]
        public decimal DonGia { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Thành tiền")]
        public decimal ThanhTien { get; set; }
    }
}
