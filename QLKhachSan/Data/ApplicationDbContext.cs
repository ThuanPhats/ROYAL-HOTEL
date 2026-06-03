using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QLKhachSan.Models;

namespace QLKhachSan.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets for Hotel Management System
        public DbSet<HangThanhVien> HangThanhViens { get; set; } = null!;
        public DbSet<KhachHang> KhachHangs { get; set; } = null!;
        public DbSet<SoThichKhachHang> SoThichKhachHangs { get; set; } = null!;
        public DbSet<DanhSachDen> DanhSachDens { get; set; } = null!;
        public DbSet<BoPhan> BoPhans { get; set; } = null!;
        public DbSet<ChucVu> ChucVus { get; set; } = null!;
        public DbSet<NhanVien> NhanViens { get; set; } = null!;
        public DbSet<CaLamViec> CaLamViecs { get; set; } = null!;
        public DbSet<LichLamViec> LichLamViecs { get; set; } = null!;
        public DbSet<LoaiPhong> LoaiPhongs { get; set; } = null!;
        public DbSet<TienNghi> TienNghis { get; set; } = null!;
        public DbSet<Phong> Phongs { get; set; } = null!;
        public DbSet<ChinhSachGia> ChinhSachGias { get; set; } = null!;
        public DbSet<ChinhSachHuy> ChinhSachHuys { get; set; } = null!;
        public DbSet<PhieuDatPhong> PhieuDatPhongs { get; set; } = null!;
        public DbSet<ChiTietDatPhong> ChiTietDatPhongs { get; set; } = null!;
        public DbSet<PhieuLuuTru> PhieuLuuTrus { get; set; } = null!;
        public DbSet<DangKyLuuTru> DangKyLuuTrus { get; set; } = null!;
        public DbSet<LichSuDoiPhong> LichSuDoiPhongs { get; set; } = null!;
        public DbSet<LichSuLuuTru> LichSuLuuTrus { get; set; } = null!;
        public DbSet<DanhMucDichVu> DanhMucDichVus { get; set; } = null!;
        public DbSet<DichVu> DichVus { get; set; } = null!;
        public DbSet<PhieuSuDungDichVu> PhieuSuDungDichVus { get; set; } = null!;
        public DbSet<ChiTietDichVu> ChiTietDichVus { get; set; } = null!;
        public DbSet<ChiTietFolio> ChiTietFolios { get; set; } = null!;
        public DbSet<LoaiThietBi> LoaiThietBis { get; set; } = null!;
        public DbSet<ThietBi> ThietBis { get; set; } = null!;
        public DbSet<PhongThietBi> PhongThietBis { get; set; } = null!;
        public DbSet<LichSuBaoTri> LichSuBaoTris { get; set; } = null!;
        public DbSet<LichBaoTriDinhKy> LichBaoTriDinhKys { get; set; } = null!;
        public DbSet<VatTuTieuHao> VatTuTieuHaos { get; set; } = null!;
        public DbSet<HinhThucThanhToan> HinhThucThanhToans { get; set; } = null!;
        public DbSet<PhieuThanhToan> PhieuThanhToans { get; set; } = null!;
        public DbSet<HoaDon> HoaDons { get; set; } = null!;
        public DbSet<ChiTietHoaDon> ChiTietHoaDons { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Call base method first for Identity tables setup
            base.OnModelCreating(modelBuilder);

            // 1. Many-to-Many Relationship between LoaiPhong and TienNghi
            modelBuilder.Entity<LoaiPhong>()
                .HasMany(lp => lp.TienNghis)
                .WithMany(tn => tn.LoaiPhongs)
                .UsingEntity<Dictionary<string, object>>(
                    "LoaiPhong_TienNghi",
                    r => r.HasOne<TienNghi>().WithMany().HasForeignKey("TienNghiId").OnDelete(DeleteBehavior.Cascade),
                    l => l.HasOne<LoaiPhong>().WithMany().HasForeignKey("LoaiPhongId").OnDelete(DeleteBehavior.Cascade),
                    je =>
                    {
                        je.HasKey("LoaiPhongId", "TienNghiId");
                    });

            // 2. Concurrency Token on Phong (RowVersion)
            modelBuilder.Entity<Phong>()
                .Property(p => p.RowVersion)
                .IsRowVersion();

            // 3. Global Query Filters for Soft Delete
            modelBuilder.Entity<KhachHang>().HasQueryFilter(k => !k.IsDeleted);
            modelBuilder.Entity<NhanVien>().HasQueryFilter(n => !n.IsDeleted);
            modelBuilder.Entity<Phong>().HasQueryFilter(p => !p.IsDeleted);

            // 4. Configure Cascade Deletes / Restrict Behaviors
            // Prevent multiple cascade path errors in SQL Server

            modelBuilder.Entity<KhachHang>()
                .HasOne(k => k.HangThanhVien)
                .WithMany()
                .HasForeignKey(k => k.HangThanhVienId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PhieuDatPhong>()
                .HasOne(p => p.KhachHang)
                .WithMany()
                .HasForeignKey(p => p.KhachHangId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PhieuLuuTru>()
                .HasOne(p => p.KhachHang)
                .WithMany()
                .HasForeignKey(p => p.KhachHangId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PhieuLuuTru>()
                .HasOne(p => p.Phong)
                .WithMany()
                .HasForeignKey(p => p.PhongId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LichSuDoiPhong>()
                .HasOne(l => l.PhongCu)
                .WithMany()
                .HasForeignKey(l => l.PhongCuId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LichSuDoiPhong>()
                .HasOne(l => l.PhongMoi)
                .WithMany()
                .HasForeignKey(l => l.PhongMoiId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LichSuDoiPhong>()
                .HasOne(l => l.NhanVien)
                .WithMany()
                .HasForeignKey(l => l.NhanVienId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PhieuSuDungDichVu>()
                .HasOne(p => p.NhanVien)
                .WithMany()
                .HasForeignKey(p => p.NhanVienId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<HoaDon>()
                .HasOne(h => h.KhachHang)
                .WithMany()
                .HasForeignKey(h => h.KhachHangId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LichSuLuuTru>()
                .HasOne(l => l.KhachHang)
                .WithMany(k => k.LichSuLuuTrus)
                .HasForeignKey(l => l.KhachHangId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LichSuLuuTru>()
                .HasOne(l => l.Phong)
                .WithMany()
                .HasForeignKey(l => l.PhongId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
