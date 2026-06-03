using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLKhachSan.Models
{
    [Table("PhieuDatPhong")]
    public class PhieuDatPhong
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Mã đặt phòng không được để trống")]
        [StringLength(50)]
        [Display(Name = "Mã đặt phòng")]
        public string MaDatPhong { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Khách hàng")]
        public int KhachHangId { get; set; }

        [ForeignKey("KhachHangId")]
        [Display(Name = "Khách hàng")]
        public virtual KhachHang? KhachHang { get; set; }

        [Required]
        [Display(Name = "Ngày tạo phiếu")]
        public DateTime NgayTao { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Ngày check-in không được để trống")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày Check-in")]
        public DateTime NgayCheckIn { get; set; }

        [Required(ErrorMessage = "Ngày check-out không được để trống")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày Check-out")]
        public DateTime NgayCheckOut { get; set; }

        [Required]
        [Range(1, 100)]
        [Display(Name = "Số lượng khách")]
        public int SoLuongKhach { get; set; } = 1;

        [StringLength(500)]
        [Display(Name = "Yêu cầu đặc biệt")]
        public string? YeuCauDacBiet { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Trạng thái đặt phòng")]
        public string TrangThai { get; set; } = "ChoXacNhan"; // ChoXacNhan, DaXacNhan, DaNhanPhong, DaHuy, NoShow

        [Display(Name = "Chính sách hủy")]
        public int? ChinhSachHuyId { get; set; }

        [ForeignKey("ChinhSachHuyId")]
        [Display(Name = "Chính sách hủy")]
        public virtual ChinhSachHuy? ChinhSachHuy { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Tổng tiền dự kiến")]
        [DisplayFormat(DataFormatString = "{0:N0} đ")]
        public decimal TongTienDuKien { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Tiền đặt cọc")]
        [DisplayFormat(DataFormatString = "{0:N0} đ")]
        public decimal TienDatCoc { get; set; } = 0;

        // Navigation properties
        public virtual ICollection<ChiTietDatPhong> ChiTietDatPhongs { get; set; } = new List<ChiTietDatPhong>();
        public virtual ICollection<PhieuLuuTru> PhieuLuuTrus { get; set; } = new List<PhieuLuuTru>();
    }

    [Table("ChiTietDatPhong")]
    public class ChiTietDatPhong
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PhieuDatPhongId { get; set; }

        [ForeignKey("PhieuDatPhongId")]
        public virtual PhieuDatPhong? PhieuDatPhong { get; set; }

        [Required]
        [Display(Name = "Loại phòng")]
        public int LoaiPhongId { get; set; }

        [ForeignKey("LoaiPhongId")]
        [Display(Name = "Loại phòng")]
        public virtual LoaiPhong? LoaiPhong { get; set; }

        [Required]
        [Range(1, 100)]
        [Display(Name = "Số lượng phòng")]
        public int SoLuongPhong { get; set; } = 1;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Giá thỏa thuận/đêm")]
        [DisplayFormat(DataFormatString = "{0:N0} đ")]
        public decimal GiaThoaThuan { get; set; }
    }
}
