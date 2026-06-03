using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLKhachSan.Models
{
    [Table("BoPhan")]
    public class BoPhan
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên bộ phận không được để trống")]
        [StringLength(100)]
        [Display(Name = "Bộ phận")]
        public string TenBoPhan { get; set; } = string.Empty;

        // Navigation properties
        public virtual ICollection<ChucVu> ChucVus { get; set; } = new List<ChucVu>();
        public virtual ICollection<NhanVien> NhanViens { get; set; } = new List<NhanVien>();
    }

    [Table("ChucVu")]
    public class ChucVu
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên chức vụ không được để trống")]
        [StringLength(100)]
        [Display(Name = "Chức vụ")]
        public string TenChucVu { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Bộ phận")]
        public int BoPhanId { get; set; }

        [ForeignKey("BoPhanId")]
        [Display(Name = "Bộ phận")]
        public virtual BoPhan? BoPhan { get; set; }

        // Navigation properties
        public virtual ICollection<NhanVien> NhanViens { get; set; } = new List<NhanVien>();
    }

    [Table("NhanVien")]
    public class NhanVien
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Mã nhân viên không được để trống")]
        [StringLength(20)]
        [Display(Name = "Mã NV")]
        public string MaNV { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100)]
        [Display(Name = "Họ tên")]
        public string HoTen { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [Display(Name = "Ngày sinh")]
        public DateTime? NgaySinh { get; set; }

        [Required(ErrorMessage = "Số CCCD không được để trống")]
        [StringLength(50)]
        [Display(Name = "Số CCCD")]
        public string Cccd { get; set; } = string.Empty;

        [StringLength(20)]
        [Display(Name = "Số điện thoại")]
        public string? Sdt { get; set; }

        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(100)]
        public string? Email { get; set; }

        [Required]
        [Display(Name = "Bộ phận")]
        public int BoPhanId { get; set; }

        [ForeignKey("BoPhanId")]
        [Display(Name = "Bộ phận")]
        public virtual BoPhan? BoPhan { get; set; }

        [Required]
        [Display(Name = "Chức vụ")]
        public int ChucVuId { get; set; }

        [ForeignKey("ChucVuId")]
        [Display(Name = "Chức vụ")]
        public virtual ChucVu? ChucVu { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày vào làm")]
        public DateTime NgayVaoLam { get; set; } = DateTime.Now;

        [Required]
        public bool IsDeleted { get; set; } = false;

        // Identity link
        [StringLength(450)]
        public string? AppUserId { get; set; }

        // Navigation properties
        public virtual ICollection<LichLamViec> LichLamViecs { get; set; } = new List<LichLamViec>();
    }

    [Table("CaLamViec")]
    public class CaLamViec
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên ca làm việc không được để trống")]
        [StringLength(50)]
        [Display(Name = "Tên ca")]
        public string TenCa { get; set; } = string.Empty; // Ca sáng, Ca chiều, Ca đêm

        [Required]
        [DataType(DataType.Time)]
        [Display(Name = "Giờ bắt đầu")]
        public TimeSpan GioBatDau { get; set; }

        [Required]
        [DataType(DataType.Time)]
        [Display(Name = "Giờ kết thúc")]
        public TimeSpan GioKetThuc { get; set; }

        // Navigation properties
        public virtual ICollection<LichLamViec> LichLamViecs { get; set; } = new List<LichLamViec>();
    }

    [Table("LichLamViec")]
    public class LichLamViec
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Nhân viên")]
        public int NhanVienId { get; set; }

        [ForeignKey("NhanVienId")]
        [Display(Name = "Nhân viên")]
        public virtual NhanVien? NhanVien { get; set; }

        [Required]
        [Display(Name = "Ca làm việc")]
        public int CaLamViecId { get; set; }

        [ForeignKey("CaLamViecId")]
        [Display(Name = "Ca làm việc")]
        public virtual CaLamViec? CaLamViec { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày làm việc")]
        public DateTime NgayLamViec { get; set; }

        [Display(Name = "Giờ chấm công vào")]
        public DateTime? CheckInTime { get; set; }

        [Display(Name = "Giờ chấm công ra")]
        public DateTime? CheckOutTime { get; set; }

        [StringLength(50)]
        [Display(Name = "Trạng thái chấm công")]
        public string? TrangThaiChambCong { get; set; } // DungGio, Tre, Som, Vang
    }
}
