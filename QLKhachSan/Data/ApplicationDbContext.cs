using Microsoft.EntityFrameworkCore;
using QLKhachSan.Models;

namespace QLKhachSan.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // 15 DbSets for Hotel Management System
        public DbSet<VaiTro> VaiTros { get; set; } = null!;
        public DbSet<TaiKhoan> TaiKhoans { get; set; } = null!;
        public DbSet<KhachHang> KhachHangs { get; set; } = null!;
        public DbSet<LoaiPhong> LoaiPhongs { get; set; } = null!;
        public DbSet<TienNghi> TienNghis { get; set; } = null!;
        public DbSet<LoaiPhong_TienNghi> LoaiPhong_TienNghis { get; set; } = null!;
        public DbSet<Phong> Phongs { get; set; } = null!;
        public DbSet<PhieuDatPhong> PhieuDatPhongs { get; set; } = null!;
        public DbSet<ChiTietDatPhong> ChiTietDatPhongs { get; set; } = null!;
        public DbSet<LoaiDichVu> LoaiDichVus { get; set; } = null!;
        public DbSet<DichVu> DichVus { get; set; } = null!;
        public DbSet<PhieuSuDungDichVu> PhieuSuDungDichVus { get; set; } = null!;
        public DbSet<ChiTietSuDungDichVu> ChiTietSuDungDichVus { get; set; } = null!;
        public DbSet<KhuyenMai> KhuyenMais { get; set; } = null!;
        public DbSet<HoaDon> HoaDons { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Bỏ pluralize (không tự động thêm 's' vào tên bảng)
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var tableName = entityType.GetTableName();
                if (tableName != null && !tableName.StartsWith("AspNet"))
                {
                    entityType.SetTableName(entityType.DisplayName());
                }
            }

            // Cấu hình khóa chính composite cho bảng trung gian LoaiPhong_TienNghi
            modelBuilder.Entity<LoaiPhong_TienNghi>()
                .HasKey(lt => new { lt.LoaiPhongId, lt.TienNghiId });

            modelBuilder.Entity<LoaiPhong_TienNghi>()
                .HasOne(lt => lt.LoaiPhong)
                .WithMany(lp => lp.LoaiPhong_TienNghis)
                .HasForeignKey(lt => lt.LoaiPhongId);

            modelBuilder.Entity<LoaiPhong_TienNghi>()
                .HasOne(lt => lt.TienNghi)
                .WithMany(tn => tn.LoaiPhong_TienNghis)
                .HasForeignKey(lt => lt.TienNghiId);

            // Cấu hình Restrict Delete để tránh lỗi "multiple cascade paths" trong SQL Server
            modelBuilder.Entity<PhieuDatPhong>()
                .HasOne(p => p.KhachHang)
                .WithMany(k => k.PhieuDatPhongs)
                .HasForeignKey(p => p.KhachHangId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PhieuDatPhong>()
                .HasOne(p => p.TaiKhoan)
                .WithMany(t => t.PhieuDatPhongs)
                .HasForeignKey(p => p.TaiKhoanId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<HoaDon>()
                .HasOne(h => h.TaiKhoan)
                .WithMany(t => t.HoaDons)
                .HasForeignKey(h => h.TaiKhoanId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
