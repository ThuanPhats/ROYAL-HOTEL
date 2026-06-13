using QLKhachSan.Models;

namespace QLKhachSan.Data
{
    public static class DataSeeder
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // Kiểm tra nếu DB đã có dữ liệu thì không Seed nữa
            if (context.Phongs.Any())
            {
                return;
            }

            // 1. Thêm Loại Phòng
            var standard = new LoaiPhong { TenLoai = "Standard", GiaNgay = 500000, SoNguoiToiDa = 2 };
            var deluxe = new LoaiPhong { TenLoai = "Deluxe", GiaNgay = 1000000, SoNguoiToiDa = 3 };
            var suite = new LoaiPhong { TenLoai = "Suite (VIP)", GiaNgay = 2500000, SoNguoiToiDa = 4 };

            context.LoaiPhongs.AddRange(standard, deluxe, suite);
            context.SaveChanges();

            // 2. Thêm Phòng
            context.Phongs.AddRange(
                new Phong { TenPhong = "P.101", TrangThai = "Trong", LoaiPhongId = standard.Id },
                new Phong { TenPhong = "P.102", TrangThai = "Trong", LoaiPhongId = standard.Id },
                new Phong { TenPhong = "P.103", TrangThai = "Trong", LoaiPhongId = deluxe.Id },
                new Phong { TenPhong = "P.104", TrangThai = "Trong", LoaiPhongId = deluxe.Id },
                new Phong { TenPhong = "P.105", TrangThai = "Trong", LoaiPhongId = suite.Id },
                
                new Phong { TenPhong = "P.201", TrangThai = "Trong", LoaiPhongId = standard.Id },
                new Phong { TenPhong = "P.202", TrangThai = "Trong", LoaiPhongId = standard.Id },
                new Phong { TenPhong = "P.203", TrangThai = "Trong", LoaiPhongId = deluxe.Id },
                new Phong { TenPhong = "P.204", TrangThai = "Trong", LoaiPhongId = deluxe.Id },
                new Phong { TenPhong = "P.205", TrangThai = "Trong", LoaiPhongId = suite.Id }
            );

            // 3. Thêm Loại Dịch Vụ
            var loaiNuoc = new LoaiDichVu { TenLoai = "Đồ uống" };
            var loaiAn = new LoaiDichVu { TenLoai = "Thức ăn" };
            var loaiSpa = new LoaiDichVu { TenLoai = "Chăm sóc sức khỏe" };
            context.LoaiDichVus.AddRange(loaiNuoc, loaiAn, loaiSpa);
            context.SaveChanges();

            // 4. Thêm Dịch Vụ
            context.DichVus.AddRange(
                new DichVu { TenDichVu = "Nước suối Dasani", DonGia = 15000, LoaiDichVuId = loaiNuoc.Id },
                new DichVu { TenDichVu = "Coca Cola", DonGia = 25000, LoaiDichVuId = loaiNuoc.Id },
                new DichVu { TenDichVu = "Bò bít tết", DonGia = 150000, LoaiDichVuId = loaiAn.Id },
                new DichVu { TenDichVu = "Massage Body 60p", DonGia = 500000, LoaiDichVuId = loaiSpa.Id }
            );

            // 5. Thêm Vai trò & Tài khoản (Admin & Lễ Tân)
            var vaiTroAdmin = new VaiTro { TenVaiTro = "Quản lý" };
            var vaiTroLeTan = new VaiTro { TenVaiTro = "Lễ tân" };
            context.VaiTros.AddRange(vaiTroAdmin, vaiTroLeTan);
            context.SaveChanges();

            context.TaiKhoans.AddRange(
                new TaiKhoan { Username = "admin",   PasswordHash = "123456", HoTen = "Quản trị viên", TrangThai = true, VaiTroId = vaiTroAdmin.Id },
                new TaiKhoan { Username = "letan1",  PasswordHash = "123456", HoTen = "Nguyễn Lễ Tân",  TrangThai = true, VaiTroId = vaiTroLeTan.Id }
            );

            context.SaveChanges();
        }
    }
}
