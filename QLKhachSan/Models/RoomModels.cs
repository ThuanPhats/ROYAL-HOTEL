using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLKhachSan.Models
{
    [Table("LoaiPhong")]
    public class LoaiPhong
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên loại phòng không được để trống")]
        [StringLength(100)]
        [Display(Name = "Tên loại phòng")]
        public string TenLoaiPhong { get; set; } = string.Empty;

        [Required(ErrorMessage = "Diện tích không được để trống")]
        [Range(1, 1000, ErrorMessage = "Diện tích phải từ 1 đến 1000 m2")]
        [Display(Name = "Diện tích (m2)")]
        public double DienTich { get; set; }

        [Required(ErrorMessage = "Số giường không được để trống")]
        [Range(1, 10, ErrorMessage = "Số giường phải từ 1 đến 10")]
        [Display(Name = "Số giường")]
        public int SoGiuong { get; set; }

        [Required(ErrorMessage = "Sức chứa không được để trống")]
        [Range(1, 20, ErrorMessage = "Sức chứa phải từ 1 đến 20 người")]
        [Display(Name = "Sức chứa (người)")]
        public int SucChua { get; set; }

        [StringLength(100)]
        [Display(Name = "Hướng nhìn")]
        public string? HuongNhin { get; set; }

        [Required(ErrorMessage = "Giá niêm yết không được để trống")]
        [Range(0, 1000000000, ErrorMessage = "Giá niêm yết không hợp lệ")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Giá niêm yết/đêm")]
        [DisplayFormat(DataFormatString = "{0:N0} đ")]
        public decimal GiaNiemYet { get; set; }

        // Navigation properties
        public virtual ICollection<TienNghi> TienNghis { get; set; } = new List<TienNghi>();
        public virtual ICollection<Phong> Phongs { get; set; } = new List<Phong>();
        public virtual ICollection<ChinhSachGia> ChinhSachGias { get; set; } = new List<ChinhSachGia>();
    }

    [Table("TienNghi")]
    public class TienNghi
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên tiện nghi không được để trống")]
        [StringLength(100)]
        [Display(Name = "Tên tiện nghi")]
        public string TenTienNghi { get; set; } = string.Empty;

        [StringLength(250)]
        [Display(Name = "Mô tả tiện nghi")]
        public string? MoTa { get; set; }

        // Navigation property
        public virtual ICollection<LoaiPhong> LoaiPhongs { get; set; } = new List<LoaiPhong>();
    }

    [Table("Phong")]
    public class Phong
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Mã phòng không được để trống")]
        [StringLength(20)]
        [Display(Name = "Mã phòng")]
        public string MaPhong { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tầng không được để trống")]
        [Display(Name = "Tầng")]
        public int Tang { get; set; }

        [Required(ErrorMessage = "Loại phòng không được để trống")]
        [Display(Name = "Loại phòng")]
        public int LoaiPhongId { get; set; }

        [ForeignKey("LoaiPhongId")]
        [Display(Name = "Loại phòng")]
        public virtual LoaiPhong? LoaiPhong { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Trạng thái phòng")]
        public string TrangThai { get; set; } = "Trong-Sach"; // Trong-Sach, Trong-ChuaDon, DangSuDung, BaoTri, DaDat

        // Concurrency token for Optimistic Concurrency
        [Timestamp]
        public byte[]? RowVersion { get; set; }

        // Soft delete indicator
        [Required]
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        public virtual ICollection<PhongThietBi> PhongThietBis { get; set; } = new List<PhongThietBi>();
    }

    [Table("ChinhSachGia")]
    public class ChinhSachGia
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Loại phòng")]
        public int LoaiPhongId { get; set; }

        [ForeignKey("LoaiPhongId")]
        public virtual LoaiPhong? LoaiPhong { get; set; }

        [Required(ErrorMessage = "Tên chính sách giá không được để trống")]
        [StringLength(100)]
        [Display(Name = "Tên chính sách")]
        public string TenChinhSach { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ngày bắt đầu áp dụng không được để trống")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày bắt đầu")]
        public DateTime NgayBatDau { get; set; }

        [Required(ErrorMessage = "Ngày kết thúc áp dụng không được để trống")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày kết thúc")]
        public DateTime NgayKetThuc { get; set; }

        [Required(ErrorMessage = "Giá áp dụng không được để trống")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 1000000000)]
        [Display(Name = "Giá áp dụng/đêm")]
        [DisplayFormat(DataFormatString = "{0:N0} đ")]
        public decimal GiaApDung { get; set; }

        [Required]
        [Range(1, 30)]
        [Display(Name = "Số đêm tối thiểu")]
        public int SoDemToiThieu { get; set; } = 1;

        [Required]
        [Display(Name = "Kích hoạt")]
        public bool IsActive { get; set; } = true;
    }

    [Table("ChinhSachHuy")]
    public class ChinhSachHuy
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Mô tả chính sách hủy không được để trống")]
        [StringLength(250)]
        [Display(Name = "Mô tả chính sách hủy")]
        public string MoTa { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hạn hủy trước không được để trống")]
        [Display(Name = "Hạn hủy trước (giờ)")]
        public int HanHuyTruoc { get; set; } // số giờ trước check-in

        [Required(ErrorMessage = "Phí hủy không được để trống")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Phí hủy")]
        public decimal PhiHuy { get; set; } // Phí hủy cố định hoặc tỷ lệ
    }
}
