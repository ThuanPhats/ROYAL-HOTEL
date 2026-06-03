using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLKhachSan.Models
{
    [Table("LoaiThietBi")]
    public class LoaiThietBi
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên loại thiết bị không được để trống")]
        [StringLength(100)]
        [Display(Name = "Loại thiết bị")]
        public string TenLoai { get; set; } = string.Empty;

        // Navigation properties
        public virtual ICollection<ThietBi> ThietBis { get; set; } = new List<ThietBi>();
    }

    [Table("ThietBi")]
    public class ThietBi
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Mã tài sản không được để trống")]
        [StringLength(50)]
        [Display(Name = "Mã tài sản")]
        public string MaTaiSan { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên thiết bị không được để trống")]
        [StringLength(150)]
        [Display(Name = "Tên thiết bị/tài sản")]
        public string TenThietBi { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [Display(Name = "Ngày mua")]
        public DateTime? NgayMua { get; set; }

        [StringLength(150)]
        [Display(Name = "Nhà cung cấp")]
        public string? NhaCungCap { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Hạn bảo hành")]
        public DateTime? HanBaoHanh { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = "Tot"; // Tot, CanSuaChua, DaThayThe, ThanhLy

        [Required]
        [Display(Name = "Loại thiết bị")]
        public int LoaiThietBiId { get; set; }

        [ForeignKey("LoaiThietBiId")]
        [Display(Name = "Loại thiết bị")]
        public virtual LoaiThietBi? LoaiThietBi { get; set; }

        // Navigation properties
        public virtual ICollection<PhongThietBi> PhongThietBis { get; set; } = new List<PhongThietBi>();
        public virtual ICollection<LichSuBaoTri> LichSuBaoTris { get; set; } = new List<LichSuBaoTri>();
        public virtual ICollection<LichBaoTriDinhKy> LichBaoTriDinhKys { get; set; } = new List<LichBaoTriDinhKy>();
    }

    [Table("Phong_ThietBi")]
    public class PhongThietBi
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Phòng")]
        public int PhongId { get; set; }

        [ForeignKey("PhongId")]
        [Display(Name = "Phòng")]
        public virtual Phong? Phong { get; set; }

        [Required]
        [Display(Name = "Thiết bị")]
        public int ThietBiId { get; set; }

        [ForeignKey("ThietBiId")]
        [Display(Name = "Thiết bị")]
        public virtual ThietBi? ThietBi { get; set; }

        [Required]
        [Display(Name = "Ngày bàn giao")]
        public DateTime NgayBanGiao { get; set; } = DateTime.Now;

        [Required]
        [Range(1, 1000)]
        [Display(Name = "Số lượng")]
        public int SoLuong { get; set; } = 1;
    }

    [Table("LichSuBaoTri")]
    public class LichSuBaoTri
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Thiết bị")]
        public int ThietBiId { get; set; }

        [ForeignKey("ThietBiId")]
        [Display(Name = "Thiết bị")]
        public virtual ThietBi? ThietBi { get; set; }

        [Required]
        [Display(Name = "Ngày báo hỏng")]
        public DateTime NgayBaoHong { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Mô tả sự cố không được để trống")]
        [StringLength(500)]
        [Display(Name = "Mô tả sự cố")]
        public string MoTaSuCo { get; set; } = string.Empty;

        [Display(Name = "Kỹ thuật viên thực hiện")]
        public int? NhanVienKyThuatId { get; set; }

        [ForeignKey("NhanVienKyThuatId")]
        [Display(Name = "Nhân viên kỹ thuật")]
        public virtual NhanVien? NhanVienKyThuat { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Chi phí")]
        [DisplayFormat(DataFormatString = "{0:N0} đ")]
        public decimal ChiPhi { get; set; } = 0;

        [Display(Name = "Ngày hoàn tất")]
        public DateTime? NgayHoanTat { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Trạng thái sửa chữa")]
        public string TrangThai { get; set; } = "DangXuLy"; // DangXuLy, HoanTat, Huy
    }

    [Table("LichBaoTriDinhKy")]
    public class LichBaoTriDinhKy
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Thiết bị")]
        public int ThietBiId { get; set; }

        [ForeignKey("ThietBiId")]
        [Display(Name = "Thiết bị")]
        public virtual ThietBi? ThietBi { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày bảo trì dự kiến")]
        public DateTime NgayBaoTriDuKien { get; set; }

        [Required]
        [Display(Name = "Chu kỳ bảo trì (ngày)")]
        public int ChuKyBaoTri { get; set; }

        [StringLength(500)]
        [Display(Name = "Nội dung bảo trì")]
        public string? MoTaNoiDung { get; set; }
    }

    [Table("VatTuTieuHao")]
    public class VatTuTieuHao
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên vật tư tiêu hao không được để trống")]
        [StringLength(150)]
        [Display(Name = "Tên vật tư")]
        public string TenVatTu { get; set; } = string.Empty;

        [Required(ErrorMessage = "Đơn vị tính không được để trống")]
        [StringLength(50)]
        [Display(Name = "Đơn vị tính")]
        public string Dvt { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Số lượng tồn")]
        public int SoLuongTon { get; set; } = 0;

        [Required]
        [Display(Name = "Định mức tối thiểu")]
        public int DinhMucToiThieu { get; set; } = 10;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Đơn giá nhập")]
        [DisplayFormat(DataFormatString = "{0:N0} đ")]
        public decimal DonGia { get; set; }
    }
}
