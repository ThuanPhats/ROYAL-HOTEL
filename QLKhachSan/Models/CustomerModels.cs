using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLKhachSan.Models
{
    [Table("HangThanhVien")]
    public class HangThanhVien
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên hạng thành viên không được để trống")]
        [StringLength(50)]
        [Display(Name = "Hạng thành viên")]
        public string TenHang { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Doanh thu tối thiểu")]
        [DisplayFormat(DataFormatString = "{0:N0} đ")]
        public decimal DoanhThuToiThieu { get; set; }

        [Required]
        [Display(Name = "Số lượt lưu trú tối thiểu")]
        public int SoLuotLuuTruToiThieu { get; set; }

        [Required]
        [Display(Name = "Tỉ lệ giảm giá phòng")]
        public double TiLeGiamGia { get; set; } // Ví dụ: 0.1 tương ứng 10%

        [Required]
        [Display(Name = "Tỷ lệ tích điểm")]
        public double LoyaltyPointsRate { get; set; } // Ví dụ: 0.05 tương ứng 5% tích lũy thành điểm
    }

    [Table("KhachHang")]
    public class KhachHang
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100)]
        [Display(Name = "Họ tên khách")]
        public string HoTen { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [Display(Name = "Ngày sinh")]
        public DateTime? NgaySinh { get; set; }

        [StringLength(10)]
        [Display(Name = "Giới tính")]
        public string? GioiTinh { get; set; }

        [StringLength(50)]
        [Display(Name = "Quốc tịch")]
        public string? QuocTich { get; set; }

        [Required(ErrorMessage = "Số CCCD/Passport không được để trống")]
        [StringLength(50)]
        [Display(Name = "CCCD/Passport")]
        public string CccdPassport { get; set; } = string.Empty;

        [StringLength(20)]
        [Display(Name = "Số điện thoại")]
        public string? Sdt { get; set; }

        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(250)]
        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }

        // Trường thông tin khách doanh nghiệp
        [StringLength(150)]
        [Display(Name = "Tên công ty")]
        public string? TenCongTy { get; set; }

        [StringLength(50)]
        [Display(Name = "Mã số thuế")]
        public string? MstCongTy { get; set; }

        [StringLength(250)]
        [Display(Name = "Địa chỉ công ty")]
        public string? DiaChiCongTy { get; set; }

        [Required]
        [Display(Name = "Điểm tích lũy")]
        public int LoyaltyPoints { get; set; } = 0;

        [Required]
        [Display(Name = "Hạng thành viên")]
        public int HangThanhVienId { get; set; }

        [ForeignKey("HangThanhVienId")]
        [Display(Name = "Hạng thành viên")]
        public virtual HangThanhVien? HangThanhVien { get; set; }

        // Soft delete property
        [Required]
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        public virtual ICollection<SoThichKhachHang> SoThichs { get; set; } = new List<SoThichKhachHang>();
        public virtual ICollection<DanhSachDen> Blacklists { get; set; } = new List<DanhSachDen>();
        public virtual ICollection<LichSuLuuTru> LichSuLuuTrus { get; set; } = new List<LichSuLuuTru>();
    }

    [Table("SoThichKhachHang")]
    public class SoThichKhachHang
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int KhachHangId { get; set; }

        [ForeignKey("KhachHangId")]
        public virtual KhachHang? KhachHang { get; set; }

        [Required(ErrorMessage = "Sở thích không được để trống")]
        [StringLength(250)]
        [Display(Name = "Mô tả sở thích")]
        public string SoThich { get; set; } = string.Empty;
    }

    [Table("DanhSachDen")]
    public class DanhSachDen
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int KhachHangId { get; set; }

        [ForeignKey("KhachHangId")]
        [Display(Name = "Khách hàng")]
        public virtual KhachHang? KhachHang { get; set; }

        [Required(ErrorMessage = "Lý do đưa vào danh sách đen không được để trống")]
        [StringLength(500)]
        [Display(Name = "Lý do")]
        public string LyDo { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Ngày áp dụng")]
        public DateTime NgayApDung { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Đang kích hoạt")]
        public bool IsActive { get; set; } = true;
    }

    [Table("LichSuLuuTru")]
    public class LichSuLuuTru
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int KhachHangId { get; set; }

        [ForeignKey("KhachHangId")]
        public virtual KhachHang? KhachHang { get; set; }

        [Required]
        public int PhongId { get; set; }

        [ForeignKey("PhongId")]
        public virtual Phong? Phong { get; set; }

        [Required]
        [Display(Name = "Ngày nhận phòng")]
        public DateTime NgayCheckIn { get; set; }

        [Required]
        [Display(Name = "Ngày trả phòng")]
        public DateTime NgayCheckOut { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Tổng số tiền")]
        public decimal TongTien { get; set; }
    }
}
