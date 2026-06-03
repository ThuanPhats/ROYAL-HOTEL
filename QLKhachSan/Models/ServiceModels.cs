using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLKhachSan.Models
{
    [Table("DanhMucDichVu")]
    public class DanhMucDichVu
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên nhóm dịch vụ không được để trống")]
        [StringLength(100)]
        [Display(Name = "Nhóm dịch vụ")]
        public string TenNhom { get; set; } = string.Empty; // FB, Giặt ủi, Spa, Vận chuyển, Khác

        // Navigation properties
        public virtual ICollection<DichVu> DichVus { get; set; } = new List<DichVu>();
    }

    [Table("DichVu")]
    public class DichVu
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên dịch vụ không được để trống")]
        [StringLength(150)]
        [Display(Name = "Tên dịch vụ")]
        public string TenDichVu { get; set; } = string.Empty;

        [Required(ErrorMessage = "Đơn giá không được để trống")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 1000000000)]
        [Display(Name = "Đơn giá")]
        [DisplayFormat(DataFormatString = "{0:N0} đ")]
        public decimal DonGia { get; set; }

        [Required(ErrorMessage = "Đơn vị tính không được để trống")]
        [StringLength(50)]
        [Display(Name = "ĐVT")]
        public string DonViTinh { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Nhóm dịch vụ")]
        public int DanhMucDichVuId { get; set; }

        [ForeignKey("DanhMucDichVuId")]
        [Display(Name = "Nhóm dịch vụ")]
        public virtual DanhMucDichVu? DanhMucDichVu { get; set; }
    }

    [Table("PhieuSuDungDichVu")]
    public class PhieuSuDungDichVu
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Phiếu lưu trú (Folio)")]
        public int? PhieuLuuTruId { get; set; }

        [ForeignKey("PhieuLuuTruId")]
        [Display(Name = "Mã Folio")]
        public virtual PhieuLuuTru? PhieuLuuTru { get; set; }

        [Display(Name = "Phòng")]
        public int? PhongId { get; set; }

        [ForeignKey("PhongId")]
        [Display(Name = "Phòng")]
        public virtual Phong? Phong { get; set; }

        [Required]
        [Display(Name = "Thời gian gọi")]
        public DateTime NgayTao { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Nhân viên phục vụ")]
        public int NhanVienId { get; set; }

        [ForeignKey("NhanVienId")]
        [Display(Name = "Nhân viên thực hiện")]
        public virtual NhanVien? NhanVien { get; set; }

        [Required]
        [Display(Name = "Thanh toán riêng tại quầy")]
        public bool DaThanhToanRieng { get; set; } = false;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Tổng tiền dịch vụ")]
        [DisplayFormat(DataFormatString = "{0:N0} đ")]
        public decimal TongTien { get; set; } = 0;

        // Navigation properties
        public virtual ICollection<ChiTietDichVu> ChiTietDichVus { get; set; } = new List<ChiTietDichVu>();
    }

    [Table("ChiTietDichVu")]
    public class ChiTietDichVu
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PhieuSuDungDichVuId { get; set; }

        [ForeignKey("PhieuSuDungDichVuId")]
        public virtual PhieuSuDungDichVu? PhieuSuDungDichVu { get; set; }

        [Required]
        [Display(Name = "Dịch vụ")]
        public int DichVuId { get; set; }

        [ForeignKey("DichVuId")]
        [Display(Name = "Dịch vụ")]
        public virtual DichVu? DichVu { get; set; }

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

        [StringLength(250)]
        [Display(Name = "Ghi chú dịch vụ")]
        public string? GhiChu { get; set; } // VD: Tên món ăn, Bàn ăn số mấy, Slot spa...
    }
}
