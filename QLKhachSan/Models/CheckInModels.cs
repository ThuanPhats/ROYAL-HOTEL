using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLKhachSan.Models
{
    [Table("PhieuLuuTru")]
    public class PhieuLuuTru
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Mã Folio không được để trống")]
        [StringLength(50)]
        [Display(Name = "Mã Folio")]
        public string MaFolio { get; set; } = string.Empty;

        [Display(Name = "Phiếu đặt phòng")]
        public int? PhieuDatPhongId { get; set; }

        [ForeignKey("PhieuDatPhongId")]
        [Display(Name = "Phiếu đặt phòng")]
        public virtual PhieuDatPhong? PhieuDatPhong { get; set; }

        [Required]
        [Display(Name = "Phòng")]
        public int PhongId { get; set; }

        [ForeignKey("PhongId")]
        [Display(Name = "Phòng")]
        public virtual Phong? Phong { get; set; }

        [Required]
        [Display(Name = "Khách hàng")]
        public int KhachHangId { get; set; }

        [ForeignKey("KhachHangId")]
        [Display(Name = "Khách đại diện")]
        public virtual KhachHang? KhachHang { get; set; }

        [Required]
        [Display(Name = "Giờ nhận phòng thực tế")]
        public DateTime NgayCheckInAct { get; set; } = DateTime.Now;

        [Display(Name = "Giờ trả phòng thực tế")]
        public DateTime? NgayCheckOutAct { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Phụ thu Early Check-in")]
        public decimal PhuThuEarlyCheckIn { get; set; } = 0;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Phụ thu Late Check-out")]
        public decimal PhuThuLateCheckOut { get; set; } = 0;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Giảm trừ")]
        public decimal GiamTru { get; set; } = 0;

        [Required]
        [StringLength(50)]
        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = "DangLuuTru"; // DangLuuTru, DaThanhToan

        // Navigation properties
        public virtual ICollection<DangKyLuuTru> DangKyLuuTrus { get; set; } = new List<DangKyLuuTru>();
        public virtual ICollection<LichSuDoiPhong> LichSuDoiPhongs { get; set; } = new List<LichSuDoiPhong>();
        public virtual ICollection<ChiTietFolio> ChiTietFolios { get; set; } = new List<ChiTietFolio>();
        public virtual ICollection<PhieuSuDungDichVu> PhieuSuDungDichVus { get; set; } = new List<PhieuSuDungDichVu>();
    }

    [Table("DangKyLuuTru")]
    public class DangKyLuuTru
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PhieuLuuTruId { get; set; }

        [ForeignKey("PhieuLuuTruId")]
        public virtual PhieuLuuTru? PhieuLuuTru { get; set; }

        [Required]
        [Display(Name = "Khách hàng lưu trú")]
        public int KhachHangId { get; set; }

        [ForeignKey("KhachHangId")]
        [Display(Name = "Khách hàng")]
        public virtual KhachHang? KhachHang { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày bắt đầu lưu trú")]
        public DateTime NgayBatDau { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày kết thúc lưu trú")]
        public DateTime NgayKetThuc { get; set; }
    }

    [Table("LichSuDoiPhong")]
    public class LichSuDoiPhong
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PhieuLuuTruId { get; set; }

        [ForeignKey("PhieuLuuTruId")]
        public virtual PhieuLuuTru? PhieuLuuTru { get; set; }

        [Required]
        [Display(Name = "Phòng cũ")]
        public int PhongCuId { get; set; }

        [ForeignKey("PhongCuId")]
        public virtual Phong? PhongCu { get; set; }

        [Required]
        [Display(Name = "Phòng mới")]
        public int PhongMoiId { get; set; }

        [ForeignKey("PhongMoiId")]
        public virtual Phong? PhongMoi { get; set; }

        [Required]
        [Display(Name = "Ngày đổi")]
        public DateTime NgayDoi { get; set; } = DateTime.Now;

        [StringLength(250)]
        [Display(Name = "Lý do đổi")]
        public string? LyDo { get; set; }

        [Required]
        [Display(Name = "Nhân viên thực hiện")]
        public int NhanVienId { get; set; }

        [ForeignKey("NhanVienId")]
        public virtual NhanVien? NhanVien { get; set; }
    }

    [Table("ChiTietFolio")]
    public class ChiTietFolio
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PhieuLuuTruId { get; set; }

        [ForeignKey("PhieuLuuTruId")]
        public virtual PhieuLuuTru? PhieuLuuTru { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Loại chi phí")]
        public string LoaiChiTiet { get; set; } = "TienPhong"; // TienPhong, PhuThu, DichVu, GiamTru

        [Required]
        [StringLength(250)]
        [Display(Name = "Nội dung")]
        public string NoiDung { get; set; } = string.Empty;

        [Required]
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

        [Required]
        [Display(Name = "Ngày ghi nhận")]
        public DateTime NgayGhiNhan { get; set; } = DateTime.Now;
    }
}
