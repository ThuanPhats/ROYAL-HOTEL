-- CREATE DATABASE
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'QLKhachSanDb')
BEGIN
    CREATE DATABASE QLKhachSanDb;
END
GO

USE QLKhachSanDb;
GO

-- XÓA BẢNG NẾU TỒN TẠI (Theo thứ tự khóa ngoại trước)
IF OBJECT_ID('ChiTietHoaDon', 'U') IS NOT NULL DROP TABLE ChiTietHoaDon;
IF OBJECT_ID('HoaDon', 'U') IS NOT NULL DROP TABLE HoaDon;
IF OBJECT_ID('PhieuThanhToan', 'U') IS NOT NULL DROP TABLE PhieuThanhToan;
IF OBJECT_ID('HinhThucThanhToan', 'U') IS NOT NULL DROP TABLE HinhThucThanhToan;
IF OBJECT_ID('LichBaoTriDinhKy', 'U') IS NOT NULL DROP TABLE LichBaoTriDinhKy;
IF OBJECT_ID('LichSuBaoTri', 'U') IS NOT NULL DROP TABLE LichSuBaoTri;
IF OBJECT_ID('Phong_ThietBi', 'U') IS NOT NULL DROP TABLE Phong_ThietBi;
IF OBJECT_ID('ThietBi', 'U') IS NOT NULL DROP TABLE ThietBi;
IF OBJECT_ID('LoaiThietBi', 'U') IS NOT NULL DROP TABLE LoaiThietBi;
IF OBJECT_ID('VatTuTieuHao', 'U') IS NOT NULL DROP TABLE VatTuTieuHao;
IF OBJECT_ID('ChiTietDichVu', 'U') IS NOT NULL DROP TABLE ChiTietDichVu;
IF OBJECT_ID('PhieuSuDungDichVu', 'U') IS NOT NULL DROP TABLE PhieuSuDungDichVu;
IF OBJECT_ID('DichVu', 'U') IS NOT NULL DROP TABLE DichVu;
IF OBJECT_ID('DanhMucDichVu', 'U') IS NOT NULL DROP TABLE DanhMucDichVu;
IF OBJECT_ID('LichSuDoiPhong', 'U') IS NOT NULL DROP TABLE LichSuDoiPhong;
IF OBJECT_ID('DangKyLuuTru', 'U') IS NOT NULL DROP TABLE DangKyLuuTru;
IF OBJECT_ID('ChiTietFolio', 'U') IS NOT NULL DROP TABLE ChiTietFolio;
IF OBJECT_ID('PhieuLuuTru', 'U') IS NOT NULL DROP TABLE PhieuLuuTru;
IF OBJECT_ID('LichSuLuuTru', 'U') IS NOT NULL DROP TABLE LichSuLuuTru;
IF OBJECT_ID('ChiTietDatPhong', 'U') IS NOT NULL DROP TABLE ChiTietDatPhong;
IF OBJECT_ID('PhieuDatPhong', 'U') IS NOT NULL DROP TABLE PhieuDatPhong;
IF OBJECT_ID('ChinhSachHuy', 'U') IS NOT NULL DROP TABLE ChinhSachHuy;
IF OBJECT_ID('ChinhSachGia', 'U') IS NOT NULL DROP TABLE ChinhSachGia;
IF OBJECT_ID('Phong', 'U') IS NOT NULL DROP TABLE Phong;
IF OBJECT_ID('LoaiPhong_TienNghi', 'U') IS NOT NULL DROP TABLE LoaiPhong_TienNghi;
IF OBJECT_ID('TienNghi', 'U') IS NOT NULL DROP TABLE TienNghi;
IF OBJECT_ID('LoaiPhong', 'U') IS NOT NULL DROP TABLE LoaiPhong;
IF OBJECT_ID('LichLamViec', 'U') IS NOT NULL DROP TABLE LichLamViec;
IF OBJECT_ID('CaLamViec', 'U') IS NOT NULL DROP TABLE CaLamViec;
IF OBJECT_ID('NhanVien', 'U') IS NOT NULL DROP TABLE NhanVien;
IF OBJECT_ID('ChucVu', 'U') IS NOT NULL DROP TABLE ChucVu;
IF OBJECT_ID('BoPhan', 'U') IS NOT NULL DROP TABLE BoPhan;
IF OBJECT_ID('DanhSachDen', 'U') IS NOT NULL DROP TABLE DanhSachDen;
IF OBJECT_ID('SoThichKhachHang', 'U') IS NOT NULL DROP TABLE SoThichKhachHang;
IF OBJECT_ID('KhachHang', 'U') IS NOT NULL DROP TABLE KhachHang;
IF OBJECT_ID('HangThanhVien', 'U') IS NOT NULL DROP TABLE HangThanhVien;

-- BẢNG IDENTITY CỦA ASP.NET CORE IDENTITY (Nếu chưa có)
IF OBJECT_ID('AspNetUserRoles', 'U') IS NOT NULL DROP TABLE AspNetUserRoles;
IF OBJECT_ID('AspNetUserClaims', 'U') IS NOT NULL DROP TABLE AspNetUserClaims;
IF OBJECT_ID('AspNetUserLogins', 'U') IS NOT NULL DROP TABLE AspNetUserLogins;
IF OBJECT_ID('AspNetUserTokens', 'U') IS NOT NULL DROP TABLE AspNetUserTokens;
IF OBJECT_ID('AspNetRoleClaims', 'U') IS NOT NULL DROP TABLE AspNetRoleClaims;
IF OBJECT_ID('AspNetRoles', 'U') IS NOT NULL DROP TABLE AspNetRoles;
IF OBJECT_ID('AspNetUsers', 'U') IS NOT NULL DROP TABLE AspNetUsers;

--------------------------------------------------------------------------------
-- 1. BẢNG ASP.NET CORE IDENTITY
--------------------------------------------------------------------------------
CREATE TABLE AspNetUsers (
    Id NVARCHAR(450) NOT NULL PRIMARY KEY,
    UserName NVARCHAR(256) NULL,
    NormalizedUserName NVARCHAR(256) NULL,
    Email NVARCHAR(256) NULL,
    NormalizedEmail NVARCHAR(256) NULL,
    EmailConfirmed BIT NOT NULL,
    PasswordHash NVARCHAR(MAX) NULL,
    SecurityStamp NVARCHAR(MAX) NULL,
    ConcurrencyStamp NVARCHAR(MAX) NULL,
    PhoneNumber NVARCHAR(MAX) NULL,
    PhoneNumberConfirmed BIT NOT NULL,
    TwoFactorEnabled BIT NOT NULL,
    LockoutEnd DATETIMEOFFSET(7) NULL,
    LockoutEnabled BIT NOT NULL,
    AccessFailedCount INT NOT NULL
);

CREATE TABLE AspNetRoles (
    Id NVARCHAR(450) NOT NULL PRIMARY KEY,
    Name NVARCHAR(256) NULL,
    NormalizedName NVARCHAR(256) NULL,
    ConcurrencyStamp NVARCHAR(MAX) NULL
);

CREATE TABLE AspNetRoleClaims (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    RoleId NVARCHAR(450) NOT NULL FOREIGN KEY REFERENCES AspNetRoles(Id) ON DELETE CASCADE,
    ClaimType NVARCHAR(MAX) NULL,
    ClaimValue NVARCHAR(MAX) NULL
);

CREATE TABLE AspNetUserClaims (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UserId NVARCHAR(450) NOT NULL FOREIGN KEY REFERENCES AspNetUsers(Id) ON DELETE CASCADE,
    ClaimType NVARCHAR(MAX) NULL,
    ClaimValue NVARCHAR(MAX) NULL
);

CREATE TABLE AspNetUserLogins (
    LoginProvider NVARCHAR(450) NOT NULL,
    ProviderKey NVARCHAR(450) NOT NULL,
    ProviderDisplayName NVARCHAR(MAX) NULL,
    UserId NVARCHAR(450) NOT NULL FOREIGN KEY REFERENCES AspNetUsers(Id) ON DELETE CASCADE,
    PRIMARY KEY (LoginProvider, ProviderKey)
);

CREATE TABLE AspNetUserRoles (
    UserId NVARCHAR(450) NOT NULL FOREIGN KEY REFERENCES AspNetUsers(Id) ON DELETE CASCADE,
    RoleId NVARCHAR(450) NOT NULL FOREIGN KEY REFERENCES AspNetRoles(Id) ON DELETE CASCADE,
    PRIMARY KEY (UserId, RoleId)
);

CREATE TABLE AspNetUserTokens (
    UserId NVARCHAR(450) NOT NULL FOREIGN KEY REFERENCES AspNetUsers(Id) ON DELETE CASCADE,
    LoginProvider NVARCHAR(450) NOT NULL,
    Name NVARCHAR(450) NOT NULL,
    Value NVARCHAR(MAX) NULL,
    PRIMARY KEY (UserId, LoginProvider, Name)
);

--------------------------------------------------------------------------------
-- 2. CÁC BẢNG NGHIỆP VỤ KHÁCH SẠN
--------------------------------------------------------------------------------

-- Hạng thành viên
CREATE TABLE HangThanhVien (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    TenHang NVARCHAR(50) NOT NULL,
    DoanhThuToiThieu DECIMAL(18,2) NOT NULL,
    SoLuotLuuTruToiThieu INT NOT NULL,
    TiLeGiamGia FLOAT NOT NULL DEFAULT 0,
    LoyaltyPointsRate FLOAT NOT NULL DEFAULT 0 -- Tỷ lệ tích điểm
);

-- Khách hàng
CREATE TABLE KhachHang (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    HoTen NVARCHAR(100) NOT NULL,
    NgaySinh DATETIME NULL,
    GioiTinh NVARCHAR(10) NULL,
    QuocTich NVARCHAR(50) NULL,
    CccdPassport NVARCHAR(50) NOT NULL UNIQUE,
    Sdt NVARCHAR(20) NULL,
    Email NVARCHAR(100) NULL,
    DiaChi NVARCHAR(250) NULL,
    TenCongTy NVARCHAR(150) NULL,
    MstCongTy NVARCHAR(50) NULL,
    DiaChiCongTy NVARCHAR(250) NULL,
    LoyaltyPoints INT NOT NULL DEFAULT 0,
    HangThanhVienId INT NOT NULL FOREIGN KEY REFERENCES HangThanhVien(Id),
    IsDeleted BIT NOT NULL DEFAULT 0
);

-- Sở thích khách hàng
CREATE TABLE SoThichKhachHang (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    KhachHangId INT NOT NULL FOREIGN KEY REFERENCES KhachHang(Id) ON DELETE CASCADE,
    SoThich NVARCHAR(250) NOT NULL
);

-- Danh sách đen (Blacklist)
CREATE TABLE DanhSachDen (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    KhachHangId INT NOT NULL FOREIGN KEY REFERENCES KhachHang(Id) ON DELETE CASCADE,
    LyDo NVARCHAR(500) NOT NULL,
    NgayApDung DATETIME NOT NULL DEFAULT GETDATE(),
    IsActive BIT NOT NULL DEFAULT 1
);

-- Bộ phận nhân viên
CREATE TABLE BoPhan (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    TenBoPhan NVARCHAR(100) NOT NULL
);

-- Chức vụ
CREATE TABLE ChucVu (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    TenChucVu NVARCHAR(100) NOT NULL,
    BoPhanId INT NOT NULL FOREIGN KEY REFERENCES BoPhan(Id) ON DELETE CASCADE
);

-- Nhân viên
CREATE TABLE NhanVien (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    MaNV NVARCHAR(20) NOT NULL UNIQUE,
    HoTen NVARCHAR(100) NOT NULL,
    NgaySinh DATETIME NULL,
    Cccd NVARCHAR(50) NOT NULL UNIQUE,
    Sdt NVARCHAR(20) NULL,
    Email NVARCHAR(100) NULL,
    BoPhanId INT NOT NULL FOREIGN KEY REFERENCES BoPhan(Id),
    ChucVuId INT NOT NULL FOREIGN KEY REFERENCES ChucVu(Id),
    NgayVaoLam DATETIME NOT NULL DEFAULT GETDATE(),
    IsDeleted BIT NOT NULL DEFAULT 0,
    AppUserId NVARCHAR(450) NULL FOREIGN KEY REFERENCES AspNetUsers(Id) ON DELETE SET NULL
);

-- Ca làm việc
CREATE TABLE CaLamViec (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    TenCa NVARCHAR(50) NOT NULL, -- Ca sáng, Ca chiều, Ca đêm
    GioBatDau TIME NOT NULL,
    GioKetThuc TIME NOT NULL
);

-- Lịch làm việc và Chấm công
CREATE TABLE LichLamViec (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    NhanVienId INT NOT NULL FOREIGN KEY REFERENCES NhanVien(Id) ON DELETE CASCADE,
    CaLamViecId INT NOT NULL FOREIGN KEY REFERENCES CaLamViec(Id) ON DELETE CASCADE,
    NgayLamViec DATE NOT NULL,
    CheckInTime DATETIME NULL,
    CheckOutTime DATETIME NULL,
    TrangThaiChambCong NVARCHAR(50) NULL -- DungGio, Tre, Som, Vang
);

-- Loại phòng
CREATE TABLE LoaiPhong (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    TenLoaiPhong NVARCHAR(100) NOT NULL,
    DienTich FLOAT NOT NULL,
    SoGiuong INT NOT NULL,
    SucChua INT NOT NULL,
    HuongNhin NVARCHAR(100) NULL,
    GiaNiemYet DECIMAL(18,2) NOT NULL
);

-- Tiện nghi
CREATE TABLE TienNghi (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    TenTienNghi NVARCHAR(100) NOT NULL,
    MoTa NVARCHAR(250) NULL
);

-- Bảng nhiều-nhiều Loại phòng & Tiện nghi
CREATE TABLE LoaiPhong_TienNghi (
    LoaiPhongId INT NOT NULL FOREIGN KEY REFERENCES LoaiPhong(Id) ON DELETE CASCADE,
    TienNghiId INT NOT NULL FOREIGN KEY REFERENCES TienNghi(Id) ON DELETE CASCADE,
    PRIMARY KEY (LoaiPhongId, TienNghiId)
);

-- Phòng
CREATE TABLE Phong (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    MaPhong NVARCHAR(20) NOT NULL UNIQUE, -- VD: P101, P102...
    Tang INT NOT NULL,
    LoaiPhongId INT NOT NULL FOREIGN KEY REFERENCES LoaiPhong(Id) ON DELETE CASCADE,
    TrangThai NVARCHAR(50) NOT NULL DEFAULT 'Trong-Sach', -- Trong-Sach, Trong-ChuaDon, DangSuDung, BaoTri, DaDat
    IsDeleted BIT NOT NULL DEFAULT 0,
    RowVersion ROWVERSION NOT NULL
);

-- Chính sách giá phòng (Rate Plan)
CREATE TABLE ChinhSachGia (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    LoaiPhongId INT NOT NULL FOREIGN KEY REFERENCES LoaiPhong(Id) ON DELETE CASCADE,
    TenChinhSach NVARCHAR(100) NOT NULL,
    NgayBatDau DATETIME NOT NULL,
    NgayKetThuc DATETIME NOT NULL,
    GiaApDung DECIMAL(18,2) NOT NULL,
    SoDemToiThieu INT NOT NULL DEFAULT 1,
    IsActive BIT NOT NULL DEFAULT 1
);

-- Chính sách hủy đặt phòng
CREATE TABLE ChinhSachHuy (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    MoTa NVARCHAR(250) NOT NULL,
    HanHuyTruoc INT NOT NULL, -- số giờ trước CheckIn
    PhiHuy DECIMAL(18,2) NOT NULL -- số tiền hoặc tỷ lệ
);

-- Phiếu đặt phòng (Reservation)
CREATE TABLE PhieuDatPhong (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    MaDatPhong NVARCHAR(50) NOT NULL UNIQUE,
    KhachHangId INT NOT NULL FOREIGN KEY REFERENCES KhachHang(Id),
    NgayTao DATETIME NOT NULL DEFAULT GETDATE(),
    NgayCheckIn DATETIME NOT NULL,
    NgayCheckOut DATETIME NOT NULL,
    SoLuongKhach INT NOT NULL DEFAULT 1,
    YeuCauDacBiet NVARCHAR(500) NULL,
    TrangThai NVARCHAR(50) NOT NULL DEFAULT 'ChoXacNhan', -- ChoXacNhan, DaXacNhan, DaNhanPhong, DaHuy, NoShow
    ChinhSachHuyId INT NULL FOREIGN KEY REFERENCES ChinhSachHuy(Id),
    TongTienDuKien DECIMAL(18,2) NOT NULL,
    TienDatCoc DECIMAL(18,2) NOT NULL DEFAULT 0
);

-- Chi tiết đặt phòng
CREATE TABLE ChiTietDatPhong (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    PhieuDatPhongId INT NOT NULL FOREIGN KEY REFERENCES PhieuDatPhong(Id) ON DELETE CASCADE,
    LoaiPhongId INT NOT NULL FOREIGN KEY REFERENCES LoaiPhong(Id),
    SoLuongPhong INT NOT NULL DEFAULT 1,
    GiaThoaThuan DECIMAL(18,2) NOT NULL
);

-- Phiếu lưu trú (Folio)
CREATE TABLE PhieuLuuTru (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    MaFolio NVARCHAR(50) NOT NULL UNIQUE,
    PhieuDatPhongId INT NULL FOREIGN KEY REFERENCES PhieuDatPhong(Id) ON DELETE SET NULL,
    PhongId INT NOT NULL FOREIGN KEY REFERENCES Phong(Id),
    KhachHangId INT NOT NULL FOREIGN KEY REFERENCES KhachHang(Id),
    NgayCheckInAct DATETIME NOT NULL DEFAULT GETDATE(),
    NgayCheckOutAct DATETIME NULL,
    PhuThuEarlyCheckIn DECIMAL(18,2) NOT NULL DEFAULT 0,
    PhuThuLateCheckOut DECIMAL(18,2) NOT NULL DEFAULT 0,
    GiamTru DECIMAL(18,2) NOT NULL DEFAULT 0,
    TrangThai NVARCHAR(50) NOT NULL DEFAULT 'DangLuuTru' -- DangLuuTru, DaThanhToan
);

-- Đăng ký lưu trú (Danh sách những người ở trong phòng)
CREATE TABLE DangKyLuuTru (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    PhieuLuuTruId INT NOT NULL FOREIGN KEY REFERENCES PhieuLuuTru(Id) ON DELETE CASCADE,
    KhachHangId INT NOT NULL FOREIGN KEY REFERENCES KhachHang(Id),
    NgayBatDau DATETIME NOT NULL,
    NgayKetThuc DATETIME NOT NULL
);

-- Lịch sử đổi phòng
CREATE TABLE LichSuDoiPhong (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    PhieuLuuTruId INT NOT NULL FOREIGN KEY REFERENCES PhieuLuuTru(Id) ON DELETE CASCADE,
    PhongCuId INT NOT NULL FOREIGN KEY REFERENCES Phong(Id),
    PhongMoiId INT NOT NULL FOREIGN KEY REFERENCES Phong(Id),
    NgayDoi DATETIME NOT NULL DEFAULT GETDATE(),
    LyDo NVARCHAR(250) NULL,
    NhanVienId INT NOT NULL FOREIGN KEY REFERENCES NhanVien(Id)
);

-- Lịch sử lưu trú tổng hợp (để nâng hạng nhanh)
CREATE TABLE LichSuLuuTru (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    KhachHangId INT NOT NULL FOREIGN KEY REFERENCES KhachHang(Id) ON DELETE CASCADE,
    PhongId INT NOT NULL FOREIGN KEY REFERENCES Phong(Id),
    NgayCheckIn DATETIME NOT NULL,
    NgayCheckOut DATETIME NOT NULL,
    TongTien DECIMAL(18,2) NOT NULL
);

-- Danh mục nhóm dịch vụ
CREATE TABLE DanhMucDichVu (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    TenNhom NVARCHAR(100) NOT NULL -- FB, Giặt ủi, Spa & Wellness, Vận chuyển, Khác
);

-- Dịch vụ cụ thể
CREATE TABLE DichVu (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    TenDichVu NVARCHAR(150) NOT NULL,
    DonGia DECIMAL(18,2) NOT NULL,
    DonViTinh NVARCHAR(50) NOT NULL,
    DanhMucDichVuId INT NOT NULL FOREIGN KEY REFERENCES DanhMucDichVu(Id) ON DELETE CASCADE
);

-- Phiếu sử dụng dịch vụ
CREATE TABLE PhieuSuDungDichVu (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    PhieuLuuTruId INT NULL FOREIGN KEY REFERENCES PhieuLuuTru(Id) ON DELETE SET NULL,
    PhongId INT NULL FOREIGN KEY REFERENCES Phong(Id),
    NgayTao DATETIME NOT NULL DEFAULT GETDATE(),
    NhanVienId INT NOT NULL FOREIGN KEY REFERENCES NhanVien(Id),
    DaThanhToanRieng BIT NOT NULL DEFAULT 0,
    TongTien DECIMAL(18,2) NOT NULL DEFAULT 0
);

-- Chi tiết dịch vụ
CREATE TABLE ChiTietDichVu (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    PhieuSuDungDichVuId INT NOT NULL FOREIGN KEY REFERENCES PhieuSuDungDichVu(Id) ON DELETE CASCADE,
    DichVuId INT NOT NULL FOREIGN KEY REFERENCES DichVu(Id),
    SoLuong INT NOT NULL DEFAULT 1,
    DonGia DECIMAL(18,2) NOT NULL,
    ThanhTien DECIMAL(18,2) NOT NULL,
    GhiChu NVARCHAR(250) NULL -- Thời gian slot, số bàn dọn ăn, nhân viên trị liệu spa...
);

-- Chi tiết các giao dịch phụ thuộc vào Folio (Tiền phòng, phụ thu, dịch vụ)
CREATE TABLE ChiTietFolio (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    PhieuLuuTruId INT NOT NULL FOREIGN KEY REFERENCES PhieuLuuTru(Id) ON DELETE CASCADE,
    LoaiChiTiet NVARCHAR(50) NOT NULL, -- TienPhong, PhuThu, DichVu, GiamTru
    NoiDung NVARCHAR(250) NOT NULL,
    SoLuong INT NOT NULL DEFAULT 1,
    DonGia DECIMAL(18,2) NOT NULL,
    ThanhTien DECIMAL(18,2) NOT NULL,
    NgayGhiNhan DATETIME NOT NULL DEFAULT GETDATE()
);

-- Loại thiết bị
CREATE TABLE LoaiThietBi (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    TenLoai NVARCHAR(100) NOT NULL
);

-- Thiết bị cố định
CREATE TABLE ThietBi (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    MaTaiSan NVARCHAR(50) NOT NULL UNIQUE,
    TenThietBi NVARCHAR(150) NOT NULL,
    NgayMua DATETIME NULL,
    NhaCungCap NVARCHAR(150) NULL,
    HanBaoHanh DATETIME NULL,
    TrangThai NVARCHAR(50) NOT NULL DEFAULT 'Tot', -- Tot, CanSuaChua, DaThayThe, ThanhLy
    LoaiThietBiId INT NOT NULL FOREIGN KEY REFERENCES LoaiThietBi(Id)
);

-- Phân bổ thiết bị trong phòng
CREATE TABLE Phong_ThietBi (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    PhongId INT NOT NULL FOREIGN KEY REFERENCES Phong(Id) ON DELETE CASCADE,
    ThietBiId INT NOT NULL FOREIGN KEY REFERENCES ThietBi(Id) ON DELETE CASCADE,
    NgayBanGiao DATETIME NOT NULL DEFAULT GETDATE(),
    SoLuong INT NOT NULL DEFAULT 1
);

-- Lịch sử bảo trì thiết bị
CREATE TABLE LichSuBaoTri (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    ThietBiId INT NOT NULL FOREIGN KEY REFERENCES ThietBi(Id) ON DELETE CASCADE,
    NgayBaoHong DATETIME NOT NULL DEFAULT GETDATE(),
    MoTaSuCo NVARCHAR(500) NOT NULL,
    NhanVienKyThuatId INT NULL FOREIGN KEY REFERENCES NhanVien(Id) ON DELETE SET NULL,
    ChiPhi DECIMAL(18,2) NOT NULL DEFAULT 0,
    NgayHoanTat DATETIME NULL,
    TrangThai NVARCHAR(50) NOT NULL DEFAULT 'DangXuLy' -- DangXuLy, HoanTiet, Huy
);

-- Lịch bảo trì định kỳ
CREATE TABLE LichBaoTriDinhKy (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    ThietBiId INT NOT NULL FOREIGN KEY REFERENCES ThietBi(Id) ON DELETE CASCADE,
    NgayBaoTriDuKien DATETIME NOT NULL,
    ChuKyBaoTri INT NOT NULL, -- Số ngày chu kỳ
    MoTaNoiDung NVARCHAR(500) NULL
);

-- Vật tư tiêu hao (trong kho)
CREATE TABLE VatTuTieuHao (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    TenVatTu NVARCHAR(150) NOT NULL,
    Dvt NVARCHAR(50) NOT NULL, -- cái, chai, hộp...
    SoLuongTon INT NOT NULL DEFAULT 0,
    DinhMucToiThieu INT NOT NULL DEFAULT 10,
    DonGia DECIMAL(18,2) NOT NULL
);

-- Hình thức thanh toán
CREATE TABLE HinhThucThanhToan (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    TenHinhThuc NVARCHAR(50) NOT NULL -- TienMat, ChuyenKhoan, The, ViDienTu, DirectBilling
);

-- Phiếu thanh toán
CREATE TABLE PhieuThanhToan (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    PhieuLuuTruId INT NULL FOREIGN KEY REFERENCES PhieuLuuTru(Id) ON DELETE SET NULL,
    PhieuSuDungDichVuId INT NULL FOREIGN KEY REFERENCES PhieuSuDungDichVu(Id),
    NgayThanhToan DATETIME NOT NULL DEFAULT GETDATE(),
    HinhThucThanhToanId INT NOT NULL FOREIGN KEY REFERENCES HinhThucThanhToan(Id),
    SoTien DECIMAL(18,2) NOT NULL,
    GhiChu NVARCHAR(250) NULL
);

-- Hóa đơn giá trị gia tăng (VAT)
CREATE TABLE HoaDon (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    SoHoaDon NVARCHAR(50) NOT NULL UNIQUE,
    PhieuLuuTruId INT NULL FOREIGN KEY REFERENCES PhieuLuuTru(Id) ON DELETE SET NULL,
    KhachHangId INT NOT NULL FOREIGN KEY REFERENCES KhachHang(Id),
    NgayXuat DATETIME NOT NULL DEFAULT GETDATE(),
    MST NVARCHAR(20) NULL,
    TenCongTy NVARCHAR(150) NULL,
    DiaChiCongTy NVARCHAR(250) NULL,
    ThueSuat FLOAT NOT NULL DEFAULT 0.08, -- 8%
    TienThue DECIMAL(18,2) NOT NULL,
    TongTien DECIMAL(18,2) NOT NULL,
    IsEInvoice BIT NOT NULL DEFAULT 1
);

-- Chi tiết hóa đơn
CREATE TABLE ChiTietHoaDon (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    HoaDonId INT NOT NULL FOREIGN KEY REFERENCES HoaDon(Id) ON DELETE CASCADE,
    TenMuc NVARCHAR(250) NOT NULL,
    SoLuong INT NOT NULL DEFAULT 1,
    DonGia DECIMAL(18,2) NOT NULL,
    ThanhTien DECIMAL(18,2) NOT NULL
);

--------------------------------------------------------------------------------
-- 3. CHÈN DỮ LIỆU MẪU (SEED DATA - 3 ĐẾN 5 BẢN GHI CHO MỖI BẢNG)
--------------------------------------------------------------------------------

-- Hạng thành viên
INSERT INTO HangThanhVien (TenHang, DoanhThuToiThieu, SoLuotLuuTruToiThieu, TiLeGiamGia, LoyaltyPointsRate) VALUES
('Member', 0, 0, 0, 0.05),
('Silver', 10000000, 5, 0.05, 0.07),
('Gold', 30000000, 15, 0.10, 0.10),
('Platinum', 70000000, 30, 0.15, 0.12),
('Diamond', 150000000, 50, 0.20, 0.15);

-- Khách hàng
INSERT INTO KhachHang (HoTen, NgaySinh, GioiTinh, QuocTich, CccdPassport, Sdt, Email, DiaChi, TenCongTy, MstCongTy, DiaChiCongTy, LoyaltyPoints, HangThanhVienId, IsDeleted) VALUES
(N'Nguyễn Văn A', '1990-05-15', N'Nam', N'Việt Nam', '012345678901', '0901234567', 'nguyenvana@gmail.com', N'Hà Nội', NULL, NULL, NULL, 120, 1, 0),
(N'Trần Thị B', '1995-10-20', N'Nữ', N'Việt Nam', '012345678902', '0902345678', 'tranthib@gmail.com', N'Đà Nẵng', NULL, NULL, NULL, 650, 2, 0),
(N'John Smith', '1985-02-12', N'Nam', N'Mỹ', 'B1234567', '0903456789', 'john.smith@gmail.com', N'New York', N'Tech Corp', '123456789', N'California, USA', 2000, 3, 0),
(N'Phạm Minh C', '1988-08-08', N'Nam', N'Việt Nam', '012345678903', '0904567890', 'phamminhc@gmail.com', N'TP. HCM', NULL, NULL, NULL, 0, 1, 0),
(N'Lê Hoàng D', '1992-12-25', N'Nam', N'Việt Nam', '012345678904', '0905678901', 'lehoangd@gmail.com', N'Cần Thơ', N'Vingroup', '0100789456', N'Hà Nội', 0, 1, 0);

-- Sở thích khách hàng
INSERT INTO SoThichKhachHang (KhachHangId, SoThich) VALUES
(1, N'Thích phòng yên tĩnh, hướng vườn'),
(2, N'Yêu cầu nệm mềm, uống trà xanh'),
(3, N'Không hút thuốc, phòng tầng cao');

-- Blacklist (Danh sách đen)
INSERT INTO DanhSachDen (KhachHangId, LyDo, NgayApDung, IsActive) VALUES
(4, N'Gây rối trật tự tại sảnh khách sạn và không thanh toán tiền dịch vụ mini-bar', '2026-01-10', 1);

-- Bộ phận nhân viên
INSERT INTO BoPhan (TenBoPhan) VALUES
(N'Lễ tân'),
(N'Buồng phòng'),
(N'Kỹ thuật'),
(N'F&B'),
(N'Quản lý');

-- Chức vụ
INSERT INTO ChucVu (TenChucVu, BoPhanId) VALUES
(N'Trưởng bộ phận Lễ tân', 1),
(N'Nhân viên Lễ tân', 1),
(N'Nhân viên Buồng phòng', 2),
(N'Kỹ sư Bảo trì', 3),
(N'Nhân viên Phục vụ Nhà hàng', 4),
(N'Giám đốc điều hành', 5);

-- Nhân viên
INSERT INTO NhanVien (MaNV, HoTen, NgaySinh, Cccd, Sdt, Email, BoPhanId, ChucVuId, NgayVaoLam, IsDeleted, AppUserId) VALUES
('NV001', N'Lê Văn Tám', '1985-03-24', '123456789123', '0911122233', 'levantam@hotel.com', 5, 6, '2020-01-01', 0, NULL),
('NV002', N'Nguyễn Thị Mai', '1993-07-12', '123456789124', '0911122234', 'nguyenthimai@hotel.com', 1, 2, '2022-05-10', 0, NULL),
('NV003', N'Trần Văn Khoa', '1990-11-30', '123456789125', '0911122235', 'tranvankhoa@hotel.com', 3, 4, '2021-08-15', 0, NULL),
('NV004', N'Phạm Thị Cúc', '1996-02-18', '123456789126', '0911122236', 'phamthicuc@hotel.com', 2, 3, '2023-02-01', 0, NULL),
('NV005', N'Hoàng Văn Nam', '1998-09-05', '123456789127', '0911122237', 'hoangvannam@hotel.com', 4, 5, '2024-03-01', 0, NULL);

-- Ca làm việc
INSERT INTO CaLamViec (TenCa, GioBatDau, GioKetThuc) VALUES
(N'Ca sáng', '06:00:00', '14:00:00'),
(N'Ca chiều', '14:00:00', '22:00:00'),
(N'Ca đêm', '22:00:00', '06:00:00');

-- Lịch làm việc & Chấm công
INSERT INTO LichLamViec (NhanVienId, CaLamViecId, NgayLamViec, CheckInTime, CheckOutTime, TrangThaiChambCong) VALUES
(2, 1, '2026-06-01', '2026-06-01 05:55:00', '2026-06-01 14:05:00', N'DungGio'),
(2, 1, '2026-06-02', '2026-06-02 06:10:00', '2026-06-02 14:00:00', N'Tre'),
(4, 2, '2026-06-01', '2026-06-01 13:50:00', '2026-06-01 22:05:00', N'DungGio'),
(3, 3, '2026-06-01', '2026-06-01 21:55:00', '2026-06-02 06:00:00', N'DungGio'),
(5, 1, '2026-06-01', NULL, NULL, N'Vang');

-- Loại phòng
INSERT INTO LoaiPhong (TenLoaiPhong, DienTich, SoGiuong, SucChua, HuongNhin, GiaNiemYet) VALUES
(N'Standard Single', 22.5, 1, 1, N'Hướng phố', 500000),
(N'Superior Double', 28.0, 1, 2, N'Hướng phố', 800000),
(N'Deluxe Twin', 35.0, 2, 2, N'Hướng vườn', 1200000),
(N'Executive Suite', 55.0, 1, 2, N'Hướng biển', 2500000),
(N'Presidential Suite', 120.0, 2, 4, N'Hướng biển toàn cảnh', 10000000);

-- Tiện nghi
INSERT INTO TienNghi (TenTienNghi, MoTa) VALUES
('Wifi', N'Kết nối wifi tốc độ cao miễn phí'),
('Air Conditioner', N'Điều hòa nhiệt độ độc lập'),
('Mini Bar', N'Tủ lạnh mini chứa đồ uống phát sinh phí'),
('Bathtub', N'Bồn tắm nằm cao cấp'),
('Smart TV', N'Tivi thông minh kết nối internet');

-- Loại phòng & Tiện nghi
INSERT INTO LoaiPhong_TienNghi (LoaiPhongId, TienNghiId) VALUES
(1, 1), (1, 2),
(2, 1), (2, 2), (2, 3),
(3, 1), (3, 2), (3, 3), (3, 5),
(4, 1), (4, 2), (4, 3), (4, 4), (4, 5),
(5, 1), (5, 2), (5, 3), (5, 4), (5, 5);

-- Phòng
INSERT INTO Phong (MaPhong, Tang, LoaiPhongId, TrangThai) VALUES
('P101', 1, 1, 'Trong-Sach'),
('P102', 1, 1, 'Trong-ChuaDon'),
('P201', 2, 2, 'Trong-Sach'),
('P202', 2, 2, 'DangSuDung'),
('P301', 3, 3, 'Trong-Sach'),
('P302', 3, 3, 'DaDat'),
('P401', 4, 4, 'DangSuDung'),
('P501', 5, 5, 'BaoTri');

-- Chính sách giá phòng (Rate Plan)
INSERT INTO ChinhSachGia (LoaiPhongId, TenChinhSach, NgayBatDau, NgayKetThuc, GiaApDung, SoDemToiThieu, IsActive) VALUES
(1, N'Giá mùa hè giảm giá', '2026-06-01', '2026-08-31', 450000, 1, 1),
(2, N'Giá khuyến mãi cuối tuần', '2026-01-01', '2026-12-31', 750000, 2, 1),
(4, N'Giá cao điểm lễ tết', '2026-12-20', '2027-01-05', 3000000, 1, 1);

-- Chính sách hủy đặt phòng
INSERT INTO ChinhSachHuy (MoTa, HanHuyTruoc, PhiHuy) VALUES
(N'Hủy miễn phí trước 24 giờ', 24, 0),
(N'Hủy muộn tính phí đêm đầu tiên', 24, 1.00), -- 100% của 1 đêm
(N'Không hoàn hủy', 0, 1.00);

-- Phiếu đặt phòng
INSERT INTO PhieuDatPhong (MaDatPhong, KhachHangId, NgayTao, NgayCheckIn, NgayCheckOut, SoLuongKhach, YeuCauDacBiet, TrangThai, ChinhSachHuyId, TongTienDuKien, TienDatCoc) VALUES
('BK260601001', 1, '2026-05-25', '2026-06-05', '2026-06-07', 1, N'Không ăn hành', 'DaXacNhan', 1, 900000, 200000),
('BK260601002', 2, '2026-05-26', '2026-06-10', '2026-06-12', 2, N'Phòng lãng mạn kỉ niệm', 'DaXacNhan', 2, 1600000, 0),
('BK260601003', 3, '2026-05-28', '2026-06-01', '2026-06-04', 2, N'Hỗ trợ đón sân bay', 'DaNhanPhong', 3, 7500000, 1000000),
('BK260601004', 5, '2026-05-29', '2026-06-15', '2026-06-20', 3, NULL, 'ChoXacNhan', 1, 6000000, 0);

-- Chi tiết đặt phòng
INSERT INTO ChiTietDatPhong (PhieuDatPhongId, LoaiPhongId, SoLuongPhong, GiaThoaThuan) VALUES
(1, 1, 1, 450000),
(2, 2, 1, 800000),
(3, 4, 1, 2500000),
(4, 3, 1, 1200000);

-- Phiếu lưu trú (Folio)
INSERT INTO PhieuLuuTru (MaFolio, PhieuDatPhongId, PhongId, KhachHangId, NgayCheckInAct, NgayCheckOutAct, PhuThuEarlyCheckIn, PhuThuLateCheckOut, GiamTru, TrangThai) VALUES
('FO260601001', 3, 7, 3, '2026-06-01 10:00:00', NULL, 300000, 0, 500000, 'DangLuuTru'),
('FO260601002', NULL, 4, 2, '2026-06-02 14:00:00', NULL, 0, 0, 0, 'DangLuuTru');

-- Đăng ký lưu trú
INSERT INTO DangKyLuuTru (PhieuLuuTruId, KhachHangId, NgayBatDau, NgayKetThuc) VALUES
(1, 3, '2026-06-01', '2026-06-04'),
(2, 2, '2026-06-02', '2026-06-04');

-- Lịch sử đổi phòng
INSERT INTO LichSuDoiPhong (PhieuLuuTruId, PhongCuId, PhongMoiId, NgayDoi, LyDo, NhanVienId) VALUES
(1, 3, 7, '2026-06-01 10:30:00', N'Khách nâng hạng lên Suite từ Superior', 2);

-- Lịch sử lưu trú
INSERT INTO LichSuLuuTru (KhachHangId, PhongId, NgayCheckIn, NgayCheckOut, TongTien) VALUES
(1, 1, '2026-05-10', '2026-05-12', 1000000),
(2, 2, '2026-05-15', '2026-05-18', 2400000);

-- Danh mục dịch vụ
INSERT INTO DanhMucDichVu (TenNhom) VALUES
('F&B'),
(N'Giặt ủi'),
('Spa & Wellness'),
(N'Vận chuyển'),
(N'Khác');

-- Dịch vụ
INSERT INTO DichVu (TenDichVu, DonGia, DonViTinh, DanhMucDichVuId) VALUES
(N'Bún bò Huế', 85000, N'Tô', 1),
(N'Nước cam ép', 45000, N'Ly', 1),
(N'Giặt quần jean', 25000, N'Cái', 2),
(N'Massage Thảo dược 60p', 350000, N'Liệu trình', 3),
(N'Xe đưa đón sân bay 4 chỗ', 400000, N'Lượt', 4);

-- Phiếu sử dụng dịch vụ
INSERT INTO PhieuSuDungDichVu (PhieuLuuTruId, PhongId, NgayTao, NhanVienId, DaThanhToanRieng, TongTien) VALUES
(1, 7, '2026-06-01 18:30:00', 5, 0, 130000),
(2, 4, '2026-06-02 09:00:00', 5, 1, 400000);

-- Chi tiết dịch vụ
INSERT INTO ChiTietDichVu (PhieuSuDungDichVuId, DichVuId, SoLuong, DonGia, ThanhTien, GhiChu) VALUES
(1, 1, 1, 85000, 85000, N'Phục vụ tại phòng 501 - Không cay'),
(1, 2, 1, 45000, 45000, N'Phục vụ kèm đá'),
(2, 5, 1, 400000, 400000, N'Đón ga quốc tế lúc 08:30');

-- Chi tiết Folio
INSERT INTO ChiTietFolio (PhieuLuuTruId, LoaiChiTiet, NoiDung, SoLuong, DonGia, ThanhTien, NgayGhiNhan) VALUES
(1, 'TienPhong', N'Tiền phòng Executive Suite (3 đêm)', 3, 2500000, 7500000, '2026-06-01'),
(1, 'PhuThu', N'Phụ thu Early Check-In', 1, 300000, 300000, '2026-06-01'),
(1, 'DichVu', N'Sử dụng F&B chiều 01/06', 1, 130000, 130000, '2026-06-01 18:30:00'),
(1, 'GiamTru', N'Giảm trừ voucher thành viên Gold', 1, 500000, 500000, '2026-06-01');

-- Loại thiết bị
INSERT INTO LoaiThietBi (TenLoai) VALUES
(N'Điện tử'),
(N'Điện lạnh'),
(N'Đồ gỗ nội thất'),
(N'Thiết bị vệ sinh');

-- Thiết bị cố định
INSERT INTO ThietBi (MaTaiSan, TenThietBi, NgayMua, NhaCungCap, HanBaoHanh, TrangThai, LoaiThietBiId) VALUES
('TB-LG55', 'Smart TV LG 55 inch', '2025-05-10', 'Dien May Xanh', '2027-05-10', 'Tot', 1),
('TB-DAIKIN12', 'Dieu hoa Daikin 12000 BTU', '2025-05-12', 'Nguyen Kim', '2027-05-12', 'Tot', 2),
('TB-BEDKING', 'Giuong Go Soi King Size', '2025-05-01', 'Noi That Hoa Phat', '2028-05-01', 'Tot', 3),
('TB-TOTO-WC', 'Bon Cau Thong Minh Toto', '2025-05-15', 'Toto Viet Nam', '2029-05-15', 'CanSuaChua', 4);

-- Phân bổ thiết bị trong phòng
INSERT INTO Phong_ThietBi (PhongId, ThietBiId, NgayBanGiao, SoLuong) VALUES
(7, 1, '2025-06-01', 1),
(7, 2, '2025-06-01', 1),
(7, 3, '2025-06-01', 1),
(7, 4, '2025-06-01', 1);

-- Lịch sử bảo trì
INSERT INTO LichSuBaoTri (ThietBiId, NgayBaoHong, MoTaSuCo, NhanVienKyThuatId, ChiPhi, NgayHoanTat, TrangThai) VALUES
(4, '2026-06-02 08:00:00', N'Rò rỉ nước ở dây cấp nước', 3, 50000, NULL, 'DangXuLy');

-- Lịch bảo trì định kỳ
INSERT INTO LichBaoTriDinhKy (ThietBiId, NgayBaoTriDuKien, ChuKyBaoTri, MoTaNoiDung) VALUES
(2, '2026-07-01', 90, N'Vệ sinh lưới lọc máy lạnh, đo ga định kỳ');

-- Vật tư tiêu hao
INSERT INTO VatTuTieuHao (TenVatTu, Dvt, SoLuongTon, DinhMucToiThieu, DonGia) VALUES
(N'Khăn tắm trắng 70x140', N'Cái', 120, 20, 45000),
(N'Bàn chải đánh răng', N'Cái', 500, 100, 3000),
(N'Nước suối Aquafina 350ml', N'Chai', 1000, 200, 5000),
(N'Dầu gội mini', N'Chai', 800, 150, 2500);

-- Hình thức thanh toán
INSERT INTO HinhThucThanhToan (TenHinhThuc) VALUES
(N'Tiền mặt'),
(N'Chuyển khoản'),
(N'Thẻ ngân hàng'),
(N'Ví điện tử (MoMo/ZaloPay)'),
(N'Direct Billing');

-- Phiếu thanh toán
INSERT INTO PhieuThanhToan (PhieuLuuTruId, PhieuSuDungDichVuId, NgayThanhToan, HinhThucThanhToanId, SoTien, GhiChu) VALUES
(NULL, 2, '2026-06-02 09:05:00', 4, 400000, N'Khách quét MoMo thanh toán xe đưa đón');

-- Hóa đơn VAT
INSERT INTO HoaDon (SoHoaDon, PhieuLuuTruId, KhachHangId, NgayXuat, MST, TenCongTy, DiaChiCongTy, ThueSuat, TienThue, TongTien, IsEInvoice) VALUES
('INV202606020001', NULL, 3, '2026-06-02 09:10:00', '123456789', N'Tech Corp', N'California, USA', 0.08, 32000, 432000, 1);

-- Chi tiết hóa đơn
INSERT INTO ChiTietHoaDon (HoaDonId, TenMuc, SoLuong, DonGia, ThanhTien) VALUES
(1, N'Dịch vụ xe đưa đón sân bay 4 chỗ', 1, 400000, 400000);
GO
